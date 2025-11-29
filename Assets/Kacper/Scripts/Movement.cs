using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public enum InputSource { Keyboard = 0, GamepadByIndex = 10 } 
    [Header("Input")]
    public bool useNewInputSystem = true; // ustaw false je�li nie masz New Input System
    public bool autoSelectLastUsedGamepad = true; // je�li true, wybierze pad kt�ry ostatnio go u�ywa�
    [Tooltip("Je�eli u�ywasz GamepadByIndex, wpisz indeks (0 = pierwszy pad w Gamepad.all)")]
    public int selectedGamepadIndex = 0; // u�ywane je�li chcesz wybra� pad r�cznie
    public bool useGamepadByIndex = false; // true = korzystaj z Gamepad.all[selectedGamepadIndex]

    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool smoothMovement = true;
    [Range(0.001f, 1f)]
    public float smoothing = 0.12f;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip stepSound1;
    public AudioClip stepSound2;
    [Tooltip("Minimalna prędkość, przy której odtwarzane są kroki.")]
    public float footstepMinSpeed = 0.1f;
    [Tooltip("Czas pomiędzy krokami w sekundach.")]
    public float footstepInterval = 0.4f;
    [Tooltip("Czas fade-out dla ostatniego kroku (echo w biurze). Jeśli 0, użyje długości klipu audio.")]
    public float lastStepFadeOutDuration = 0f;

    Rigidbody2D rb;
    Vector2 velocitySmoothRef = Vector2.zero;
    Vector2 targetVelocity = Vector2.zero;
    float footstepTimer = 0f;
    bool wasMoving = false;
    bool useFirstStep = true;
    Coroutine currentFadeCoroutine = null;
    float targetVolume = 1f;
    AudioClip lastPlayedClip = null;

#if ENABLE_INPUT_SYSTEM
    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // reaguj na pod��czenie/od��czenie pad�w
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"Gamepad added: {device.displayName}");
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"Gamepad removed: {device.displayName}");
                    break;
                case InputDeviceChange.Disconnected:
                    Debug.Log($"Gamepad disconnected: {device.displayName}");
                    break;
                case InputDeviceChange.Reconnected:
                    Debug.Log($"Gamepad reconnected: {device.displayName}");
                    break;
            }
        }
    }
#endif

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (useNewInputSystem && (useGamepadByIndex || autoSelectLastUsedGamepad))
        {
#if ENABLE_INPUT_SYSTEM
            // Auto-select ostatnio u�ywanego gamepada
            if (autoSelectLastUsedGamepad && Gamepad.current != null)
            {
                // Gamepad.current mo�e by� aktualizowany przez Input System gdy pad wy�le sygna�
                input = ReadFromGamepad(Gamepad.current);
            }
            else if (useGamepadByIndex)
            {
                // r�czny wyb�r po indeksie
                if (Gamepad.all.Count > selectedGamepadIndex && selectedGamepadIndex >= 0)
                {
                    input = ReadFromGamepad(Gamepad.all[selectedGamepadIndex]);
                }
                else
                {
                    // je�li brak pada o takim indeksie -> fallback
                    input = ReadFromKeyboardFallback();
                }
            }
            else
            {
                // fallback: keyboard
                input = ReadFromKeyboardFallback();
            }
#else
            // New Input System nie jest w kompilacji -> fallback
            input = ReadFromKeyboardFallback();
#endif
        }
        else
        {
            // u�ywamy klasycznego Input Managera
            input = ReadFromKeyboardFallback();
        }

        input = Vector2.ClampMagnitude(input, 1f);
        targetVelocity = input * moveSpeed;

        // Logika kroków
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        if (footstepSource == null) return;

        float speed = targetVelocity.magnitude;
        bool isMoving = speed >= footstepMinSpeed;

        if (isMoving)
        {
            // Zatrzymaj fade ostatniego kroku jeśli był uruchomiony
            if (!wasMoving && currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
                targetVolume = 1f;
                footstepSource.volume = 1f;
            }

            // Odliczaj timer
            footstepTimer -= Time.deltaTime;
            
            // Dostosuj prędkość kroków do prędkości ruchu
            float adjustedInterval = footstepInterval / Mathf.Clamp(speed / moveSpeed, 0.5f, 2f);
            
            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                footstepTimer = adjustedInterval;
            }
        }
        else if (wasMoving && !isMoving)
        {
            // Zatrzymano ruch - ostatni krok z długim echo
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            currentFadeCoroutine = StartCoroutine(FadeOutLastStep());
        }

        wasMoving = isMoving;
    }

    void PlayFootstep()
    {
        if (footstepSource == null) return;

        AudioClip clipToPlay = null;
        
        // Naprzemiennie wybierz między dwoma dźwiękami (1-2-1-2)
        if (stepSound1 != null && stepSound2 != null)
        {
            clipToPlay = useFirstStep ? stepSound1 : stepSound2;
            useFirstStep = !useFirstStep;
        }
        else if (stepSound1 != null)
        {
            clipToPlay = stepSound1;
        }
        else if (stepSound2 != null)
        {
            clipToPlay = stepSound2;
        }

        if (clipToPlay != null)
        {
            lastPlayedClip = clipToPlay;
            footstepSource.PlayOneShot(clipToPlay, targetVolume);
        }
    }

    IEnumerator FadeOutLastStep()
    {
        // Długie echo dla ostatniego kroku - płynne zanikanie targetVolume
        if (footstepSource == null) yield break;
        
        // Użyj długości ostatniego klipu jeśli lastStepFadeOutDuration = 0
        float fadeDuration = lastStepFadeOutDuration;
        if (fadeDuration <= 0f && lastPlayedClip != null)
        {
            fadeDuration = lastPlayedClip.length;
        }
        if (fadeDuration <= 0f)
        {
            fadeDuration = 1.2f; // fallback
        }
        
        float startVolume = targetVolume;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            // Użyj krzywej eksponencjalnej dla szybszego zanikania na początku
            float curve = t * t; // kwadratowa krzywa - szybszy fade
            targetVolume = Mathf.Lerp(startVolume, 0f, curve);
            yield return null;
        }
        
        targetVolume = 1f; // reset dla następnego ruchu
        currentFadeCoroutine = null;
    }

    void OnDestroy()
    {
        // Cleanup przy usuwaniu obiektu
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
    }

    Vector2 ReadFromKeyboardFallback()
    {
        Vector2 i = Vector2.zero;
        i.x = Input.GetAxisRaw("Horizontal");
        i.y = Input.GetAxisRaw("Vertical");
        return i;
    }

#if ENABLE_INPUT_SYSTEM
    Vector2 ReadFromGamepad(Gamepad gp)
    {
        if (gp == null) return Vector2.zero;

        Vector2 v = gp.leftStick.ReadValue();

        // fallback do d-pad je�li joystick bliski 0
        if (v.sqrMagnitude < 0.0001f)
        {
            v = gp.dpad.ReadValue();
        }

        // deadzone
        if (Mathf.Abs(v.x) < 0.05f) v.x = 0f;
        if (Mathf.Abs(v.y) < 0.05f) v.y = 0f;

        return v;
    }
#endif

    void FixedUpdate()
    {
        if (smoothMovement)
        {
            Vector2 newVel = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocitySmoothRef, smoothing);
            rb.velocity = newVel;
        }
        else
        {
            rb.velocity = targetVelocity;
        }
    }

    // Narz�dzie debuguj�ce: poka� list� pad�w i status
    void OnGUI()
    {
        int y = 10;
#if ENABLE_INPUT_SYSTEM
        GUI.Label(new Rect(10, y, 400, 20), $"New Input System enabled. Gamepad count: {Gamepad.all.Count}");
        y += 22;
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            var g = Gamepad.all[i];
            string s = $"Index {i}: {g.displayName} (layout: {g.layout})";
            if (useGamepadByIndex && selectedGamepadIndex == i) s += "  <- selected";
            GUI.Label(new Rect(10, y, 600, 20), s);
            y += 20;
        }

        GUI.Label(new Rect(10, y, 400, 20), $"Auto-select last used: {autoSelectLastUsedGamepad}  Use by index: {useGamepadByIndex}  Selected index: {selectedGamepadIndex}");
#else
        GUI.Label(new Rect(10, y, 400, 20), "New Input System not available - using old Input Manager (keyboard).");
#endif
    }

    // Mo�esz wywo�a� t� metod� z innego skryptu (np. menu) by ustawi� pad przez indeks:
    public void SelectGamepadByIndex(int index)
    {
#if ENABLE_INPUT_SYSTEM
        if (index >= 0 && index < Gamepad.all.Count)
        {
            selectedGamepadIndex = index;
            useGamepadByIndex = true;
            autoSelectLastUsedGamepad = false;
            Debug.Log($"Selected gamepad index {index}: {Gamepad.all[index].displayName}");
        }
        else
        {
            Debug.LogWarning($"No gamepad at index {index}. Count={Gamepad.all.Count}");
        }
#else
        Debug.LogWarning("Cannot select gamepad by index: New Input System not enabled in this build.");
#endif
    }

    // pomocnicza metoda zwracaj�ca list� nazw pad�w (do UI)
    public List<string> GetGamepadNames()
    {
        List<string> names = new List<string>();
#if ENABLE_INPUT_SYSTEM
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            names.Add($"Index {i}: {Gamepad.all[i].displayName}");
        }
#endif
        if (names.Count == 0) names.Add("No gamepads connected");
        return names;
    }

    // Reset w edytorze
    void Reset()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}

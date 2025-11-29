using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    #region INPUT SETTINGS
    public enum InputSource { Keyboard = 0, GamepadByIndex = 10 }

    [Header("Input")]
    public bool useNewInputSystem = true;
    public bool autoSelectLastUsedGamepad = true;
    [Tooltip("Jeżeli używasz GamepadByIndex, wpisz indeks (0 = pierwszy pad w Gamepad.all)")]
    public int selectedGamepadIndex = 0;
    public bool useGamepadByIndex = false;
    #endregion

    #region MOVEMENT SETTINGS
    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool smoothMovement = true;
    [Range(0.001f, 1f)]
    public float smoothing = 0.12f;
    #endregion

    #region FOOTSTEP SETTINGS
    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip stepSound1;
    public AudioClip stepSound2;
    [Tooltip("Minimalna prędkość, przy której odtwarzane są kroki.")]
    public float footstepMinSpeed = 0.1f;
    [Tooltip("Czas pomiędzy krokami w sekundach.")]
    public float footstepInterval = 0.4f;
    [Tooltip("Czas fade-out dla ostatniego kroku. Jeśli 0, użyje długości klipu audio.")]
    public float lastStepFadeOutDuration = 0f;
    #endregion

    #region ANIMATION
    [Header("Animator")]
    [SerializeField] private Animator anim;
    private bool isMoving = false;
    private bool isFacingFront = true;
    #endregion

    #region PRIVATE VARIABLES
    private Rigidbody2D rb;
    private Vector2 velocitySmoothRef = Vector2.zero;
    private Vector2 targetVelocity = Vector2.zero;

    private float footstepTimer = 0f;
    private bool wasMoving = false;
    private bool useFirstStep = true;
    private Coroutine currentFadeCoroutine = null;
    private float targetVolume = 1f;
    private AudioClip lastPlayedClip = null;

    [SerializeField] private bool isDead = false; 
    #endregion

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
        // --- BLOKADA RUCHU JEŚLI MARTWY ---
        if (isDead) return; 

        Vector2 input = GetInput();
        input = Vector2.ClampMagnitude(input, 1f);
        targetVelocity = input * moveSpeed;

        UpdateAnimation(input);   // aktualizacja parametrów Animatora
        HandleFootsteps();        // logika kroków
    }

    void FixedUpdate()
    {
        // --- ZATRZYMANIE FIZYKI JEŚLI MARTWY ---
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (smoothMovement)
        {
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocitySmoothRef, smoothing);
        }
        else
        {
            rb.velocity = targetVelocity;
        }
    }

    // --- NOWA METODA DO ZABIJANIA ---
    /// <summary>
    /// Wywołaj tę funkcję (np. z innego skryptu Health), aby zabić gracza.
    /// </summary>
    [ContextMenu("Kill Player (Test)")] // Pozwala testować w Inspectorze prawym przyciskiem myszy
    public void Die()
    {
        if (isDead) return; // Jeśli już nie żyje, nie rób tego ponownie

        isDead = true;
        targetVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;

        // Trigger animacji
        if (anim != null)
        {
            anim.SetTrigger("dead");
            anim.SetBool("moving", false); // Dla pewności wyłączamy chodzenie
        }

        // Zatrzymanie dźwięków kroków
        if (footstepSource != null) footstepSource.Stop();
        
        Debug.Log("Player is dead. Input disabled.");
    }

    // Opcjonalnie: Metoda do ożywiania
    [ContextMenu("Revive Player (Test)")]
    public void Revive()
    {
        isDead = false;
        if (anim != null) anim.Play("Idle"); // Lub inny stan początkowy
        Debug.Log("Player revived.");
    }

    #region INPUT METHODS
    private Vector2 GetInput()
    {
        Vector2 input = Vector2.zero;

        if (useNewInputSystem && (useGamepadByIndex || autoSelectLastUsedGamepad))
        {
#if ENABLE_INPUT_SYSTEM
            if (autoSelectLastUsedGamepad && Gamepad.current != null)
                input = ReadFromGamepad(Gamepad.current);
            else if (useGamepadByIndex)
            {
                if (Gamepad.all.Count > selectedGamepadIndex && selectedGamepadIndex >= 0)
                    input = ReadFromGamepad(Gamepad.all[selectedGamepadIndex]);
                else
                    input = ReadFromKeyboardFallback();
            }
            else
                input = ReadFromKeyboardFallback();
#else
            input = ReadFromKeyboardFallback();
#endif
        }
        else
            input = ReadFromKeyboardFallback();

        return input;
    }

    private Vector2 ReadFromKeyboardFallback()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

#if ENABLE_INPUT_SYSTEM
    private Vector2 ReadFromGamepad(Gamepad gp)
    {
        if (gp == null) return Vector2.zero;
        Vector2 v = gp.leftStick.ReadValue();
        if (v.sqrMagnitude < 0.0001f) v = gp.dpad.ReadValue();
        if (Mathf.Abs(v.x) < 0.05f) v.x = 0f;
        if (Mathf.Abs(v.y) < 0.05f) v.y = 0f;
        return v;
    }
#endif
    #endregion

    #region ANIMATION METHODS
    private void UpdateAnimation(Vector2 input)
    {
        isMoving = input.sqrMagnitude > 0.01f;

        // Facing front/back logic
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            isFacingFront = input.y <= 0; // dół = front (true), góra = back (false)

        if (anim != null)
        {
            anim.SetBool("moving", isMoving);
            anim.SetBool("facing_front", isFacingFront);
        }
    }
    #endregion

    #region FOOTSTEP METHODS
    private void HandleFootsteps()
    {
        if (footstepSource == null) return;

        float speed = targetVelocity.magnitude;
        bool currentlyMoving = speed >= footstepMinSpeed;

        // *** NOWE: Powiadom TimeController o ruchu gracza ***
        TimeController timeController = FindObjectOfType<TimeController>();
        if (timeController != null)
        {
            timeController.RegisterPlayerMovement(gameObject, currentlyMoving);
        }

        if (currentlyMoving)
        {
            if (!wasMoving && currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
                targetVolume = 1f;
                footstepSource.volume = 1f;
            }

            footstepTimer -= Time.deltaTime;
            float adjustedInterval = footstepInterval / Mathf.Clamp(speed / moveSpeed, 0.5f, 2f);
            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                footstepTimer = adjustedInterval;
            }
        }
        else if (wasMoving && !currentlyMoving)
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            currentFadeCoroutine = StartCoroutine(FadeOutLastStep());
        }

        wasMoving = currentlyMoving;
    }

    private void PlayFootstep()
    {
        if (footstepSource == null) return;

        AudioClip clipToPlay = null;
        if (stepSound1 != null && stepSound2 != null)
        {
            clipToPlay = useFirstStep ? stepSound1 : stepSound2;
            useFirstStep = !useFirstStep;
        }
        else if (stepSound1 != null) clipToPlay = stepSound1;
        else if (stepSound2 != null) clipToPlay = stepSound2;

        if (clipToPlay != null)
        {
            lastPlayedClip = clipToPlay;
            footstepSource.PlayOneShot(clipToPlay, targetVolume);
        }
    }

    private IEnumerator FadeOutLastStep()
    {
        if (footstepSource == null) yield break;

        float fadeDuration = lastStepFadeOutDuration;
        if (fadeDuration <= 0f && lastPlayedClip != null) fadeDuration = lastPlayedClip.length;
        if (fadeDuration <= 0f) fadeDuration = 1.2f;

        float startVolume = targetVolume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = (elapsed / fadeDuration);
            targetVolume = Mathf.Lerp(startVolume, 0f, t * t);
            yield return null;
        }

        targetVolume = 1f;
        currentFadeCoroutine = null;
    }
    #endregion

    #region PUBLIC METHODS
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
        else Debug.LogWarning($"No gamepad at index {index}. Count={Gamepad.all.Count}");
#else
        Debug.LogWarning("Cannot select gamepad by index: New Input System not enabled in this build.");
#endif
    }

    public List<string> GetGamepadNames()
    {
        List<string> names = new List<string>();
#if ENABLE_INPUT_SYSTEM
        for (int i = 0; i < Gamepad.all.Count; i++)
            names.Add($"Index {i}: {Gamepad.all[i].displayName}");
#endif
        if (names.Count == 0) names.Add("No gamepads connected");
        return names;
    }
    #endregion

    void OnDestroy()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);
    }

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
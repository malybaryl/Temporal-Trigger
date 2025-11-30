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
    
    // --- ZMIENNE DO FLIPOWANIA ---
    [Header("Rotation Settings")]
    [SerializeField] private bool isFacingRight = true; // Domyślnie zakładamy, że sprite patrzy w prawo
    
    [Tooltip("Zaznacz to, jeśli postać stojąc przodem obraca się odwrotnie do myszki. To naprawi problem.")]
    public bool fixFrontRotation = true; 
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
    
    // Kamera potrzebna do śledzenia myszki
    private Camera mainCamera; 

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
            // Opcjonalne logi pada
        }
    }
#endif

    void Awake()
    {
        // --- DODAJ TO NA POCZĄTKU ---
        PlayerState.Reset();
        // ----------------------------

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        mainCamera = Camera.main;
    }

    void Update()
    {
        // --- BLOKADA RUCHU JEŚLI MARTWY ---
       // --- PODMIEŃ WARUNEK NA GÓRZE ---
        if (PlayerState.IsDead) return; 
        // --------------------------------

        Vector2 input = GetInput();
        input = Vector2.ClampMagnitude(input, 1f);
        targetVelocity = input * moveSpeed;

        // 1. Aktualizacja animacji (chodzenie, przód/tył)
        // Musimy to zrobić PRZED obracaniem, żeby wiedzieć czy stoimy przodem (isFacingFront)
        UpdateAnimation(input);   

        // 2. Obracanie w stronę kursora (Flip) z poprawką
        HandleRotationToCursor();
        
        // 3. Logika kroków i TimeControllera
        HandleFootsteps();        
    }

    void FixedUpdate()
    {
        // --- ZATRZYMANIE FIZYKI JEŚLI MARTWY ---
        if (PlayerState.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        // --------------------------------

        if (smoothMovement)
        {
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocitySmoothRef, smoothing);
        }
        else
        {
            rb.velocity = targetVelocity;
        }
    }

    // --- POPRAWIONA METODA: OBRACANIE DO KURSORA ---
    private void HandleRotationToCursor()
    {
        if (mainCamera == null) return;

        Vector3 mousePosition = Vector3.zero;

        // Pobranie pozycji myszki
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        }
        else 
        {
             return; // Brak myszki
        }
#else
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
#endif

        // Sprawdzamy: Czy myszka jest po prawej stronie gracza?
        bool mouseIsRight = mousePosition.x > transform.position.x;
        
        // Domyślnie chcemy patrzeć tam gdzie myszka (czyli jeśli myszka po prawej -> patrzymy w prawo)
        bool shouldFaceRight = mouseIsRight;

        // --- FIX: NAPRAWA ODWROTNEGO OBRACANIA PRZY WIDOKU Z PRZODU ---
        if (fixFrontRotation && isFacingFront)
        {
            // Jeśli stoimy przodem i zaznaczyliśmy opcję naprawy,
            // odwracamy logikę "gdzie powinien patrzeć".
            shouldFaceRight = !shouldFaceRight;
        }
        // -------------------------------------------------------------

        // Teraz wykonujemy Flip tylko jeśli aktualny kierunek jest inny niż ten wyliczony
        if (shouldFaceRight && !isFacingRight)
        {
            Flip();
        }
        else if (!shouldFaceRight && isFacingRight)
        {
            Flip();
        }
    }

    // --- METODY PUBLICZNE ---

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    [ContextMenu("Kill Player (Test)")] 
    public void Die()
    {
        // Sprawdzamy globalny stan
        if (PlayerState.IsDead) return; 

        // Ustawiamy globalny stan na true -> to uruchomi Game Over
        PlayerState.IsDead = true;

        targetVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;

        // Trigger animacji
        if (anim != null)
        {
            anim.SetTrigger("dead");
            anim.SetBool("moving", false); 
        }

        if (footstepSource != null) footstepSource.Stop();
        
        Debug.Log("Player died. PlayerState.IsDead is now true.");
    }

    [ContextMenu("Revive Player (Test)")]
    public void Revive()
    {
        // Resetujemy globalny stan
        PlayerState.Reset();
        
        if (anim != null) anim.Play("Idle"); 
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

        // Ustawianie kierunku (Góra/Dół) na podstawie inputu klawiatury/pada
        // Zmieniamy isFacingFront tylko jeśli faktycznie jest wciśnięty przycisk góra/dół
        if (Mathf.Abs(input.y) > 0.1f) 
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                isFacingFront = input.y <= 0; // dół = front (true), góra = back (false)
            }
        }

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

        // TimeController został usunięty - TimeManagerScript automatycznie śledzi prędkość gracza

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

    #region PUBLIC METHODS (GAMEPAD)
    public void SelectGamepadByIndex(int index)
    {
#if ENABLE_INPUT_SYSTEM
        if (index >= 0 && index < Gamepad.all.Count)
        {
            selectedGamepadIndex = index;
            useGamepadByIndex = true;
            autoSelectLastUsedGamepad = false;
        }
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
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kontroluje pozycję celownika.
/// Zawiera logikę Auto-Aim i informuje broń, czy ma cel.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CrosshairController : MonoBehaviour
{
    [Header("Input Settings")]
    public bool useNewInputSystem = true;
    public float gamepadCrosshairDistance = 5f;

    [Header("Multiplayer Settings")]
    [Tooltip("Który pad steruje tym celownikiem? (0 = pierwszy, 1 = drugi itd.)")]
    public int gamepadIndex = 0;
    public bool useGamepadByIndex = true;
    
    [Header("Auto Aim Settings")]
    [Tooltip("Włącz/Wyłącz Auto Aim dla gamepada")]
    [SerializeField] private bool useGamepadAutoAim = true; 

    [Tooltip("Zasięg skanowania przeciwników")]
    [SerializeField] private float autoAimRange = 10f;

    [Tooltip("Jak szybko celownik przykleja się do wroga")]
    [SerializeField] private float autoAimSmoothing = 15f;
    
    [Header("References")]
    public Transform playerTransform;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isUsingGamepad = false;
    private SpriteRenderer spriteRenderer;
    
    // --- NOWA ZMIENNA: Czy aktualnie namierzamy wroga? ---
    private bool hasActiveTarget = false;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null) Debug.LogError("CrosshairController: Brak Main Camera!");

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        Cursor.visible = false;
    }

    void Update()
    {
        UpdateCrosshairPosition();
    }

    void UpdateCrosshairPosition()
    {
        if (mainCamera == null) return;

        if (useNewInputSystem)
        {
#if ENABLE_INPUT_SYSTEM
            Gamepad myGamepad = GetGamepad();

            if (myGamepad != null)
            {
                Vector2 rightStick = myGamepad.rightStick.ReadValue();
                
                if (rightStick.magnitude > 0.1f || useGamepadAutoAim) 
                {
                    isUsingGamepad = true;
                    HandleGamepadInput(rightStick);
                    return;
                }
            }
            
            if (gamepadIndex == 0)
            {
                isUsingGamepad = false;
                HandleMouseInput();
            }
            else
            {
                // Gracz 2 bez inputu -> brak celu
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                hasActiveTarget = false;
            }
#else
            HandleMouseInput();
#endif
        }
        else
        {
            HandleMouseInput();
        }
    }

    Gamepad GetGamepad()
    {
#if ENABLE_INPUT_SYSTEM
        if (useGamepadByIndex)
        {
            if (Gamepad.all.Count > gamepadIndex) return Gamepad.all[gamepadIndex];
            return null;
        }
        return Gamepad.current;
#endif
        return null;
    }

    void HandleMouseInput()
    {
        // Mysz zawsze "ma cel" (celujesz ręcznie)
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        hasActiveTarget = true; 

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
        targetPosition.z = 0f;
        transform.position = targetPosition;
    }

    void HandleGamepadInput(Vector2 stickInput)
    {
        if (playerTransform == null) return;

        Transform closestEnemy = null;
        if (useGamepadAutoAim)
        {
            closestEnemy = GetClosestEnemy();
        }

        if (closestEnemy != null)
        {
            // MAMY CEL
            hasActiveTarget = true;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            
            Vector3 enemyPos = closestEnemy.position;
            enemyPos.z = 0f;
            transform.position = Vector3.Lerp(transform.position, enemyPos, Time.deltaTime * autoAimSmoothing);
        }
        else
        {
            // BRAK CELU
            hasActiveTarget = false;
            
            // Jeśli nie ma celu, ukrywamy celownik
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            
            // Celownik wraca do gracza
            transform.position = playerTransform.position;
        }
    }

    Transform GetClosestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerTransform.position, autoAimRange);
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPos = playerTransform.position;

        foreach (Collider2D col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Vector3 directionToTarget = col.transform.position - currentPos;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = col.transform;
                }
            }
        }
        return bestTarget;
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, autoAimRange);
        }
    }

    // --- PUBLICZNE METODY DLA WEAPON CONTROLLERA ---
    public Vector3 GetCrosshairPosition() => transform.position;
    public bool IsUsingGamepad() => isUsingGamepad;
    
    /// <summary>
    /// Czy celownik aktualnie widzi cel (lub jest myszką)?
    /// </summary>
    public bool HasActiveTarget() => hasActiveTarget;

    /// <summary>
    /// Czy tryb Auto-Aim jest włączony w ustawieniach?
    /// </summary>
    public bool IsAutoAimEnabled() => useGamepadAutoAim;

    void OnDestroy() => Cursor.visible = true;
    void OnApplicationFocus(bool hasFocus) { if (hasFocus) Cursor.visible = false; }
}
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kontroluje pozycję celownika.
/// Na gamepadzie działa jak Target Lock: pojawia się tylko gdy wykryje wroga.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // Wymaga komponentu SpriteRenderer do znikania
public class CrosshairController : MonoBehaviour
{
    [Header("Input Settings")]
    public bool useNewInputSystem = true;
    public float gamepadCrosshairDistance = 5f;
    
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
    private SpriteRenderer spriteRenderer; // Do ukrywania celownika

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
            if (Gamepad.current != null)
            {
                Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
                
                // Gamepad jest aktywny jeśli ruszamy gałką LUB auto-aim jest włączony
                if (rightStick.magnitude > 0.1f || useGamepadAutoAim) 
                {
                    isUsingGamepad = true;
                    HandleGamepadInput(rightStick);
                    return;
                }
            }
            // Jeśli nie gamepad, to mysz
            isUsingGamepad = false;
            HandleMouseInput();
#else
            HandleMouseInput();
#endif
        }
        else
        {
            HandleMouseInput();
        }
    }

    void HandleMouseInput()
    {
        // MYSZKA: Zawsze pokazuj celownik
        if (spriteRenderer != null) spriteRenderer.enabled = true;

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

        // Szukamy wroga tylko jeśli auto-aim jest włączony
        if (useGamepadAutoAim)
        {
            closestEnemy = GetClosestEnemy();
        }

        if (closestEnemy != null)
        {
            // SYTUACJA 1: Znaleziono wroga
            // Pokaż celownik
            if (spriteRenderer != null) spriteRenderer.enabled = true;

            // Namierzanie wroga
            Vector3 enemyPos = closestEnemy.position;
            enemyPos.z = 0f;
            
            // Płynny ruch do wroga
            transform.position = Vector3.Lerp(transform.position, enemyPos, Time.deltaTime * autoAimSmoothing);
        }
        else
        {
            // SYTUACJA 2: Brak wroga w zasięgu
            // Ukryj celownik (zrób go niewidzialnym)
            if (spriteRenderer != null) spriteRenderer.enabled = false;

            // Opcjonalnie: Trzymaj celownik na graczu (niewidoczny), żeby startował z dobrej pozycji
            transform.position = playerTransform.position;
        }
    }

    /// <summary>
    /// Skanuje otoczenie i zwraca transform najbliższego wroga z TAGIEM "Enemy"
    /// </summary>
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

    public Vector3 GetCrosshairPosition()
    {
        return transform.position;
    }

    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    void OnDestroy() => Cursor.visible = true;
    void OnApplicationFocus(bool hasFocus) { if (hasFocus) Cursor.visible = false; }
}
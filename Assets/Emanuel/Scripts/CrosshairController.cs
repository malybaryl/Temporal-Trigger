using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CrosshairController : MonoBehaviour
{
    [Header("Input Settings")]
    public bool useNewInputSystem = true;
    public float gamepadCrosshairDistance = 5f;
    
    [Header("Auto Aim Settings")]
    [Tooltip("Włącz/Wyłącz Auto Aim dla gamepada")]
    [SerializeField] private bool useGamepadAutoAim = true; 

    // USUNIĘTO: private LayerMask enemyLayer (już niepotrzebne)

    [Tooltip("Zasięg skanowania przeciwników")]
    [SerializeField] private float autoAimRange = 10f;

    [Tooltip("Jak szybko celownik przykleja się do wroga")]
    [SerializeField] private float autoAimSmoothing = 15f;
    
    [Header("References")]
    public Transform playerTransform;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isUsingGamepad = false;

    void Start()
    {
        mainCamera = Camera.main;
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
                
                if (rightStick.magnitude > 0.1f || useGamepadAutoAim) 
                {
                    isUsingGamepad = true;
                    HandleGamepadInput(rightStick);
                    return;
                }
            }
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
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
        targetPosition.z = 0f;
        transform.position = targetPosition;
    }

    void HandleGamepadInput(Vector2 stickInput)
    {
        if (playerTransform == null) return;

        Vector3 finalPosition = transform.position;
        Transform closestEnemy = null;

        if (useGamepadAutoAim)
        {
            closestEnemy = GetClosestEnemy();
        }

        if (closestEnemy != null)
        {
            // Namierzanie wroga
            Vector3 enemyPos = closestEnemy.position;
            enemyPos.z = 0f;
            finalPosition = Vector3.Lerp(transform.position, enemyPos, Time.deltaTime * autoAimSmoothing);
        }
        else
        {
            // Manualne sterowanie (brak wroga)
            if (stickInput.magnitude > 0.1f)
            {
                Vector3 direction = new Vector3(stickInput.x, stickInput.y, 0f).normalized;
                Vector3 manualPos = playerTransform.position + direction * gamepadCrosshairDistance;
                manualPos.z = 0f;
                finalPosition = Vector3.Lerp(transform.position, manualPos, Time.deltaTime * autoAimSmoothing);
            }
        }

        transform.position = finalPosition;
    }

    /// <summary>
    /// Skanuje otoczenie i zwraca transform najbliższego wroga z TAGIEM "Enemy"
    /// </summary>
    Transform GetClosestEnemy()
    {
        // 1. Pobierz WSZYSTKIE collidery w zasięgu (bez filtrowania warstwą)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerTransform.position, autoAimRange);

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPos = playerTransform.position;

        foreach (Collider2D col in hitColliders)
        {
            // 2. Tutaj sprawdzamy TAG
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
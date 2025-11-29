using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kontroluje pozycję celownika - śledzi mysz lub gamepad stick
/// Dodaj ten skrypt do obiektu Crosshair na scenie
/// </summary>
public class CrosshairController : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Czy używać New Input System (true) czy klasycznego Input Manager (false)")]
    public bool useNewInputSystem = true;
    
    [Tooltip("Dystans celownika od gracza przy użyciu gamepada")]
    public float gamepadCrosshairDistance = 5f;
    
    [Header("References")]
    [Tooltip("Referencja do gracza - ustawi się automatycznie jeśli pozostawione puste")]
    public Transform playerTransform;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isUsingGamepad = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CrosshairController: Brak Main Camera w scenie!");
        }

        // Automatycznie znajdź gracza po tagu
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("CrosshairController: Nie znaleziono gracza z tagiem 'Player'");
            }
        }

        // Ukryj systemowy kursor myszy
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
            // Sprawdź czy gamepad jest aktywny
            if (Gamepad.current != null)
            {
                Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
                
                // Jeśli prawy stick jest używany
                if (rightStick.magnitude > 0.1f)
                {
                    isUsingGamepad = true;
                    HandleGamepadInput(rightStick);
                    return;
                }
            }
            
            // Fallback do myszy
            isUsingGamepad = false;
            HandleMouseInput();
#else
            HandleMouseInput();
#endif
        }
        else
        {
            // Klasyczny Input Manager
            HandleMouseInput();
        }
    }

    void HandleMouseInput()
    {
        // Konwertuj pozycję myszy na pozycję w świecie gry
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
        targetPosition.z = 0f; // Upewnij się że z = 0 dla 2D
        
        transform.position = targetPosition;
    }

    void HandleGamepadInput(Vector2 stickInput)
    {
        if (playerTransform == null)
        {
            // Fallback do środka ekranu jeśli brak gracza
            targetPosition = mainCamera.transform.position;
            targetPosition.z = 0f;
            transform.position = targetPosition;
            return;
        }

        // Celownik porusza się względem pozycji gracza
        Vector3 direction = new Vector3(stickInput.x, stickInput.y, 0f).normalized;
        targetPosition = playerTransform.position + direction * gamepadCrosshairDistance;
        targetPosition.z = 0f;
        
        transform.position = targetPosition;
    }

    /// <summary>
    /// Zwraca aktualną pozycję celownika
    /// </summary>
    public Vector3 GetCrosshairPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Czy aktualnie używany jest gamepad
    /// </summary>
    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    void OnDestroy()
    {
        // Przywróć kursor przy usunięciu skryptu
        Cursor.visible = true;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Ukryj kursor gdy gra ma focus
        if (hasFocus)
        {
            Cursor.visible = false;
        }
    }
}
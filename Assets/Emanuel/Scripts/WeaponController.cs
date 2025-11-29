using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Odpowiada za strzelanie.
/// BLOKUJE strzał, jeśli włączony jest Auto-Aim, a w pobliżu nie ma wrogów.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Który gamepad steruje tą postacią? (0 = Gracz 1, 1 = Gracz 2)")]
    public int gamepadIndex = 0;
    public bool useGamepadByIndex = true;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;

    [Header("References")]
    public CrosshairController crosshairController;

    private float nextFireTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (firePoint == null)
        {
            firePoint = transform; 
            Debug.LogWarning("WeaponController: Brak Fire Point! Strzelam ze środka gracza.");
        }

        if (crosshairController == null)
        {
            crosshairController = FindObjectOfType<CrosshairController>();
        }
    }

    void Update()
    {
        HandleShooting();
    }

    void HandleShooting()
    {
        bool shootInput = false;

#if ENABLE_INPUT_SYSTEM
        Gamepad myGamepad = GetGamepad();
        
        // --- SPRAWDZENIE BLOKADY STRZAŁU ---
        // Sprawdzamy czy celownik pozwala nam strzelić
        if (crosshairController != null)
        {
            // Jeśli:
            // 1. Używamy Gamepada (IsUsingGamepad)
            // 2. Auto Aim jest włączony (IsAutoAimEnabled)
            // 3. I NIE MAMY CELU (!HasActiveTarget)
            // ... to blokujemy strzelanie.
            if (crosshairController.IsUsingGamepad() && 
                crosshairController.IsAutoAimEnabled() && 
                !crosshairController.HasActiveTarget())
            {
                // RETURN - przerywamy funkcję, nie sprawdzamy inputu
                return; 
            }
        }
        // -----------------------------------

        if (myGamepad != null)
        {
            if (myGamepad.rightShoulder.isPressed) 
            {
                shootInput = true;
            }
        }
        
        if (gamepadIndex == 0 && !shootInput && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            shootInput = true;
        }
#else
        if (Input.GetButton("Fire1")) shootInput = true;
#endif

        if (shootInput && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    Gamepad GetGamepad()
    {
#if ENABLE_INPUT_SYSTEM
        if (useGamepadByIndex)
        {
            if (Gamepad.all.Count > gamepadIndex)
            {
                return Gamepad.all[gamepadIndex];
            }
            return null;
        }
        return Gamepad.current;
#endif
        return null;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 targetPos = Vector3.zero;

        if (crosshairController != null)
        {
            targetPos = crosshairController.GetCrosshairPosition();
        }
        else
        {
             Vector3 mousePos = Input.mousePosition;
             mousePos.z = -mainCamera.transform.position.z;
             targetPos = mainCamera.ScreenToWorldPoint(mousePos);
        }

        Vector2 direction = (targetPos - firePoint.position).normalized;
        
        // Tworzenie pocisku
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Inicjalizacja pocisku (przekazanie kierunku do BulletController)
        BulletController bulletScript = bullet.GetComponent<BulletController>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction);
        }
        else
        {
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = direction * bulletForce;
        }
    }
}
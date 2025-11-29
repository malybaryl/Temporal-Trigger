using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kontroluje strzelanie gracza - wykrywa input, tworzy pociski
/// WERSJA DEBUG - Rozbudowane logi do diagnozowania problemów
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Prefab pocisku do wystrzeliwania")]
    public GameObject bulletPrefab;
    
    [Tooltip("Punkt z którego wystrzeliwane są pociski")]
    public Transform firePoint;
    
    [Tooltip("Bazowy czas między strzałami gdy gracz STOI")]
    public float baseFireRate = 0.5f;
    
    [Tooltip("Minimalny czas między strzałami gdy gracz BIEGNIE")]
    public float minFireDelay = 0.2f;
    
    [Tooltip("Czy używać New Input System")]
    public bool useNewInputSystem = true;

    [Header("Multiplayer Settings")]
    [Tooltip("Który pad steruje tą bronią? (0 = Gracz 1, 1 = Gracz 2)")]
    public int gamepadIndex = 0;
    public bool useGamepadByIndex = true;

    [Header("References")]
    [Tooltip("Referencja do celownika")]
    public CrosshairController crosshair;

    [Header("Audio")]
    [Tooltip("Dźwięk strzału")]
    public AudioClip shootSound;
    
    [Header("Debug")]
    [Tooltip("Pokaż debug logi w Console")]
    public bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private float lastFireTime = 0f;
    private float fireRateCooldown = 0f;
    private bool canShoot = true;

    void Start()
    {
        DebugLog("=== WeaponController START ===");
        
        if (crosshair == null)
        {
            crosshair = FindObjectOfType<CrosshairController>();
            DebugLog($"Crosshair auto-found: {crosshair != null}");
        }

        if (firePoint == null)
        {
            firePoint = transform;
            DebugLog("FirePoint = player position");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        DebugLog($"Bullet Prefab assigned: {bulletPrefab != null}");
        DebugLog($"Crosshair assigned: {crosshair != null}");
        DebugLog("=== WeaponController READY ===");
    }

    void Update()
    {
        // Zmniejsz cooldown
        if (fireRateCooldown > 0f)
        {
            fireRateCooldown -= Time.deltaTime * GameTime.timescale;
        }

        HandleShootInput();
    }

    void HandleShootInput()
    {
        bool canFireNow = fireRateCooldown <= 0f && canShoot;
        bool shootPressed = false;

        // === DEBUG: Sprawdź input ===
        bool lpmClassic = Input.GetMouseButtonDown(0);
        
#if ENABLE_INPUT_SYSTEM
        bool lpmNewSystem = false;
        bool rtGamepad = false;

        // Sprawdź mysz (tylko dla Gracza 1 - gamepadIndex 0)
        if (gamepadIndex == 0 && Mouse.current != null)
        {
            lpmNewSystem = Mouse.current.leftButton.wasPressedThisFrame;
        }

        // Sprawdź gamepad (konkretny pad dla tego gracza)
        Gamepad myGamepad = GetGamepad();
        if (myGamepad != null)
        {
            rtGamepad = myGamepad.rightTrigger.wasPressedThisFrame;
        }
        
        if (lpmClassic || lpmNewSystem || rtGamepad)
        {
            DebugLog($"[INPUT DETECTED] Classic: {lpmClassic}, NewSystem: {lpmNewSystem}, Gamepad RT: {rtGamepad}, Pad Index: {gamepadIndex}");
            shootPressed = true;
        }
#else
        if (lpmClassic)
        {
            DebugLog($"[INPUT DETECTED] Classic LPM");
            shootPressed = true;
        }
#endif

        // === DEBUG: Jeśli wciśnięto przycisk ===
        if (shootPressed)
        {
            DebugLog($"[SHOOT ATTEMPT] canFireNow: {canFireNow}, cooldown: {fireRateCooldown:F3}, timescale: {GameTime.timescale:F3}");
            
            if (!canFireNow)
            {
                if (fireRateCooldown > 0f)
                {
                    DebugLog($"[BLOCKED] Cooldown active: {fireRateCooldown:F2}s");
                }
                else if (!canShoot)
                {
                    DebugLog($"[BLOCKED] canShoot = false");
                }
                return;
            }

            // === Sprawdź crosshair ===
            if (crosshair == null)
            {
                DebugLog("[ERROR] Crosshair is NULL!");
                return;
            }

            // === Sprawdź czy crosshair ma cel (dla gamepada) ===
            if (crosshair.IsUsingGamepad())
            {
                if (crosshair.IsAutoAimEnabled() && !crosshair.HasActiveTarget())
                {
                    DebugLog("[BLOCKED] Gamepad Auto-Aim: No target in range");
                    return;
                }
            }

            // === Sprawdź bullet prefab ===
            if (bulletPrefab == null)
            {
                DebugLog("[ERROR] Bullet Prefab is NULL!");
                return;
            }

            // === STRZAŁ ===
            Shoot();
        }
    }

    void Shoot()
    {
        DebugLog("=== SHOOTING ===");

        Vector2 direction = (crosshair.GetCrosshairPosition() - firePoint.position).normalized;
        DebugLog($"Direction: {direction}");

        // Stwórz pocisk
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        DebugLog($"Bullet created: {bullet != null}");

        // Inicjalizuj
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(direction);
            DebugLog("BulletController.Initialize() called");
        }
        else
        {
            DebugLog("[WARNING] No BulletController on prefab!");
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * 15f * GameTime.timescale;
                DebugLog($"Manual velocity set: {bulletRb.velocity}");
            }
        }

        // Audio
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Cooldown
        float calculatedCooldown = Mathf.Lerp(baseFireRate, minFireDelay, GameTime.timescale);
        fireRateCooldown = calculatedCooldown;
        lastFireTime = Time.time;

        DebugLog($"Cooldown set to: {calculatedCooldown:F2}s | timescale: {GameTime.timescale:F2}");
        DebugLog("=== SHOT COMPLETE ===");
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
        DebugLog($"canShoot set to: {value}");
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
#else
        return null;
#endif
    }

    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[WEAPON P{gamepadIndex + 1}] {message}");
        }
    }

    void OnDrawGizmos()
    {
        if (crosshair != null && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, crosshair.GetCrosshairPosition());
        }
    }
}
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kontroluje strzelanie gracza - wykrywa input, tworzy pociski
/// Dodaj ten skrypt do prefaba Player
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Prefab pocisku do wystrzeliwania")]
    public GameObject bulletPrefab;
    
    [Tooltip("Punkt z którego wystrzeliwane są pociski (opcjonalnie, jeśli null = środek gracza)")]
    public Transform firePoint;
    
    [Tooltip("Bazowy czas między strzałami gdy gracz STOI (timescale ~0)")]
    public float baseFireRate = 0.5f;
    
    [Tooltip("Minimalny czas między strzałami gdy gracz BIEGNIE (timescale = 1)")]
    public float minFireDelay = 0.2f;
    
    [Tooltip("Czy używać New Input System")]
    public bool useNewInputSystem = true;

    [Header("References")]
    [Tooltip("Referencja do celownika - znajdzie automatycznie jeśli puste")]
    public CrosshairController crosshair;

    [Header("Audio - Opcjonalnie")]
    [Tooltip("Dźwięk strzału (opcjonalnie)")]
    public AudioClip shootSound;
    
    private AudioSource audioSource;
    private float lastFireTime = 0f;
    private float fireRateCooldown = 0f;
    private bool canShoot = true;

    void Start()
    {
        Debug.Log("WeaponController: Start() called");
        
        // Znajdź celownik automatycznie
        if (crosshair == null)
        {
            crosshair = FindObjectOfType<CrosshairController>();
            if (crosshair == null)
            {
                Debug.LogError("WeaponController: Nie znaleziono CrosshairController w scenie!");
            }
            else
            {
                Debug.Log("WeaponController: Znaleziono CrosshairController automatycznie");
            }
        }

        // Jeśli brak firePoint, użyj pozycji gracza
        if (firePoint == null)
        {
            firePoint = transform;
            Debug.Log("WeaponController: Używam pozycji gracza jako firePoint");
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Sprawdź czy bulletPrefab jest przypisany
        if (bulletPrefab == null)
        {
            Debug.LogError("WeaponController: Bullet Prefab NIE JEST PRZYPISANY! Przypisz go w Inspectorze!");
        }
        else
        {
            Debug.Log($"WeaponController: Bullet Prefab przypisany: {bulletPrefab.name}");
        }
    }

    void Update()
    {
        // KLUCZOWE: Zmniejsz cooldown każdą klatkę (skalowane przez GameTime.timescale)
        if (fireRateCooldown > 0f)
        {
            // Cooldown zmniejsza się szybciej gdy się ruszasz (GameTime.timescale wyższy)
            fireRateCooldown -= Time.deltaTime * GameTime.timescale;
        }

        HandleShootInput();
    }

    void HandleShootInput()
    {
        // Sprawdź czy cooldown minął
        bool canFireNow = fireRateCooldown <= 0f && canShoot;
        
        bool shootPressed = false;

        // ZAWSZE sprawdź klasyczny input jako podstawę (działa zawsze)
        if (Input.GetMouseButtonDown(0)) // 0 = LPM
        {
            shootPressed = true;
            Debug.Log("WeaponController: Wykryto LPM (Input.GetMouseButtonDown)");
        }

        // Dodatkowo sprawdź New Input System jeśli włączony
        if (!shootPressed && useNewInputSystem)
        {
#if ENABLE_INPUT_SYSTEM
            // Sprawdź input z myszy
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                shootPressed = true;
                Debug.Log("WeaponController: Wykryto LPM (New Input System)");
            }
            
            // Sprawdź input z gamepada
            if (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                shootPressed = true;
                Debug.Log("WeaponController: Wykryto RT gamepad");
            }
#endif
        }

        if (shootPressed && canFireNow)
        {
            Shoot();
        }
        else if (shootPressed && !canFireNow)
        {
            if (fireRateCooldown > 0f)
            {
                Debug.Log($"WeaponController: Cooldown aktywny ({fireRateCooldown:F2}s pozostało)");
            }
            else if (!canShoot)
            {
                Debug.Log("WeaponController: Nie można strzelać (canShoot = false)");
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("WeaponController: Nie można strzelić - brak Bullet Prefab!");
            return;
        }

        if (crosshair == null)
        {
            Debug.LogError("WeaponController: Nie można strzelić - brak Crosshair!");
            return;
        }

        Vector2 direction = (crosshair.transform.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        if (bulletController != null)
        {
            bulletController.Initialize(direction);
        }
        else
        {
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * 15f * GameTime.timescale;
            }
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Ustaw cooldown: Im wyższy timescale (szybciej się ruszasz), tym KRÓTSZY cooldown
        // timescale = 0.0 (stoisz) → cooldown = baseFireRate (0.5s) = WOLNO
        // timescale = 1.0 (biegniesz) → cooldown = minFireDelay (0.2s) = SZYBKO
        float calculatedCooldown = Mathf.Lerp(baseFireRate, minFireDelay, GameTime.timescale);
        fireRateCooldown = calculatedCooldown;
        
        lastFireTime = Time.time;
        Debug.Log($"Shot fired! Cooldown: {calculatedCooldown:F2}s | Player speed (timescale): {GameTime.timescale:F2}");
    }

    /// <summary>
    /// Włącz/wyłącz możliwość strzelania (np. podczas przeładowania)
    /// </summary>
    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    // Debug - rysuj linię strzału w edytorze
    void OnDrawGizmos()
    {
        if (crosshair != null && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, crosshair.GetCrosshairPosition());
        }
    }
}
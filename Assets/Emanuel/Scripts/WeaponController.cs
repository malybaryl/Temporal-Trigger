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
    
    [Tooltip("Czas między strzałami w sekundach")]
    public float fireRate = 0.2f;
    
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
        HandleShootInput();
    }

    void HandleShootInput()
    {
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

        if (shootPressed && canShoot)
        {
            Shoot();
        }
        else if (shootPressed && !canShoot)
        {
            Debug.Log("WeaponController: Nie można strzelać (canShoot = false)");
        }
    }

    void Shoot()
    {
        // Sprawdź cooldown
        if (Time.time - lastFireTime < fireRate)
        {
            return;
        }

        if (bulletPrefab == null || crosshair == null)
        {
            Debug.LogWarning("WeaponController: Brak bulletPrefab lub crosshair!");
            return;
        }

        // Oblicz kierunek strzału (od gracza do celownika)
        Vector3 shootDirection = (crosshair.GetCrosshairPosition() - firePoint.position).normalized;

        // Stwórz pocisk
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // Zainicjalizuj pocisk
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(shootDirection);
        }
        else
        {
            Debug.LogError("WeaponController: Bullet prefab nie ma BulletController!");
        }

        // Odtwórz dźwięk strzału
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Zapisz czas ostatniego strzału
        lastFireTime = Time.time;

        // Debug info
        Debug.Log($"Shot fired at {Time.time}");
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
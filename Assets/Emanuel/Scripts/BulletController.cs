using UnityEngine;

/// <summary>
/// Kontroluje zachowanie pocisku - ruch, kolizje, niszczenie
/// Dodaj ten skrypt do prefaba Bullet
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Prędkość pocisku")]
    public float bulletSpeed = 15f;
    
    [Tooltip("Czas życia pocisku w sekundach (0 = nieskończony)")]
    public float lifetime = 5f;
    
    [Tooltip("Czy pocisk niszczy się po trafieniu")]
    public bool destroyOnHit = true;

    [Header("Effects - Opcjonalnie")]
    [Tooltip("Prefab efektu przy trafieniu (opcjonalnie)")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
    }

    /// <summary>
    /// Inicjalizuje pocisk z kierunkiem lotu
    /// Wywoływane przez WeaponController przy tworzeniu pocisku
    /// </summary>
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        
        // Ustaw prędkość pocisku
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        // Obróć pocisk w kierunku lotu (opcjonalnie, dla wizualnego efektu)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // Pocisk porusza się tylko gdy czas płynie (Time.timeScale > 0)
        // Unity automatycznie obsługuje to przez Rigidbody2D.velocity
        
        // Zniszcz pocisk po upływie lifetime (używa Time.time który respektuje timeScale)
        if (lifetime > 0 && Time.time - spawnTime >= lifetime)
        {
            DestroyBullet();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignoruj kolizje z graczem (pocisk nie powinien trafić własnego gracza)
        if (collision.CompareTag("Player"))
        {
            return;
        }

        // Ignoruj inne pociski
        if (collision.CompareTag("Bullet"))
        {
            return;
        }

        // Trafienie w cokolwiek innego
        OnHit(collision);
    }

    void OnHit(Collider2D hitObject)
    {
        // Spawn efektu trafienia (jeśli jest przypisany)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Tutaj możesz dodać damage do przeciwników
        // Przykład:
        // var enemy = hitObject.GetComponent<EnemyHealth>();
        // if (enemy != null) enemy.TakeDamage(damageAmount);

        Debug.Log($"Bullet hit: {hitObject.gameObject.name}");

        // Zniszcz pocisk po trafieniu
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        // Zniszcz pocisk gdy wyjdzie poza ekran (opcjonalna optymalizacja)
        // Odkomentuj jeśli chcesz
        // DestroyBullet();
    }
}
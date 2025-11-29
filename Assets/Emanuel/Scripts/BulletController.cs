using UnityEngine;

/// <summary>
/// Kontroluje zachowanie pocisku - ruch, kolizje, niszczenie
/// UŻYWA GameTime.timescale dla płynnego slow-motion
/// ONE-HIT-KILL - każde trafienie niszczy wroga
/// Dodaj ten skrypt do prefaba Bullet
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Bazowa prędkość pocisku (będzie skalowana przez GameTime.timescale)")]
    public float bulletSpeed = 15f;

    [Tooltip("Czas życia pocisku w sekundach")]
    public float lifetime = 5f;

    [Tooltip("Czy pocisk niszczy się po trafieniu")]
    public bool destroyOnHit = true;

    [Header("One-Hit-Kill")]
    [Tooltip("ONE HIT = ONE KILL - wróg umiera od jednego strzału")]
    public bool oneHitKill = true;

    [Header("Effects - Opcjonalnie")]
    [Tooltip("Prefab efektu przy trafieniu (opcjonalnie)")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D rb;
    private Vector2 baseDirection; // Bazowy kierunek (nie skalowany)
    private float spawnTime;
    private TrailController trailController; // Referencja do smugi

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailController = GetComponent<TrailController>();
        spawnTime = Time.time;
    }

    /// <summary>
    /// Inicjalizuje pocisk z kierunkiem lotu
    /// Wywoływane przez WeaponController przy tworzeniu pocisku
    /// </summary>
    public void Initialize(Vector2 shootDirection)
    {
        baseDirection = shootDirection.normalized;

        // Ustaw początkową prędkość (skalowaną przez GameTime.timescale)
        if (rb != null)
        {
            rb.velocity = baseDirection * bulletSpeed * GameTime.timescale;
        }

        // Obróć pocisk w kierunku lotu
        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        // KLUCZOWE: Aktualizuj prędkość na podstawie GameTime.timescale
        // Pozwala na płynne slow-motion (0.1 - 1.0)
        if (rb != null)
        {
            rb.velocity = baseDirection * bulletSpeed * GameTime.timescale;
        }
    }

    void Update()
    {
        // Zniszcz pocisk po upływie lifetime
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

        // Trafienie w wroga
        if (collision.CompareTag("Enemy"))
        {
            OnHitEnemy(collision);
            return;
        }

        // Trafienie w ścianę/przeszkodę
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            OnHitWall(collision);
            return;
        }

        // Trafienie w cokolwiek innego
        OnHit(collision);
    }

    void OnHitEnemy(Collider2D enemy)
    {
        Debug.Log($"Bullet hit ENEMY: {enemy.gameObject.name} - ONE HIT KILL!");

        // Spawn efektu trafienia
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // ONE-HIT-KILL: Zniszcz wroga natychmiast
        if (oneHitKill)
        {
            IDamageable damageScript = enemy.GetComponent<IDamageable>();
            if (damageScript != null)
            {
                damageScript.TakeDamage();
                Debug.Log($"Enemy {enemy.gameObject.name} hit!");
            }
        }

        // Zniszcz pocisk
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }

    void OnHitWall(Collider2D wall)
    {
        Debug.Log($"Bullet hit WALL: {wall.gameObject.name}");

        // Spawn efektu trafienia
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Zniszcz pocisk po trafieniu w ścianę
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }

    void OnHit(Collider2D hitObject)
    {
        Debug.Log($"Bullet hit: {hitObject.gameObject.name}");

        // Spawn efektu trafienia
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Zniszcz pocisk po trafieniu
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        // Opcjonalnie: wyczyść smugę przed zniszczeniem
        if (trailController != null)
        {
            trailController.ClearTrail();
        }
        
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        // Opcjonalnie: Zniszcz pocisk gdy wyjdzie poza ekran
        // Odkomentuj jeśli chcesz automatyczne niszczenie
        // DestroyBullet();
    }
}
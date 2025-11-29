using UnityEngine;

/// <summary>
/// Kontroluje zachowanie pocisku - ruch, kolizje, niszczenie
/// UŻYWA GameTime.timescale dla płynnego slow-motion
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
        // Ignoruj kolizje z graczem
        if (collision.CompareTag("Player")) return;

        // Ignoruj inne pociski
        if (collision.CompareTag("Bullet")) return;

        // Trafienie w wroga
        if (collision.CompareTag("Enemy"))
        {
            OnHitEnemy(collision);
            return;
        }

        // Trafienie w ścianę
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
        Debug.Log($"[BULLET] HIT ENEMY: {enemy.gameObject.name}");
        
        // Spawn efektu
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // ONE-HIT-KILL: Zniszcz wroga natychmiast
        if (oneHitKill)
        {
            IDamageable damageScript = enemy.GetComponent<IDamageable>();
            if (damageScript != null)
            {
                damageScript.TakeDamage();
                Debug.Log($"[BULLET] Enemy {enemy.gameObject.name} took damage via IDamageable!");
            }
            else
            {
                // Fallback: Jeśli nie ma IDamageable, po prostu zniszcz wroga
                Destroy(enemy.gameObject);
                Debug.Log($"[BULLET] Enemy {enemy.gameObject.name} destroyed (no IDamageable)!");
            }
        }
        
        // KLUCZOWE: Zniszcz pocisk po trafieniu!
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }

    void OnHitWall(Collider2D wall)
    {
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        
        // Poprawiona literówka (było destroyOnHait)
        if (destroyOnHit) DestroyBullet();
    }

    void OnHit(Collider2D hitObject)
    {
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (destroyOnHit) DestroyBullet();
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
}
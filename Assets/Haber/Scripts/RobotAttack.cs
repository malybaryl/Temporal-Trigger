using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    private Animator animator;

    private const float defaultFireCooldownMs = 450f;
    private float currFireCooldownMs;
    private bool allowFire = false;
    private Transform targetPlayer;

    private Vector2 prevDirection = Vector3.zero;
    float verticalDirection = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currFireCooldownMs = defaultFireCooldownMs;
    }

    // Update is called once per frame
    void Update()
    {
        if (currFireCooldownMs <= 0)
        {
            currFireCooldownMs = defaultFireCooldownMs;
            Fire();
        }
        currFireCooldownMs -= Time.deltaTime * 1000 * GameTime.timescale;
    }

    public void SetAllowFire(Transform player, bool allow, int verDirection)
    {
        allowFire = allow;
        verticalDirection = verDirection;

        if (!allowFire)
            currFireCooldownMs = defaultFireCooldownMs;
        else targetPlayer = player;
    }

    private void Fire()
    {
        if (targetPlayer == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript == null) return;

        float bulletSpeed = bulletScript.GetSpeed();

        Vector2 predictedPos = GetPredictedPosition(targetPlayer, bulletSpeed);

        Vector2 direction = (predictedPos - (Vector2)transform.position).normalized;

        audioSource.PlayOneShot(fireClip);
        bulletScript.OnSpawn(direction);
        FireAnimation();
    }

    private Vector2 GetPredictedPosition(Transform target, float bulletSpeed)
    {
        Vector2 shooterPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 targetVel = target.GetComponent<Rigidbody2D>().velocity;

        Vector2 displacement = targetPos - shooterPos;

        float a = Vector2.Dot(targetVel, targetVel) - bulletSpeed * bulletSpeed;
        float b = 2 * Vector2.Dot(displacement, targetVel);
        float c = Vector2.Dot(displacement, displacement);

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0 || Mathf.Abs(a) < 0.0001f)
            return targetPos;

        float sqrtD = Mathf.Sqrt(discriminant);

        float t1 = (-b + sqrtD) / (2 * a);
        float t2 = (-b - sqrtD) / (2 * a);

        float t = Mathf.Min(t1, t2);
        if (t < 0) t = Mathf.Max(t1, t2);
        if (t < 0) return targetPos; 

        return targetPos + targetVel * t;
    }

    private void FireAnimation()
    {
        if (verticalDirection <= 0)
            animator.Play("RobotFrontFire");
        else
            animator.Play("RobotBackFire");
    }
}

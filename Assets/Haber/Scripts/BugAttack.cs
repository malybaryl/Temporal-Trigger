using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    private Animator animator;

    private const float defaultFireCooldownMs = 1500f;
    private float currFireCooldownMs;
    private bool allowFire = false;
    private Transform targetPlayer;
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
        audioSource.pitch = GameTime.timescale;
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
        GameObject bulletLeft = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        GameObject bulletRight = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        BugBullet bulletScriptCenter = bullet.GetComponent<BugBullet>();
        BugBullet bulletScriptLeft = bulletLeft.GetComponent<BugBullet>();
        BugBullet bulletScriptRight = bulletRight.GetComponent<BugBullet>();

        if (bulletScriptCenter == null) return;

        float bulletSpeed = bulletScriptCenter.GetSpeed();

        Vector2 direction = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

        Vector2 leftDir = Quaternion.Euler(0, 0, 15f) * direction;
        Vector2 rightDir = Quaternion.Euler(0, 0, -15f) * direction;

        audioSource.PlayOneShot(fireClip);
        bulletScriptCenter.OnSpawn(direction);
        bulletScriptLeft.OnSpawn(leftDir);
        bulletScriptRight.OnSpawn(rightDir);
        FireAnimation();
    }

    private void FireAnimation()
    {
        if (verticalDirection <= 0)
            animator.Play("UglyFireFront");
        else
            animator.Play("UglyFireBack");
    }
}

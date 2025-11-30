using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserLogic : MonoBehaviour, IDamageable
{
    private EnemyPathfinding pathfindScript;
    //private RobotAttack attackScript;
    private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip activateClip;
    [SerializeField] private LayerMask visionMask;

    private List<GameObject> players = new List<GameObject>();
    private Animator animator;
    private int verticalSign = 0;
    bool isDying = false;
    private bool sawPlayer = false;

    private int hp = 3;

    private float gracePeroidMs = 250f;
    private float graceMs;

    private void Awake()
    {
        pathfindScript = GetComponent<EnemyPathfinding>();
        if (pathfindScript == null)
            Debug.LogWarning("Nie znaleziono EnemyPathfinding!");

        audioSource = GetComponent<AudioSource>();
        graceMs = 0;
    }

    private void Start()
    {
        AddPlayersToTargetList();
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Nie znaleziono animatora!");
    }

    private void Update()
    {
        if (isDying)
        {
            pathfindScript.ClearPath();
            return;
        }

        graceMs -= Time.deltaTime * GameTime.timescale * 1000;


        float y = pathfindScript.movementDirection.y;

        verticalSign = GetVerticalSign(pathfindScript.movementDirection);

        Transform targetPlayer = GetNearestPlayerInRange();
        animator.SetInteger("moveDirection", verticalSign);
        animator.speed = GameTime.timescale;

        if (players.Count > 0)
        {
            if (targetPlayer != null)
            {
                Vector2 dir = (targetPlayer.position - transform.position).normalized;
                float dist = Vector2.Distance(transform.position, targetPlayer.position);

                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    dir,
                    dist,
                    visionMask
                );

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Player") && !sawPlayer)
                    {
                        audioSource.PlayOneShot(activateClip);
                        sawPlayer = true;
                    }
                }
            }
        }

        if (targetPlayer != null && sawPlayer)
        {
            pathfindScript.SetPath(targetPlayer.transform.position);
        }
    }


    private void AddPlayersToTargetList()
    {
        if (players.Count <= 0)
        {
            GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in foundPlayers)
            {
                if (!players.Contains(player))
                    players.Add(player);
            }
        }
    }

    private Transform GetNearestPlayerInRange()
    {
        Transform targetPlayer = null;
        float closestDist = Mathf.Infinity;
        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(player.transform.position, transform.position);

            closestDist = distance;
            targetPlayer = player.transform;
        }
        return targetPlayer;
    }

    private int GetVerticalSign(Vector2 vector)
    {
        int verticalSign = 0;

        float threshold = 0.01f;
        if (vector.y > threshold)
            verticalSign = 1;
        else if (vector.y < -threshold)
            verticalSign = -1;

        return verticalSign;
    }

    public void TakeDamage()
    {
        if (isDying)
            return;

        if (graceMs <= 0)
        {
            graceMs = gracePeroidMs;

            Debug.Log("take damage");
            hp--;

            if (hp <= 0)
            {
                animator.Play("EvilDeath");
                audioSource.PlayOneShot(deathClip);
                isDying = true;
            }
            else
            {
                animator.Play("EvilDamage");
                audioSource.PlayOneShot(damageClip);
                pathfindScript.ResetSpeed();
            }
        }
    }

    public void OnDeathFinish()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnHitPlayer(collision);
            return;
        }
    }

    private void OnHitPlayer(Collider2D playerCol)
    {
        Movement player = playerCol.GetComponent<Movement>();
        if (player != null)
            player.Die();
    }
}

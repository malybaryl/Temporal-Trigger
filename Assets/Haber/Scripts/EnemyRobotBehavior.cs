using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EnemyRobotBehavior : MonoBehaviour, IDamageable
{
    private EnemyPathfinding pathfindScript;
    private RobotAttack attackScript;
    private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;


    [SerializeField] float minActivationRange = 10f;
    [SerializeField] float stopAndFireRange = 5f;

    private List<GameObject> players = new List<GameObject>();
    private Transform targetPlayer;
    private Animator animator;

    [SerializeField] private float timeModifier = 1f;
    [SerializeField] private LayerMask visionMask;

    private int verticalSign = 0;
    bool isDying = false;

    private void Awake()
    {
        pathfindScript = GetComponent<EnemyPathfinding>();
        if (pathfindScript == null)
            Debug.LogWarning("Nie znaleziono EnemyPathfinding!");

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Nie znaleziono animatora!");


        attackScript = GetComponent<RobotAttack>();
        if (attackScript == null)
            Debug.LogWarning("Nie znaleziono AttackScript");

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        AddPlayersToTargetList();
    }


    private void Update()
    {
        if (isDying)
        {
            pathfindScript.ClearPath();
            attackScript.SetAllowFire(null, false, verticalSign);
            return;
        }
  


        float y = pathfindScript.movementDirection.y;

        verticalSign = GetVerticalSign(pathfindScript.movementDirection);

        Transform targetPlayer = GetNearestPlayerInRange();
        animator.SetInteger("moveDirection", verticalSign);
        animator.speed = timeModifier;

        bool seePlayer = false;
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
                    if (hit.collider.CompareTag("Player"))
                    {
                        float distanceToCollider = Vector2.Distance(transform.position, hit.collider.transform.position);
                        if (distanceToCollider < stopAndFireRange)
                            pathfindScript.ClearPath();
                        seePlayer = true;
                    }
                }
            }
        }

        if (targetPlayer != null)
        {
            float distanceToNearest = Vector2.Distance(targetPlayer.transform.position, transform.position);
            if (distanceToNearest > stopAndFireRange || !seePlayer)
            {
                pathfindScript.SetPath(targetPlayer.transform.position);
                attackScript.SetAllowFire(targetPlayer.transform, false, verticalSign);
            }
            else
            {
                pathfindScript.ClearPath();
                verticalSign = GetVerticalSign(targetPlayer.transform.position - transform.position);
                animator.SetInteger("moveDirection", verticalSign);
                attackScript.SetAllowFire(targetPlayer.transform, true, verticalSign);
            }
        }
        else
        {
            pathfindScript.ClearPath();
            attackScript.SetAllowFire(null, false, verticalSign);
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

            if (distance < minActivationRange && distance < closestDist)
            {
                closestDist = distance;
                targetPlayer = player.transform;
            }
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
        animator.Play("RobotDeath");
        audioSource.PlayOneShot(deathClip);
        isDying = true;
    }

    public void OnDeathFinish()
    {
        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EnemyRobotBehavior : MonoBehaviour
{
    private EnemyPathfinding pathfindScript;
    [SerializeField] float minActivationRange = 10f;
    [SerializeField] float stopAndFireRange = 5f;

    private List<GameObject> players = new List<GameObject>();
    private Transform targetPlayer;
    private Animator animator;

    [SerializeField] private float timeModifier = 1f;
    [SerializeField] private LayerMask visionMask;


    private void Awake()
    {
        pathfindScript = GetComponent<EnemyPathfinding>();
        if (pathfindScript == null)
            Debug.LogWarning("Nie znaleziono EnemyPathfinding!");

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AddPlyersToTargetList();
    }


    private void Update()
    {
        float y = pathfindScript.movementDirection.y;

        int verticalSign = GetVerticalSign(pathfindScript.movementDirection);

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
                pathfindScript.SetPath(targetPlayer.transform.position);
            else
            {
                pathfindScript.ClearPath();
                verticalSign = GetVerticalSign(targetPlayer.transform.position - transform.position);
                animator.SetInteger("moveDirection", verticalSign);
            }
        }
        else
            pathfindScript.ClearPath();
    }


    private void AddPlyersToTargetList()
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
}

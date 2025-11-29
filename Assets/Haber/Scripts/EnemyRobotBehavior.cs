using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyRobotBehavior : MonoBehaviour
{
    private EnemyPathfinding pathfindScript;
    [SerializeField] float minActivationRange = 15;
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
        int verticalSign = 0;

        float threshold = 0.01f;
        if (y > threshold)
            verticalSign = 1;
        else if (y < -threshold)
            verticalSign = -1;

        Transform targetPlayer = GetNearestPlayerInRange();
        animator.SetInteger("moveDirection", verticalSign);
        pathfindScript.SetTimeModifier(timeModifier);
        animator.speed = timeModifier;


        if (players.Count > 0)
        {
            Transform nearestPlayer = GetNearestPlayerInRange();
            if (nearestPlayer == null)
                return;

            Vector2 dir = (nearestPlayer.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, nearestPlayer.position);

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
                    //hit
                }
            }
        }
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
}

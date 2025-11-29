using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyRobotBehavior : MonoBehaviour
{
    private EnemyPathfinding patchfindScript;
    [SerializeField] float minActivationRange = 15;
    private List<GameObject> players = new List<GameObject>();
    private Transform targetPlayer;
    private Animator animator;
    

    private void Awake()
    {
        patchfindScript = GetComponent<EnemyPathfinding>();
        if (patchfindScript == null)
            Debug.LogWarning("Nie znaleziono EnemyPathfinding!");

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AddPlyersToTargetList();
    }


    private void Update()
    {
        float y = patchfindScript.movementDirection.y;
        int verticalSign = 0;

        float threshold = 0.01f;
        if (y > threshold)
            verticalSign = 1;
        else if (y < -threshold)
            verticalSign = -1;

        Debug.Log($"movementDirection.y = {y}, verticalSign = {verticalSign}");

        Transform targetPlayer = GetNearestPlayerInRange();
        Debug.Log(targetPlayer);

        animator.SetInteger("moveDirection", verticalSign);
    }


    private void AddPlyersToTargetList()
    {
        if (players.Count <= 0)
        {
            GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log(foundPlayers.Length);
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

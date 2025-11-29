using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

static class GameTime
{
    public static float timescale { private set; get; } = 1f;

    public static void SetTimeScale(float newScale)
    {
        timescale = Mathf.Clamp01(newScale);
    }
}

public class TimeManagerScript : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();
    private float maxPlayerSpeed = 5f;
    private Dictionary<GameObject, Vector2> playerLastPositions = new Dictionary<GameObject, Vector2>();

    void Start()
    {
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in foundPlayers)
        {
            players.Add(player);
            Vector2 playerPosition = player.transform.position;
            playerLastPositions.Add(player, playerPosition);
        }
    }


    void FixedUpdate()
    {
        float highestSpeed = 0f;

        foreach (GameObject player in players)
        {
            Vector2 lastPos = playerLastPositions[player];
            Vector2 currentPos = player.transform.position;


            float speed = Vector2.Distance(lastPos, currentPos) / Time.fixedDeltaTime;

            if (speed > highestSpeed)
                highestSpeed = speed;

            playerLastPositions[player] = currentPos;
        }

        float playerSpeedNormalized = Mathf.Clamp01(highestSpeed / maxPlayerSpeed);
        GameTime.SetTimeScale(playerSpeedNormalized);
    }
}

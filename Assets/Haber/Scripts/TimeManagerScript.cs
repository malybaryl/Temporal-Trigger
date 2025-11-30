using System.Collections.Generic;
using UnityEngine;


public class TimeManagerScript : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();
    private Dictionary<GameObject, Vector2> playerLastPositions = new Dictionary<GameObject, Vector2>();

    private float maxPlayerSpeed = 5f;
    private float startupDelay = 0.3f;
    private float timer = 0f;

    void FixedUpdate()
    {
        float dt = Time.fixedUnscaledDeltaTime;

        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in foundPlayers)
        {
            if (!players.Contains(p))
            {
                players.Add(p);
                playerLastPositions[p] = p.transform.position;
            }
        }


        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                playerLastPositions.Remove(players[i]);
                players.RemoveAt(i);
            }
        }

        float highestSpeed = 0f;

        foreach (var player in players)
        {
            Vector2 lastPos = playerLastPositions[player];
            Vector2 currentPos = player.transform.position;

            float speed = Vector2.Distance(lastPos, currentPos) / dt;
            highestSpeed = Mathf.Max(highestSpeed, speed);

            // Aktualizacja pozycji po obliczeniu prêdkoœci
            playerLastPositions[player] = currentPos;
        }

        timer += dt;

        if (timer < startupDelay || highestSpeed <= 0f)
        {
            GameTime.SetTimeScale(1f);
            return;
        }

        float normalized = Mathf.Clamp01(highestSpeed / maxPlayerSpeed);
        GameTime.SetTimeScale(normalized);
    }
}
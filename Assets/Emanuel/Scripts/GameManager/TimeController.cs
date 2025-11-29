using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Kontroluje przepływ czasu w grze
/// Czas płynie tylko gdy którykolwiek gracz się porusza
/// Automatycznie dodawany do GameManager
/// </summary>
public class TimeController : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Jak długo po zatrzymaniu gracza czas nadal płynie (sekundy)")]
    public float timeGracePeriod = 0.1f;

    [Header("Debug")]
    [Tooltip("Pokaż status czasu w Console")]
    public bool showDebugLogs = false;

    // Śledzenie ruchu graczy
    private Dictionary<GameObject, bool> playerMovementStatus = new Dictionary<GameObject, bool>();
    private float lastMovementTime = 0f;
    private bool isTimeFlowing = true;
    private bool wasTimeFlowing = true;

    void Start()
    {
        // Na początku czas płynie normalnie
        SetTimeScale(1f);
        lastMovementTime = Time.unscaledTime;
    }

    void Update()
    {
        CheckTimeFlow();
    }

    /// <summary>
    /// Sprawdza czy którykolwiek gracz się porusza
    /// </summary>
    void CheckTimeFlow()
    {
        bool anyPlayerMoving = IsAnyPlayerMoving();

        if (anyPlayerMoving)
        {
            lastMovementTime = Time.unscaledTime;
        }

        // Czas płynie jeśli:
        // - Którykolwiek gracz się porusza ALBO
        // - Minęło mniej niż timeGracePeriod od ostatniego ruchu (dla płynności)
        float timeSinceLastMovement = Time.unscaledTime - lastMovementTime;
        isTimeFlowing = timeSinceLastMovement < timeGracePeriod;

        // Aktualizuj TimeScale gdy status się zmienia
        if (isTimeFlowing != wasTimeFlowing)
        {
            if (isTimeFlowing)
            {
                ResumeTime();
            }
            else
            {
                PauseTime();
            }
            wasTimeFlowing = isTimeFlowing;
        }
    }

    /// <summary>
    /// Czy którykolwiek gracz się porusza
    /// </summary>
    bool IsAnyPlayerMoving()
    {
        foreach (var kvp in playerMovementStatus)
        {
            if (kvp.Value) // Jeśli ten gracz się porusza
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Rejestruje ruch gracza (wywołaj z Movement.cs)
    /// </summary>
    public void RegisterPlayerMovement(GameObject player, bool isMoving)
    {
        if (!playerMovementStatus.ContainsKey(player))
        {
            playerMovementStatus.Add(player, false);
        }
        
        playerMovementStatus[player] = isMoving;
    }

    /// <summary>
    /// Wznawia czas gry
    /// </summary>
    void ResumeTime()
    {
        SetTimeScale(1f);
        DebugLog("TimeController: Czas wznowiony (gracz się porusza)");
    }

    /// <summary>
    /// Pauzuje czas gry
    /// </summary>
    void PauseTime()
    {
        SetTimeScale(0f);
        DebugLog("TimeController: Czas zatrzymany (wszyscy gracze stoją)");
    }

    /// <summary>
    /// Ustawia TimeScale (0 = pauza, 1 = normalnie)
    /// </summary>
    void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    /// <summary>
    /// Czy czas obecnie płynie
    /// </summary>
    public bool IsTimeFlowing()
    {
        return isTimeFlowing;
    }

    /// <summary>
    /// Wymuś wznowienie czasu (np. podczas strzału)
    /// </summary>
    public void ForceResumeTime(float duration)
    {
        lastMovementTime = Time.unscaledTime + duration;
    }

    /// <summary>
    /// Helper do debug logów
    /// </summary>
    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

    void OnDestroy()
    {
        // Przywróć normalny czas przy zniszczeniu
        Time.timeScale = 1f;
    }
}

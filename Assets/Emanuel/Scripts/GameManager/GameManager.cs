using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Główny manager gry - Singleton
/// Zarządza stanem gry, trybem (single/co-op), i koordynuje inne systemy
/// Dodaj do pustego GameObject o nazwie "GameManager" na scenie
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Game Mode")]
    [Tooltip("Czy gra w trybie co-op (wielu graczy)?")]
    public bool isCoopMode = false;
    
    [Header("Debug")]
    [Tooltip("Pokaż debug info w Console")]
    public bool showDebugLogs = true;

    // Lista wszystkich graczy w grze
    private List<GameObject> players = new List<GameObject>();
   

    void Awake()
    {
        // Singleton - tylko jedna instancja
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // Przetrwa zmianę sceny

        DebugLog("GameManager: Initialized");
    }

    void Start()
    {
        // Znajdź wszystkich graczy na scenie
        FindAllPlayers();
        
        // Automatycznie wykryj tryb gry
        DetectGameMode();
    }

    /// <summary>
    /// Znajduje wszystkich graczy z tagiem "Player"
    /// </summary>
    void FindAllPlayers()
    {
        players.Clear();
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(foundPlayers);
        
        DebugLog($"GameManager: Znaleziono {players.Count} graczy");
        
        foreach (var player in players)
        {
            DebugLog($"  - {player.name}");
        }
    }

    /// <summary>
    /// Automatycznie wykrywa czy to single czy co-op
    /// </summary>
    void DetectGameMode()
    {
        isCoopMode = players.Count > 1;
        DebugLog($"GameManager: Tryb gry = {(isCoopMode ? "CO-OP" : "SINGLE PLAYER")}");
    }

    /// <summary>
    /// Rejestruje nowego gracza (np. gdy spawn się w trakcie gry)
    /// </summary>
    public void RegisterPlayer(GameObject player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            DebugLog($"GameManager: Zarejestrowano gracza: {player.name}");
            DetectGameMode();
        }
    }

    /// <summary>
    /// Usuwa gracza z listy (np. gdy umrze)
    /// </summary>
    public void UnregisterPlayer(GameObject player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            DebugLog($"GameManager: Wyrejestrowano gracza: {player.name}");
        }
    }

    /// <summary>
    /// Zwraca listę wszystkich graczy
    /// </summary>
    public List<GameObject> GetAllPlayers()
    {
        return players;
    }

    /// <summary>
    /// Czy gra jest aktywna (są jacyś gracze)
    /// </summary>
    public bool IsGameActive()
    {
        return players.Count > 0;
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
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 1. Prze¿ywa zmianê scen (DontDestroyOnLoad).
/// 2. Po wciœniêciu 'R' resetuje wszystko i ³aduje "Co-op".
/// </summary>
public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;

    void Awake()
    {
        // Singleton - pilnuje, ¿eby by³ tylko jeden taki obiekt
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Nie niszcz mnie przy ³adowaniu
    }

    void Update()
    {
        // --- SPRAWDZANIE KLAWISZA 'R' ---
        bool restartPressed = false;

#if ENABLE_INPUT_SYSTEM
        // Obs³uga Nowego Input Systemu
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            restartPressed = true;
        }
#else
        // Obs³uga Starego Input Managera
        if (Input.GetKeyDown(KeyCode.R))
        {
            restartPressed = true;
        }
#endif

        // Jeœli wciœniêto R -> Restart
        if (restartPressed)
        {
            HardRestart();
        }
    }

    /// <summary>
    /// Czyœci stan gry, zeruje czas i ³aduje scenê od nowa.
    /// </summary>
    public void HardRestart()
    {
        Debug.Log("Wciœniêto R - Hard Restart Gry...");

        // 1. Reset Czasu
        Time.timeScale = 1f;

        // 2. Reset Zmiennych Statycznych (Wszystkie twoje klasy)
        ResetAllStatics();

        // 3. £adowanie Sceny (To automatycznie czyœci obiekty ze starej sceny)
        SceneManager.LoadScene("Co-op");
    }

    private void ResetAllStatics()
    {
        // Tutaj resetujemy stany ze wszystkich Twoich skryptów
        
        // Stan ¿ycia gracza
        PlayerState.Reset();
        
        // Stan œmierci (jeœli u¿ywasz tej klasy)
        PlayerDead.set(false);
        
        // Stan GameControllera (ukrycie panelu)
        GameController.dead = false;
        
        // Stan teleportacji (¿eby po restarcie nie myœla³, ¿e jest na dole)
        TeleportState.changeState(false);
    }
}
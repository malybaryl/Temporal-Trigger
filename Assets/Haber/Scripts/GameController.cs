using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private GameObject objectToShow; 
    
    // Zmienna statyczna
    public static bool dead = false;

    void Awake()
    {
        // --- 1. RESETOWANIE STANU GRY (NAJWA¯NIEJSZE) ---
        dead = false;           // Gracz ¿yje
        Time.timeScale = 1f;    // Czas p³ynie normalnie

        // --- 2. SZUKANIE UI ---
        // Szukamy obiektu. UWAGA: W edytorze Unity ten obiekt (GameOverCanvas) 
        // MUSI BYÆ W£¥CZONY (Active), ¿eby funkcja Find go znalaz³a.
        if (objectToShow == null)
        {
            objectToShow = GameObject.Find("GameOverCanvas");
        }

        // --- 3. NATYCHMIASTOWE UKRYCIE ---
        if (objectToShow != null)
        {
            objectToShow.SetActive(false); // Chowamy panel natychmiast przy ³adowaniu
        }
        else
        {
            Debug.LogError("B£¥D: Nie znaleziono 'GameOverCanvas'. Upewnij siê, ¿e jest W£¥CZONY w hierarchii przed startem gry!");
        }

        // --- 4. UKRYCIE KURSORA ---
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // Jeœli gracz ¿yje, nic nie rób
        if (!dead) return;

        // Jeœli umar³ i panel jest ukryty -> Poka¿ go
        if (objectToShow != null && !objectToShow.activeSelf)
        {
            ShowGameOver();
        }
    }

    public void ShowGameOver()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
        }
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
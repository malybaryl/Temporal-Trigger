using UnityEngine;
using UnityEngine.SceneManagement; // Wymagane do ³adowania scen

public class RestartLevel_ : MonoBehaviour
{
    /// <summary>
    /// Podepnij tê funkcjê pod przycisk (Button -> OnClick)
    /// </summary>
    public void RestartScene()
    {
        // 1. Zresetuj stan gracza (wa¿ne!)
        PlayerState.Reset();

        // 2. WA¯NE: Zresetuj stan GameControllera (¿eby ukryæ panel)
        GameController.dead = false; 
        
        // 3. Przywróæ czas (musi byæ przed za³adowaniem sceny, dla pewnoœci)
        Time.timeScale = 1f; 

        // 4. Pobierz nazwê aktywnej sceny i za³aduj j¹ ponownie
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
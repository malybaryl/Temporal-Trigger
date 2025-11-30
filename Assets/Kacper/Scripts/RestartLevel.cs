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
        
        // 2. Pobierz nazwê aktywnej sceny
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 3. Za³aduj j¹ ponownie
        SceneManager.LoadScene(currentSceneName);
        
        // Opcjonalnie: Przywróæ czas, jeœli by³ zatrzymany
        Time.timeScale = 1f; 
    }
}
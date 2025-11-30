using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlaySingle()
    {
        SceneManager.LoadScene("Haber"); // nazwa sceny singleplayer
    }

    public void PlayCoop()
    {
        SceneManager.LoadScene("Kacper"); // nazwa sceny coop
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

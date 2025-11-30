using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; // <- Konieczne do ³adowania scen

// WA¯NE: Doda³em IPointerClickHandler, ¿eby klikanie dzia³a³o
public class TryAgain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null && defaultSprite != null)
        {
            image.sprite = defaultSprite;
        }
    }

    // Najazd myszk¹ -> Zmiana na hoverSprite
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSprite != null) image.sprite = hoverSprite;
    }

    // Zjazd myszk¹ -> Powrót do defaultSprite
    public void OnPointerExit(PointerEventData eventData)
    {
        if (defaultSprite != null) image.sprite = defaultSprite;
    }

    // Klikniêcie -> Restart gry
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Przycisk klikniêty! Restartowanie gry...");
        RestartGame();
    }

    private void RestartGame()
    {
        // 1. Resetujemy zmienne statyczne (BARDZO WA¯NE!)
        // Inaczej po restarcie gracz od razu by³by martwy
        PlayerDead.set(false);
        PlayerState.Reset(); 
        
        // 2. Resetujemy czas (jeœli gra by³a zatrzymana lub w slow-mo)
        Time.timeScale = 1f;

        // 3. Pobieramy nazwê obecnej sceny i ³adujemy j¹ na nowo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
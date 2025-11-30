using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // <- konieczne dla Image

public class TryAgain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = defaultSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = defaultSprite; // powrót do domyœlnego
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Obiekt klikniêty!");
        TryAgainClick();
    }

    private void TryAgainClick()
    {
        Debug.Log("Wywo³ano funkcjê powrotu do menu!");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuReturn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        Debug.Log("Wywo³ano funkcjê powrotu do menu!");
    }
}

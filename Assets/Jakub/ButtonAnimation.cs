using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Zatrzymaj animację i odpal od nowa
        animator.Play("ButtonScalePunch_In", 0, 0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.Play("Entry", 0, 0f);
    }
}
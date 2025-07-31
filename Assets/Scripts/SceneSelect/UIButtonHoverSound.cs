using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayHoverSound();
        }
    }
}
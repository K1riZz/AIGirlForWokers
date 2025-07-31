using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PetInteractionSystem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event Action OnLeftClicked;
    public event Action OnRightClicked;
    public event Action OnBeginDrag;
    public event Action OnEndDrag;
    public event Action<Vector2> OnGuideRequest; // 新增：引导请求事件，参数为目标位置

    public Vector2 MousePosition { get; private set; }

    // This method is called by the EventSystem when a click occurs on this GameObject.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClicked?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClicked?.Invoke();
        }
    }

     void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        // 触发开始拖拽事件
        OnBeginDrag?.Invoke();
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        // 拖拽时更新鼠标位置，并移动宠物
        // 这假设UI Canvas是 Screen Space - Overlay 模式
        MousePosition = eventData.position;
        transform.position = MousePosition;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        // 触发结束拖拽事件
        OnEndDrag?.Invoke();
    }

    // 这个方法可以被外部调用，用于触发引导事件。
    public void RequestGuide(Vector2 position)
    {
        OnGuideRequest?.Invoke(position);
    }
}

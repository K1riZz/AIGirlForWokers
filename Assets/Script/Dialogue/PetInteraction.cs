using UnityEngine;

/// <summary>
/// 处理对宠物的直接交互，例如鼠标点击。
/// This script should be attached to the pet's GameObject.
/// </summary>
[RequireComponent(typeof(Collider2D))] // 确保宠物有2D碰撞体
public class PetInteraction : MonoBehaviour
{
    private PetBehaviorSystem m_BehaviorSystem;

    private void Awake()
    {
        m_BehaviorSystem = GetComponent<PetBehaviorSystem>();
        if (m_BehaviorSystem == null)
        {
            Debug.LogError("PetInteraction requires a PetBehaviorSystem component on the same GameObject.", this);
        }
    }

    private void OnMouseDown()
    {
        // 当鼠标左键点击时，通知 PetBehaviorSystem
        if (m_BehaviorSystem != null)
        {
            // 我们将交互逻辑交给 PetBehaviorSystem 处理
            // 它会在 HandlePetLeftClicked 方法中决定是显示气泡还是进入剧情
            m_BehaviorSystem.HandlePetLeftClicked();
        }
    }
}

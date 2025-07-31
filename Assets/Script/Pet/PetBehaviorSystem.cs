using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// PetBehaviorSystem.cs
public class PetBehaviorSystem : MonoBehaviour
{
    // 1. 状态机：使用枚举定义状态
    public enum PetState
    {
        Idle,       // 闲置
        Moving,     // 移动
        OnIcon,     // 在图标上
        Talking,    // 对话
        Dragging,   // 被拖动
        Following,  // 跟随鼠标
        Sleeping    // 睡觉
    }

    [Header("组件引用")]
    private PetInteractionSystem interactionSystem;
    private DesktopInteraction desktopInteraction; // 假设此脚本存在，用于获取桌面信息
    [Tooltip("将宠物的对话气泡UI对象拖到这里")]
    public GameObject speechBubble;
    [Tooltip("将对话气泡中的Text - TextMeshPro组件拖到这里")]
    public TextMeshProUGUI speechText;
    private Animator animator;

    [Header("行为参数")]
    public float moveSpeed = 2f;
    public float idleTimeMin = 3f;
    public float idleTimeMax = 8f;
    public float talkingDuration = 4f; // 宠物“说话”的持续时间
    public float followSpeed = 8f;

    [Header("状态")]
    [SerializeField] private PetState currentState;
    private Vector2 targetPosition;
    private float stateTimer; // 通用状态计时器
    private float nextBehaviorDuration; // 下一个行为的持续时间

    void Awake()
    {
        // 获取组件引用
        interactionSystem = GetComponent<PetInteractionSystem>();
        animator = GetComponent<Animator>();
        // 假设 DesktopInteraction 是一个单例或可以方便地找到
        desktopInteraction = FindObjectOfType<DesktopInteraction>();
    }

    void Start()
    {
        if (interactionSystem == null)
        {
            UnityEngine.Debug.LogError("未找到 PetInteractionSystem 组件！");
        }
        else
        {
            // 5. 引用PetInteractionSystem获取交互事件
            interactionSystem.OnLeftClicked += HandlePetLeftClicked;
            interactionSystem.OnRightClicked += HandlePetRightClicked;
            interactionSystem.OnBeginDrag += HandleBeginDrag;
            interactionSystem.OnEndDrag += HandleEndDrag;
            interactionSystem.OnGuideRequest += HandleGuideRequest;
        }

        if (desktopInteraction == null)
        {
            UnityEngine.Debug.LogError("场景中未找到 DesktopInteraction 组件！");
        }

        // 隐藏对话气泡
        if (speechBubble != null) speechBubble.SetActive(false);

        // 初始状态
        SwitchState(PetState.Idle);
        StartCoroutine(ReminderCoroutine());
    }

    void OnDestroy()
    {
        // 取消订阅以防止内存泄漏
        if (interactionSystem != null)
        {
            interactionSystem.OnLeftClicked -= HandlePetLeftClicked;
            interactionSystem.OnRightClicked -= HandlePetRightClicked;
            interactionSystem.OnBeginDrag -= HandleBeginDrag;
            interactionSystem.OnEndDrag -= HandleEndDrag;
            interactionSystem.OnGuideRequest -= HandleGuideRequest;
        }
    }

    // 3. 在Update中根据当前状态执行行为
    void Update()
    {
        switch (currentState)
        {
            case PetState.Idle:
                UpdateIdleState();
                break;
            case PetState.Moving:
                UpdateMovingState();
                break;
            case PetState.Talking:
                UpdateTalkingState();
                break;
            case PetState.Dragging:
                UpdateDraggingState();
                break;
            case PetState.Following:
                UpdateFollowingState();
                break;
        }
    }

    // 2. 状态切换方法
    private void SwitchState(PetState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        UnityEngine.Debug.Log($"切换到状态: {newState}");

        // 重置通用状态计时器
        stateTimer = 0f;

        // 每个状态的进入逻辑
        switch (newState)
        {
            case PetState.Idle:
                nextBehaviorDuration = UnityEngine.Random.Range(idleTimeMin, idleTimeMax);
                break;
            case PetState.Moving:
                // 目标位置应在切换到此状态之前设置好
                break;
            case PetState.Talking:
                nextBehaviorDuration = talkingDuration;
                break;
            case PetState.Dragging:
                // 开始拖动的逻辑，例如播放特定动画
                break;
            case PetState.Following:
                // 跟随鼠标时，如果再次右键点击，则停止跟随
                // 这个逻辑在 HandlePetRightClicked 中处理
                break;
            case PetState.Sleeping:
                // 进入睡眠状态的逻辑
                break;
        }
    }

    #region 状态更新方法

    private void UpdateIdleState()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= nextBehaviorDuration)
        {
            // 决定下一步做什么：70%概率移动到图标，30%随机移动
            if (UnityEngine.Random.value < 0.7f && desktopInteraction != null)
            {
                targetPosition = desktopInteraction.GetRandomIconPosition();
                SwitchState(PetState.Moving);
            }
            else
            {
                SetRandomDestination();
                SwitchState(PetState.Moving);
            }
        }
    }

    private void UpdateMovingState()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SwitchState(PetState.Idle);
        }
    }

    private void UpdateTalkingState()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= nextBehaviorDuration)
        {
            // 说话结束后，返回闲置状态
            if (speechBubble != null) speechBubble.SetActive(false);
            SwitchState(PetState.Idle);
        }
    }

    private void UpdateDraggingState()
    {
        // 位置更新由 PetInteractionSystem 在其 OnDrag 方法中处理
    }

    private void UpdateFollowingState()
    {
        // 平滑地跟随鼠标
        targetPosition = Input.mousePosition;
        transform.position = Vector2.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    #endregion

    #region 交互事件处理器

    private void HandlePetLeftClicked()
    {
        if (currentState != PetState.Dragging)
        {
            ShowSpeechBubble("你点我干嘛呀？");
        }
    }

    private void HandlePetRightClicked()
    {
        if (currentState != PetState.Dragging)
        {
            // 如果已经在跟随，则停止跟随；否则开始跟随
            if (currentState == PetState.Following)
            {
                SwitchState(PetState.Idle);
            }
            else
            {
                SwitchState(PetState.Following);
            }
        }
    }

    private void HandleBeginDrag()
    {
        SwitchState(PetState.Dragging);
    }

    private void HandleEndDrag()
    {
        SwitchState(PetState.Idle); // 结束拖动后返回闲置状态
    }

    private void HandleGuideRequest(Vector2 position)
    {
        // 只有在非拖动状态下才响应引导
        if (currentState != PetState.Dragging)
        {
            UnityEngine.Debug.Log($"收到引导请求，移动到: {position}");
            targetPosition = position;
            SwitchState(PetState.Moving);
        }
    }

    #endregion

    #region 辅助方法

    private void SetRandomDestination()
    {
        if (desktopInteraction == null) return;
        // 4. 引用DesktopInteraction获取桌面图标位置
        Vector2 screenBounds = desktopInteraction.GetScreenBounds();
        // 增加一个边距，防止宠物完全移出屏幕
        float margin = 50f;
        targetPosition = new Vector2(
            UnityEngine.Random.Range(margin, screenBounds.x - margin),
            UnityEngine.Random.Range(margin, screenBounds.y - margin)
        );
    }

    private void ShowSpeechBubble(string text, float duration = -1)
    {
        if (speechBubble == null || speechText == null) return;

        speechText.text = text;
        speechBubble.SetActive(true);
        talkingDuration = (duration > 0) ? duration : idleTimeMax; // Use custom duration or default
        SwitchState(PetState.Talking);
    }

    private IEnumerator ReminderCoroutine()
    {
        while (true)
        {
            // 每60秒提醒一次
            yield return new WaitForSeconds(60f);

            // 只有在宠物闲置时才触发提醒
            if(currentState == PetState.Idle)
                ShowSpeechBubble("已经过去一分钟啦，要不要起来活动一下？", 5f);
        }
    }
    #endregion
}

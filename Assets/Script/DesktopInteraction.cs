using UnityEngine;

public class DesktopInteraction : MonoBehaviour
{
    // 这是一个用于演示的简单实现。
    // 在实际应用中，这里可能需要与操作系统交互来获取图标位置。
    public static DesktopInteraction Instance { get; private set; }

    [Tooltip("将场景中代表桌面图标的UI对象拖到这里")]
    public RectTransform[] iconTransforms;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Vector2 GetRandomIconPosition()
    {
        if (iconTransforms == null || iconTransforms.Length == 0)
        {
            Debug.LogWarning("DesktopInteraction 中没有设置任何图标！返回一个随机位置。");
            // 占位符：返回屏幕上的一个随机位置。
            return new Vector2(Random.Range(100, Screen.width - 100), Random.Range(100, Screen.height - 100));
        }

        int randomIndex = Random.Range(0, iconTransforms.Length);
        return iconTransforms[randomIndex].position;
    }

    public Vector2 GetScreenBounds()
    {
        // 返回屏幕尺寸。
        return new Vector2(Screen.width, Screen.height);
    }
}
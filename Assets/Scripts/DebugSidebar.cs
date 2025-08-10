using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugSidebar : MonoBehaviour
{
    [SerializeField] GameObject panel;       // panel chứa log
    [SerializeField] TMP_Text logText;       // text hiển thị log
    [SerializeField] ScrollRect scrollRect;  // scroll để cuộn log
    [SerializeField] KeyCode toggleKey = KeyCode.BackQuote; // phím bật/tắt (`)

    public static DebugSidebar instance;

    void Awake()
    {
        instance = this;
        if (panel) panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            TogglePanel();
    }

    public void TogglePanel()
    {
        if (panel) panel.SetActive(!panel.activeSelf);
    }

    public void SetLog(string text)
    {
        if (!logText) return;
        logText.text = text;

        // cuộn xuống cuối
        Canvas.ForceUpdateCanvases();
        if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
    }
}

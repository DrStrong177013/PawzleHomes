using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugSidebar : MonoBehaviour
{
    [SerializeField] GameObject panel;       // panel chứa log
    [SerializeField] TMP_Text logText;       // text hiển thị log
    [SerializeField] ScrollRect scrollRect;  // scroll để cuộn log
    [SerializeField] KeyCode toggleKey = KeyCode.BackQuote; // phím bật/tắt (`)

    [Header("Copy UI")]
    [SerializeField] TMP_Text toastLabel;         // Text nhỏ báo "Copied!"
    [SerializeField] float toastSeconds = 1.2f;

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

    public void ClearLog()
    {
        if (!logText) return;
        logText.text = string.Empty;
        // cuộn xuống đầu
        Canvas.ForceUpdateCanvases();
        if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
    }
    public void ClosePanel()
    {
        if (panel) panel.SetActive(false);
    }

    public void SetLog(string text)
    {
        if (!logText) return;
        logText.text = text;

        // cuộn xuống cuối
        Canvas.ForceUpdateCanvases();
        if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
    }
    // ====== COPY TO CLIPBOARD ======
    public void CopyLog()
    {
        string text = logText ? logText.text : "";
        if (string.IsNullOrEmpty(text))
        {
            ShowToast("No log");
            return;
        }

        GUIUtility.systemCopyBuffer = text;   // iOS/Android/PC đều dùng được
#if UNITY_ANDROID
        Handheld.Vibrate();                   // rung nhẹ báo thành công (tuỳ chọn)
#endif
        ShowToast("Copied to clipboard");
    }

    void ShowToast(string msg)
    {
        if (!toastLabel) return;
        StopAllCoroutines();
        StartCoroutine(CoToast(msg));
    }

    IEnumerator CoToast(string msg)
    {
        toastLabel.text = msg;
        toastLabel.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(toastSeconds);
        toastLabel.gameObject.SetActive(false);
    }
}

using System.Collections;
using UnityEngine;

public class MenuTransition : MonoBehaviour
{
    // Cờ cho toàn phiên chơi (static sống xuyên scene trong runtime)
    private static bool s_AlreadyPlayed = false;

    [Header("Portal")]
    public RectTransform portalMask;

    [Header("Cat Fly-In")]
    public RectTransform catSprite;

    [Header("Neon Frame")]
    public CanvasGroup neonFrame;

    [Header("Menu Group")]
    public CanvasGroup menuGroup;

    [Header("Timings")]
    public float portalTime = 0.6f;
    public float catTime = 0.7f;
    public float neonTime = 0.5f;
    public float menuFadeTime = 0.3f;

    private void Awake()
    {
        // Nếu đã chạy rồi trong phiên này → bỏ qua hiệu ứng, set trạng thái cuối và tắt script
        if (s_AlreadyPlayed)
        {
            ApplyFinalState();
            enabled = false; // chặn Start/Coroutines của component này
        }
    }

    private void Start()
    {
        if (!enabled) return; // đã bị skip trong Awake

        // Trạng thái ban đầu
        portalMask.gameObject.SetActive(false);

        catSprite.anchoredPosition = new Vector2(-Screen.width, 0);
        catSprite.gameObject.SetActive(false);

        neonFrame.alpha = 0f;

        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        menuGroup.gameObject.SetActive(false);

        s_AlreadyPlayed = true; // đánh dấu đã phát intro
        StartCoroutine(DoTransition());
    }

    private IEnumerator DoTransition()
    {
        yield return null;

        // 1) Portal mask shader cutoff
        var portalImage = portalMask.GetComponent<UnityEngine.UI.Image>();
        var mat = portalImage.material;
        mat.SetFloat("_Cutoff", 0f);
        LeanTween.value(gameObject, 0f, 1f, portalTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate(v => mat.SetFloat("_Cutoff", v));
        yield return new WaitForSeconds(portalTime);

        // 1b) Fill vòng tròn
        portalMask.gameObject.SetActive(true);
        portalImage.fillAmount = 0f;
        LeanTween.value(portalMask.gameObject, 0f, 1f, portalTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float v) => { portalImage.fillAmount = v; });
        yield return new WaitForSeconds(portalTime);

        // 2) Cat bay vào giữa
        catSprite.gameObject.SetActive(true);
        LeanTween.moveLocal(catSprite.gameObject, Vector3.zero, catTime)
            .setEase(LeanTweenType.easeOutBack);
        yield return new WaitForSeconds(catTime);

        // 3) Neon frame fade-in
        LeanTween.alphaCanvas(neonFrame, 1f, neonTime)
            .setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(neonTime);

        // 4) Menu hiện và bật tương tác
        menuGroup.gameObject.SetActive(true);
        LeanTween.alphaCanvas(menuGroup, 1f, menuFadeTime)
            .setOnComplete(() =>
            {
                menuGroup.interactable = true;
                menuGroup.blocksRaycasts = true;
            });

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    public void DoTransitionNow()
    {
        // Nếu đã phát rồi thì không play lại
        if (s_AlreadyPlayed)
        {
            ApplyFinalState();
            return;
        }
        s_AlreadyPlayed = true;
        StartCoroutine(DoTransition());
    }

    private void ApplyFinalState()
    {
        // Trạng thái “sau khi animation xong”
        if (portalMask) portalMask.gameObject.SetActive(false);

        if (catSprite)
        {
            catSprite.gameObject.SetActive(true);
            catSprite.anchoredPosition = Vector2.zero;
        }

        if (neonFrame) neonFrame.alpha = 1f;

        if (menuGroup)
        {
            menuGroup.gameObject.SetActive(true);
            menuGroup.alpha = 1f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }

        // Ẩn object controller nếu bạn muốn
        gameObject.SetActive(false);
    }
}

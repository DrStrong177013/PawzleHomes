using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBackground : MonoBehaviour
{
    [SerializeField] GameOver gameOver;
    [SerializeField] Transform gridImage;
    [SerializeField] Transform ring1;
    [SerializeField] Component[] components;

    Image gridImg, ringImg;
    GameObject[] bgList;
    Image[] bgImgs;
    bool[] levelCleareds;

    void OnEnable() { GameEvents.LevelCleared += Run; GameEvents.GridAppears += RunGridAppears; }
    void OnDisable() { GameEvents.LevelCleared -= Run; GameEvents.GridAppears -= RunGridAppears; }

    IEnumerator Start()
    {
        // Chờ dữ liệu
        yield return new WaitUntil(() => GameData.stageLevelDict != null);
        Init();
        InitDefaultAlpha(0.07f); // cleared = 1, else = 0.07
    }

    void Init()
    {
        int stage = Mathf.Max(1, GameData.currentStage);
        levelCleareds = GetBoolClearedLevels(stage);

        var comp = components[Mathf.Clamp(stage - 1, 0, components.Length - 1)];
        var imgs = comp.GetComponentsInChildren<Image>(true)
            .Where(i => i.transform.parent == comp.transform)
            .OrderBy(i => i.transform.GetSiblingIndex())
            .Select(i => i.gameObject)
            .ToList();

        bgList = imgs.Take(GameData.stageLevelDict[stage]).ToArray();
        bgImgs = bgList.Select(go => go.GetComponent<Image>()).ToArray();

        // Off các component khác
        for (int i = 0; i < components.Length; i++)
            components[i].gameObject.SetActive(i == stage - 1);

        gridImg = gridImage ? gridImage.GetComponent<Image>() : null;
        ringImg = ring1 ? ring1.GetComponent<Image>() : null;
        if (!gridImg || !ringImg) Debug.LogError("Missing Image on grid/ring");

        // grid visible ban đầu
        if (gridImg) { var c = gridImg.color; c.a = 1; gridImg.color = c; }
        // ring màu
        if (ringImg && gridImg) { var rc = gridImg.color; rc.a = 0.8f; ringImg.color = rc; }
    }

    void InitDefaultAlpha(float unclearedAlpha)
    {
        int n = Mathf.Min(levelCleareds.Length, bgList.Length);
        for (int i = 0; i < n; i++)
        {
            var img = bgImgs[i];
            if (!img) continue;
            var c = img.color;
            c.a = levelCleareds[i] ? 1f : unclearedAlpha;
            img.color = c;
        }
    }

    void Run(int stars) { StartCoroutine(Execute(stars)); }
    IEnumerator Execute(int stars)
    {
        ring1.localScale = Vector3.zero;
        if (ringImg) StartCoroutine(Disappear(ringImg, 0.5f, 0f, 1f));
        yield return Resize(ring1, Vector3.one * 2f, 0.3f);
        yield return new WaitForSeconds(0.05f);
        yield return Resize(ring1, Vector3.one / 5f, 0.2f);
        StartCoroutine(Resize(ring1, Vector3.one * 50f, 1.2f));
        yield return new WaitForSeconds(0.3f);
        var ps = ring1.GetComponentInChildren<ParticleSystem>();
        if (ps) ps.Play();
        yield return new WaitForSeconds(0.8f);
        InitDefaultAlpha(1f);
        if (gridImg) yield return Disappear(gridImg, 0.5f, 1f, 0f);

        int idx = Mathf.Clamp(GameData.currentLevel - 1, 0, bgImgs.Length - 1);
        if (bgImgs[idx]) yield return Disappear(bgImgs[idx], 1f, 0f, 1f);
        yield return new WaitForSeconds(1.1f);
        gameOver?.GameOverPopup(stars);
    }

    void RunGridAppears() { StartCoroutine(GridAppears()); }
    IEnumerator GridAppears()
    {
        yield return Resize(gridImage, Vector3.one * 1.1f, 0f);
        StartCoroutine(Resize(gridImage, Vector3.one, 0.2f));
        if (gridImg) yield return Disappear(gridImg, 0.5f, 0f, 1f);
    }

    IEnumerator Resize(Transform tr, Vector3 to, float dur)
    {
        var from = tr.localScale;
        float t = 0f;
        while (t < dur)
        {
            float k = dur <= 0f ? 1f : t / dur;
            tr.localScale = Vector3.Lerp(from, to, k);
            t += Time.deltaTime;
            yield return null;
        }
        tr.localScale = to;
    }

    IEnumerator Disappear(Image img, float dur, float fromA, float toA)
    {
        if (!img) yield break;
        var c = img.color; float t = 0f;
        while (t < dur)
        {
            c.a = Mathf.Lerp(fromA, toA, dur <= 0f ? 1f : t / dur);
            img.color = c; t += Time.deltaTime;
            yield return null;
        }
        c.a = toA; img.color = c;
    }

    bool[] GetBoolClearedLevels(int stageID)
    {
        if (GameData.stageLevelDict == null) GameEvents.LoadPlayer();
        int count = GameData.stageLevelDict.TryGetValue(stageID, out var n) ? n : 0;
        var levelCleared = new bool[count];
        for (int i = 1; i <= count; i++)
            levelCleared[i - 1] = GameData.playerLevelData.TryGetValue((stageID, i), out var v) && v > 0;
        return levelCleared;
    }
}
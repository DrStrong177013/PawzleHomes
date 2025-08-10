// UploadBugs – focus vào tại sao shape "đổi hình dạng"
// Chỉ log: Canvas/CanvasScaler, scale chain, lossyScale, và vài triangle mẫu.
// Những log khác được comment để giữ màn hình gọn.
//
// Gợi ý dùng:
// - Đặt dumpAllShapes=false và extraShapeIndices=[0,1,...] để soi vài shape nghi án.
// - Xem kỹ "scaleChain" và "lossyScale" giữa Editor vs Device.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UploadBugs : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] TMP_Text label;
    [SerializeField] RectTransform grid;          // nếu cần so sánh, có thể bật log cho grid
    [SerializeField] ShapeStorage storage;
    [SerializeField] PuzzleBackground bg;

    [Header("Ticker")]
    [SerializeField] float tickSeconds = 0.35f;
    [SerializeField] bool logToConsole = true;
    [SerializeField] bool onlyLogWhenChanged = false;

    [Header("Shape dump")]
    [SerializeField] bool dumpAllShapes = false;  // chỉ soi shape nghi án
    [SerializeField] int maxShapes = 6;
    [SerializeField] int[] extraShapeIndices = new int[] { 0, 1 };

    [Header("Triangles sample")]
    [SerializeField] int sampleTriangles = 4;     // số triangle mẫu để log chi tiết

    [Header("Draw outlines (tùy chọn)")]
    [SerializeField] bool drawOutlines = false;   // mặc định tắt để tập trung scale
    [SerializeField] Color shapeOutline = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] float outlineDuration = 0.03f;

    StringBuilder sb;
    string _last;

    // cache corners để hạn GC
    static readonly Vector3[] _corners = new Vector3[4];

    void OnEnable()
    {
        sb = new StringBuilder(2048);
        StartCoroutine(Ticker());
    }

    IEnumerator Ticker()
    {
        var w = new WaitForSeconds(tickSeconds);
        while (true)
        {
            sb.Clear();

            // ===== Header gọn (các log khác tạm tắt) ==========================
            // sb.AppendLine(GameData.testStage);
            // sb.AppendLine($"safe={Screen.safeArea} screen={Screen.width}x{Screen.height}");
            // sb.AppendLine($"bg.enabled={(bg ? bg.enabled : false)}");

            // ===== GRID (comment mặc định) ====================================
            // if (grid) DumpRectBasic("grid", grid, sb, drawOutlines ? (Color?)shapeOutline : null);
            // if (grid) { DumpCanvasInfo(grid, sb); DumpScaleChain(grid, sb, 5); }

            // ===== SHAPES ======================================================
            DumpShapes(sb);

            // ===== UI + Log ====================================================
            string txt = sb.ToString();
            //if (label) label.text = txt;
            //if (logToConsole && (!onlyLogWhenChanged || !string.Equals(txt, _last, StringComparison.Ordinal)))
            //    Debug.Log(txt);
            //_last = txt;
            if (DebugSidebar.instance)
                DebugSidebar.instance.SetLog(txt);

            yield return w;
        }
    }

    void DumpShapes(StringBuilder s)
    {
        if (!storage || storage.shapeList == null)
        {
            s.AppendLine("shapes=<storage null/empty>");
            return;
        }

        List<int> indices = new List<int>();
        if (dumpAllShapes)
        {
            int limit = Mathf.Min(storage.shapeList.Count, maxShapes);
            for (int i = 0; i < limit; i++) indices.Add(i);
        }
        if (extraShapeIndices != null)
        {
            foreach (var id in extraShapeIndices)
                if (id >= 0 && id < storage.shapeList.Count && !indices.Contains(id))
                    indices.Add(id);
        }
        if (indices.Count == 0) indices.Add(0);

        foreach (int idx in indices) DumpShape(idx, s);
    }

    void DumpShape(int index, StringBuilder s)
    {
        if (storage == null || storage.shapeList == null || storage.shapeList.Count <= index)
        {
            s.AppendLine($"shape[{index}]=<none>");
            return;
        }

        var shape = storage.shapeList[index];
        if (!shape) { s.AppendLine($"shape[{index}]=<null ref>"); return; }

        var rt = shape.GetComponent<RectTransform>();
        s.AppendLine($"---- shape[{index}] {shape.name} ----");
        s.AppendLine($"active={shape.gameObject.activeInHierarchy} onDrag={shape._isOnDrag}");

        DumpRectBasic($"shape[{index}]", rt, s, drawOutlines ? (Color?)shapeOutline : null);
        DumpCanvasInfo(rt, s);
        DumpScaleChain(rt, s, 6);
        DumpTrianglesSample(shape, s, sampleTriangles);

        // Tổng hợp nhanh
        int triCount = shape._currentTriangles?.Count ?? 0;
        int triActive = shape._currentTriangles?.Count(go => go && go.activeSelf) ?? 0;
        s.AppendLine($"tris={triCount} activeTris={triActive}");
        s.AppendLine();
    }

    // ======================= CORE LOG HELPERS ================================

    // GỌN: chỉ những thứ liên quan scale/hình dạng
    void DumpRectBasic(string name, RectTransform rt, StringBuilder s, Color? outline = null)
    {
        if (!rt) { s.AppendLine($"{name}=<null RT>"); return; }

        // Anchors/pivot/anchored để thấy layout, nhưng không in corners/onScreen
        s.AppendLine($"{name}: anchored={rt.anchoredPosition} size={rt.rect.size} " +
                     $"pivot={rt.pivot} aMin={rt.anchorMin} aMax={rt.anchorMax} " +
                     $"localScale={rt.localScale} lossyScale={rt.lossyScale} " +
                     $"rotZ={rt.localEulerAngles.z:F1}");

        if (drawOutlines && outline.HasValue)
        {
            rt.GetWorldCorners(_corners);
            DrawWorldRectOutline(_corners, outline.Value);
        }
    }

    void DumpCanvasInfo(RectTransform rt, StringBuilder s)
    {
        if (!rt) { s.AppendLine("canvas=<null>"); return; }
        var canvas = rt.GetComponentInParent<Canvas>();
        if (!canvas) { s.AppendLine("canvas=<none>"); return; }

        var scaler = canvas.GetComponent<CanvasScaler>();
        s.AppendLine($"canvas: mode={canvas.renderMode} scaleFactor={canvas.scaleFactor:F3} " +
                     $"pixelPerfect={canvas.pixelPerfect} sortingOrder={canvas.sortingOrder}");

        if (scaler)
        {
            s.AppendLine($"scaler: uiMode={scaler.uiScaleMode} refRes={scaler.referenceResolution} " +
                         $"matchWH={scaler.matchWidthOrHeight:F2} ppu={scaler.dynamicPixelsPerUnit:F2}");
        }
    }

    void DumpScaleChain(RectTransform rt, StringBuilder s, int maxUp = 6)
    {
        if (!rt) { s.AppendLine("scaleChain=<null>"); return; }
        Transform t = rt;
        int i = 0;
        s.AppendLine("scaleChain (name  local -> lossy):");
        while (t != null && i < maxUp)
        {
            Vector3 ls = t.localScale;
            Vector3 lz = t.lossyScale;
            s.AppendLine($"  {i}. {t.name}  local={ls}  lossy={lz}");
            t = t.parent;
            i++;
        }
    }

    void DumpTrianglesSample(Shape shape, StringBuilder s, int howMany)
    {
        var list = shape?._currentTriangles;
        if (list == null || list.Count == 0) { s.AppendLine("tri[0..] = <none>"); return; }

        int n = Mathf.Min(howMany, list.Count);
        s.AppendLine($"tri-sample (0..{n - 1}):");
        for (int i = 0; i < n; i++)
        {
            var go = list[i];
            if (!go) { s.AppendLine($"  tri[{i}]=<null>"); continue; }
            var rt = go.GetComponent<RectTransform>();
            if (!rt) { s.AppendLine($"  tri[{i}]=<no RT>"); continue; }

            var img = go.GetComponent<Image>();
            string spriteInfo = "";
            if (img && img.sprite) spriteInfo = $" spritePPU={img.sprite.pixelsPerUnit} preserve={img.preserveAspect}";
            s.AppendLine($"  tri[{i}] localPos={rt.localPosition} rotZ={rt.localEulerAngles.z:F1} " +
                         $"size={rt.rect.size} localScale={rt.localScale} lossyScale={rt.lossyScale}{spriteInfo}");
        }
    }

    // ======================= DRAW HELPERS (tùy chọn) =========================
    void DrawWorldRectOutline(Vector3[] c, Color col)
    {
        Debug.DrawLine(c[0], c[1], col, outlineDuration, false); // BL->TL
        Debug.DrawLine(c[1], c[2], col, outlineDuration, false); // TL->TR
        Debug.DrawLine(c[2], c[3], col, outlineDuration, false); // TR->BR
        Debug.DrawLine(c[3], c[0], col, outlineDuration, false); // BR->BL
        Debug.DrawLine(c[0], c[2], col, outlineDuration, false); // chéo để nhìn orientation
    }
}

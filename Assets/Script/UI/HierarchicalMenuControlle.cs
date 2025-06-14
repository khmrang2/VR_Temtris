using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HierarchicalMenuController : MonoBehaviour
{
    [Header("프리팹")]
    public GameObject panelPrefab;           // PanelUI
    public GameObject buttonPrefab;          // MenuButton
    public float panelGap = 0.45f;      // 패널 X 간격(m)

    [Header("왼쪽·위 오프셋 (m)")]
    [Tooltip("음수 = 왼쪽, 양수 = 오른쪽. 0 으로 두면 패널 폭의 -½ 로 자동 설정")]
    public float offsetLeft = 0f;

    [Tooltip("양수 = 위쪽, 음수 = 아래쪽. 0 으로 두면 패널 높이의 ½ 로 자동 설정")]
    public float offsetUp = 0f;

    /* ── 내부 ─────────────────────────────── */
    Transform panelsRoot;
    readonly List<GameObject> activePanels = new();

    /*────────────────────────────────────────*/
    void Start()
    {
        /* PanelsRoot 생성 */
        panelsRoot = new GameObject("PanelsRoot").transform;
        panelsRoot.SetParent(transform, false);

        /* 오프셋 자동 계산 (필드값이 0 일 때만) */
        if (panelPrefab && (Mathf.Approximately(offsetLeft, 0f) || Mathf.Approximately(offsetUp, 0f)))
        {
            RectTransform rt = panelPrefab.GetComponent<RectTransform>();
            Vector3 s = panelPrefab.transform.localScale;     // 보통 0.001

            float widthM = rt.rect.width * s.x;
            float heightM = rt.rect.height * s.y;

            if (Mathf.Approximately(offsetLeft, 0f)) offsetLeft = -widthM * 0.5f;
            if (Mathf.Approximately(offsetUp, 0f)) offsetUp = heightM * 0.5f;
        }
    }

    /* 최상위 메뉴 열기 */
    public void Open(MenuNode root) { ClearAll(); if (root != null) SpawnPanel(root, 0); }

    /* 패널 생성 */
    void SpawnPanel(MenuNode node, int level)
    {
        if (node == null) return;

        GameObject panelGO = Instantiate(panelPrefab, panelsRoot);
        panelGO.transform.SetParent(panelsRoot, false);  // ⭐ 부모 강제 고정!

        /* 왼쪽·위 오프셋 + level*panelGap 우측 이동 */
        Vector3 localPos = new Vector3(level * panelGap + offsetLeft, offsetUp, 0f);
        panelGO.transform.localPosition = localPos;

        TMP_Text title = panelGO.transform.Find("Title")?.GetComponent<TMP_Text>();
        if (title)
        {
            if (node.label == "Scenes")
            {
                var mapManager = FindObjectOfType<MapSelectionManager>();
                if (mapManager != null)
                {
                    title.text = $"{node.label} ({mapManager.SelectedMap})";
                }
                else
                {
                    title.text = $"{node.label} (No MapManager)";
                }
            }
            else
            {
                title.text = node.label;
            }
        }

        activePanels.Add(panelGO);

        Transform content = panelGO.transform.Find("Content") ?? panelGO.transform;

        for (int i = 0; i < node.children.Count; i++)
        {
            var child = node.children[i];
            if (child == null) continue;

            GameObject btn = Instantiate(buttonPrefab, content);
            btn.GetComponent<MenuButtonUI>().Set(child.label, child.icon);

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                RemovePanelsAfter(level);
                if (child.children != null && child.children.Count > 0)
                    SpawnPanel(child, level + 1);
                else 
                    child.onSelect?.Invoke();

                if (child.closeMenuOnSelect)
                {
                    // ★ 이 child가 closeMenuOnSelect == true 면만 닫는다!
                    ClearAll();
                }

            });
        }
    }

    /* 이후 메서드 동일 */
    void RemovePanelsAfter(int level)
    {
        while (activePanels.Count > level + 1)
        {
            Destroy(activePanels[^1]);
            activePanels.RemoveAt(activePanels.Count - 1);
        }
    }
    public void ClearAll()
    {
        foreach (var p in activePanels) Destroy(p);
        activePanels.Clear();
    }
}

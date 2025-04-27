using UnityEngine;

public class FixedGameMenuManager : MonoBehaviour
{
    public Transform head;
    public float spawnDistance = 1f;

    public GameObject menu;                       // Menu 앵커
    public HierarchicalMenuController hMenu;      // Menu 오브젝트에 붙어 있음
    public MenuNode rootNode;                     // 최상위 메뉴 데이터

    void Awake()
    {
        if (head == null) Debug.LogError("[GMM] head(카메라) 미지정");
        if (menu == null) Debug.LogError("[GMM] menu 오브젝트 미지정");
        if (hMenu == null) Debug.LogError("[GMM] HierarchicalMenuController 미지정");
        if (rootNode == null) Debug.LogWarning("[GMM] rootNode 가 null 입니다");

        SetupMenuPosition();
    }

    void SetupMenuPosition()
    {
        if (head == null || menu == null) return;

        Vector3 dir = head.forward.normalized;
        menu.transform.position = head.position + dir * spawnDistance;

        menu.transform.LookAt(head.position, Vector3.up);
        menu.transform.Rotate(0f, 180f, 0f);
        Debug.Log($"[GMM] 메뉴 초기 위치 설정: {menu.transform.position}");

        menu.SetActive(true);  // 항상 활성화
    }

    void Start()
    {
        if (hMenu && rootNode)
            hMenu.Open(rootNode);
        else
            Debug.LogWarning("[GMM] hMenu 또는 rootNode 가 null. 패널 못 띄움");
    }

    public void CloseMenu()
    {
        if (menu) menu.SetActive(false);
        if (hMenu) hMenu.ClearAll();
    }
}

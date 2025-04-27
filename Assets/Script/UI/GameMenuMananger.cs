using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuManager : MonoBehaviour
{
    public Transform head;
    public float spawnDistance = 1f;

    public GameObject menu;                       // Menu 앵커
    public HierarchicalMenuController hMenu;      // Menu 오브젝트에 붙어 있음
    public MenuNode rootNode;                     // 최상위 메뉴 데이터

    public InputActionProperty showButton;

    bool isOpen;

    void Awake()
    {
        if (head == null) Debug.LogError("[GMM] head(카메라) 미지정");
        if (menu == null) Debug.LogError("[GMM] menu 오브젝트 미지정");
        if (hMenu == null) Debug.LogError("[GMM] HierarchicalMenuController 미지정");
        if (rootNode == null) Debug.LogWarning("[GMM] rootNode 가 null 입니다");
    }

    void Update()
    {
        /* ① 입력 감지 */
        if (showButton.action.WasPressedThisFrame())
        {
            isOpen = !isOpen;
            menu.SetActive(isOpen);
            Debug.Log($"[GMM] {(isOpen ? "메뉴 ON" : "메뉴 OFF")}");

            if (isOpen)
            {
                if (head == null) return;

                /* ② 위치 배치 */
                Vector3 dir = head.forward.normalized;
                menu.transform.position = head.position + dir * spawnDistance;

                menu.transform.LookAt(head.position, Vector3.up);
                menu.transform.Rotate(0f, 180f, 0f);
                Debug.Log($"[GMM] 메뉴 위치 설정: {menu.transform.position}");

                /* ③ 계층형 메뉴 생성 */
                if (hMenu && rootNode)
                    hMenu.Open(rootNode);
                else
                    Debug.LogWarning("[GMM] hMenu 또는 rootNode 가 null. 패널 못 띄움");
            }
            else
            {
                /* ④ Clear 패널 */
                if (hMenu) hMenu.ClearAll();
            }
        }

        /* ⑤ 항상 카메라 향하도록 */
        if (isOpen && head)
        {
            menu.transform.LookAt(head.position, Vector3.up);
            menu.transform.Rotate(0f, 180f, 0f);
        }
    }
    // GameMenuManager.cs
    public void CloseMenu()
    {
        isOpen = false;
        menu.SetActive(false);   // 전체 인터페이스 OFF
        if (hMenu) hMenu.ClearAll();
    }
}

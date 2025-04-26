using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;                       // 새 Input System
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;                                         // (선택) 자동 탐색용
using UnityEngine.UI;

[RequireComponent(typeof(ItemInfoHolder))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ShowInfoOnGrab : MonoBehaviour
{
    /* ──────────────── 인스펙터 노출 필드 ──────────────── */
    [Header("UI 프리팹")]
    [Tooltip("ItemInfoUI.prefab 을 할당")]
    public GameObject uiPrefab;                      // 월드-스페이스 Canvas 프리팹

    [Header("UI Anchor")]
    [Tooltip("오브젝트 자식 중 UIAnchor 라는 빈 GameObject")]
    public Transform uiAnchor;                       // 없는 경우 Awake에서 자동 탐색

    [Header("토글 입력 바인딩")]
    [Tooltip("키보드(F) + XR RightController primaryButton")]
    public string keyboardToggleKey = "<Keyboard>/f";
    public string xrToggleBinding = "<XRController>{RightHand}/primaryButton";

    [Header("UI 초기 스케일")]
    public float uiScale = 0.002f;                   // 1 = 1 m, 0.002 ≈ 2 mm

    /* ──────────────── 내부 상태 ──────────────── */
    XRGrabInteractable grab;
    InputAction toggleAction;
    GameObject spawnedUI;

    /* ──────────────────────── */
    void Awake()
    {
        /* 1) 필수 컴포넌트 참조 */
        grab = GetComponent<XRGrabInteractable>();

        /* 2) UIAnchor 자동 탐색 */
        if (uiAnchor == null)
        {
            uiAnchor = transform.Find("UIAnchor");
            if (uiAnchor == null)
                Debug.LogError($"[{name}] 'UIAnchor' 자식을 찾지 못했습니다!");
        }

        /* 3) Grab 이벤트 등록 */
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        /* 4) 토글 입력 세팅 */
        toggleAction = new InputAction(type: InputActionType.Button);
        toggleAction.AddBinding(keyboardToggleKey);
        toggleAction.AddBinding(xrToggleBinding);
        toggleAction.Enable();

        Debug.Log($"[{name}] ShowInfoOnGrab 초기화 완료");
    }

    /* ──────────────────────── */
    void Update()
    {
        if (spawnedUI && toggleAction.WasPressedThisFrame())
        {
            bool newState = !spawnedUI.activeSelf;
            spawnedUI.SetActive(newState);
            Debug.Log($"[{name}] [Toggle] UI {(newState ? "보임" : "숨김")}");
        }
    }

    /* ───────────────── Grab / Release ───────────────── */
    void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log($"[{name}] Grab by {args.interactorObject.transform.name}");

        if (spawnedUI || uiPrefab == null || uiAnchor == null) return;

        /* 1) UI 생성—위치·회전·부모 모두 지정 */
        spawnedUI = Instantiate(
            uiPrefab,
            uiAnchor.position,
            uiAnchor.rotation,
            uiAnchor             // ← parent 지정 필수
        );

        /* 2) 로컬 트랜스폼 정리 */
        Transform t = spawnedUI.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one * uiScale;

        /* 3) 데이터 바인딩 */
        var holder = GetComponent<ItemInfoHolder>();
        if (holder && holder.info)
        {
            spawnedUI.GetComponent<UIDisplay>()?.Setup(holder.info);
            Debug.Log($"[{name}] UI 생성 & ItemInfo 적용: {holder.info.itemName}");
        }
        else
        {
            Debug.LogWarning($"[{name}] ItemInfo 가 비어 있습니다");
        }


        /* 4) 기본 상태: 일단 보이도록 (필요 시 false 로 변경) */
        spawnedUI.SetActive(false);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log($"[{name}] Release");

        if (spawnedUI)
        {
            Destroy(spawnedUI);
            spawnedUI = null;
            Debug.Log($"[{name}] UI 파괴");
        }
    }
}

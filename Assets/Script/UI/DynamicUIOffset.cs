using UnityEngine;

/// <summary>
/// LateUpdate마다 카메라→UIAnchor 레이캐스트를 실행해
/// 오브젝트에 가려지면 margin 만큼 앞으로 UI를 밀어낸다.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class DynamicUIOffset : MonoBehaviour
{
    [Tooltip("UI가 표면에서 떨어질 거리(m)")]
    public float margin = 0.03f;

    [Tooltip("가려짐을 검사할 레이어 마스크\n(자기 오브젝트가 포함된 레이어로 설정)")]
    public LayerMask obstacleLayers = ~0;   // 기본 = Everything

    Transform cam;        // Main Camera
    Transform anchor;     // UIAnchor (부모)

    void Awake()
    {
        cam = Camera.main.transform;
        anchor = transform.parent;          // UIAnchor
    }

    void LateUpdate()
    {
        if (cam == null || anchor == null) return;

        // 카메라 → 앵커 방향
        Vector3 dir = (anchor.position - cam.position).normalized;
        float dist = Vector3.Distance(cam.position, anchor.position);

        // Raycast: 카메라에서 앵커까지
        bool hit = Physics.Raycast(cam.position, dir, out RaycastHit info, dist, obstacleLayers);

        // ① 오브젝트에 가려진 경우 → 부딪힌 지점 + margin 만큼 앞
        if (hit)
            transform.position = info.point + dir * margin;
        // ② 가려지지 않았으면 원래 앵커 위치
        else
            transform.position = anchor.position;

        // 항상 카메라 향하도록
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position,
                                                     Vector3.up);
    }
}

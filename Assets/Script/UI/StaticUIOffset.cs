using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class StaticUIOffset : MonoBehaviour
{
    [Tooltip("모델 표면에서 UI까지 거리(m)")]
    public float margin = 0.03f;

    Collider col;     // 부모(아이템) 콜라이더
    Transform cam;

    void Start()
    {
        cam = Camera.main?.transform;
        col = GetComponentInParent<Collider>();

        if (cam == null || col == null)
        {
            Debug.LogWarning("[StaticUIOffset] 카메라 또는 Collider 없음");
            return;
        }
        PositionUI();          // ← 한 번만 실행
    }

    public void PositionUI()
    {
        // ① 카메라에 가장 가까운 콜라이더 표면점
        Vector3 surface = col.ClosestPoint(cam.position);

        // ② 카메라 → 표면 방향
        Vector3 dir = (surface - cam.position).normalized;

        // ③ dir 쪽으로 margin 만큼 앞에 배치
        transform.position = surface + dir * margin;

        // ④ 항상 카메라 보도록 회전
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position,
                                                     Vector3.up);
    }
}

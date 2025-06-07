using UnityEngine;

public class AttachableOnCollision : MonoBehaviour
{
    public bool canAttach = false;
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    // 해당 오브젝트의 collider가 다른 collider와 부딪히면 실행됨
    private void OnCollisionEnter(Collision collision)
    {
        if (!canAttach) return;

        GameObject other = collision.gameObject;
        if (((1 << other.layer) & blockLayer) == 0 || other == gameObject) return;

        BlockConnectHelper.ConnectByCollision(gameObject, other);

        canAttach = false;
        Debug.Log($"{other.name} 와(과)의 충돌로 붙었습니다.");
    }

    // 블럭이 붙을 수 있는 상태로 변경    *풀 아이템 적용 시 사용
    public void ActivateAttachable()
    {
        canAttach = true;
        Debug.Log($"블럭 {gameObject.name} 붙을 준비 완료");
    }

    // FixedJoint 제거, 부모-자식 관계 해제   *블럭 자를 시 사용
    public void DetachFromParent()
    {
        FixedJoint joint = GetComponent<FixedJoint>();
        if (joint != null)
        {
            Destroy(joint);
            Debug.Log($"블럭 {gameObject.name} 의 FixedJoint 해제됨");
        }

        if (transform.parent != null) // 자식 오브젝트 기준 관계 해제
        {
            transform.SetParent(null, true); // world position 유지
            Debug.Log($"블럭 {gameObject.name} 의 부모 연결 해제됨");
        }
        else if (transform.childCount > 0) // 부모 오브젝트 기준 관계 해제
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                transform.GetChild(i).SetParent(null, true);
            }
            Debug.Log($"블럭 {gameObject.name} 의 자식 연결 해제됨");
        }
    }
}

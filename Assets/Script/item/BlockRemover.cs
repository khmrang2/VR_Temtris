using UnityEngine;

public class BlockRemover : MonoBehaviour
{
    public int removeCount; // 삭제 횟수
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        if (((1 << other.layer) & blockLayer) == 0 || other == gameObject) return;

        var temp = other.GetComponent<AttachableOnCollision>();
        temp?.DetachFromParent(); // rigid나 부모-자식 관계가 있다면 다 해제함.

        // 블럭 제거 이펙트가 있어도 좋을 듯
        Destroy(other);

        Debug.Log($"블럭 {other.name} 삭제 완료.");

        removeCount--;

        if(removeCount < 1)
        {
            // 아이템 제거 이펙트 있어도 좋을 듯
            Destroy(gameObject);
            Debug.Log($"아이템 '지우개' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"블럭 삭제 횟수 {removeCount} 남음.");

    }
}

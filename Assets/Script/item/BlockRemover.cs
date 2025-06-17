using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class BlockRemover : MonoBehaviour, IHandGrabUseDelegate
{
    public int removeCount; // 삭제 횟수
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    [SerializeField] private GameObject _cur_block = null;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void BeginUse()
    {
    }

    public float ComputeUseStrength(float strength)
    {
        Debug.LogAssertion("힘 측정은 되나?");
        if (strength > 0.5)
        {
            Debug.LogAssertion("이제 이게 안되네");
            OnTriggerPressed();
        }
        return strength;
    }

    public void EndUse()
    {
        Debug.LogAssertion("[Eraser] 끝남.");
        OnTriggerReleased();
    }

    public void OnTriggerPressed()
    {
        var temp = _cur_block.GetComponent<AttachableOnCollision>();
        temp?.DetachFromParent(); // rigid나 부모-자식 관계가 있다면 다 해제함.

        // 블럭 제거 이펙트가 있어도 좋을 듯
        Destroy(_cur_block);

        Debug.Log($"블럭 {_cur_block.name} 삭제 완료.");

        removeCount--;
    }

    public void OnTriggerReleased()
    {
        Debug.Log("[Eraser.cs] 트리거 릴리즈 감지");
        _rb.useGravity = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        if (((1 << other.layer) & blockLayer) == 0 || other == gameObject) return;
        _cur_block = other;

    }

    private void OnCollisionExit(Collision collision)
    {
        _cur_block = null;
        Debug.LogAssertion("[Glue] 콜라이전에 나감.");
        if (removeCount < 1)
        {
            // 아이템 제거 이펙트 있어도 좋을 듯
            Destroy(gameObject);
            Debug.Log($"아이템 '지우개' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"블럭 삭제 횟수 {removeCount} 남음.");
    }
}

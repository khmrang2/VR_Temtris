using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Events;

public class Glue : MonoBehaviour
{
    public int attachCount; // 붙일 횟수
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    [SerializeField] private GameObject _cur_block = null;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void OnTriggerPressed(PointerEvent evt)
    {
        Debug.Log("[Glue.cs] 트리거 입력 감지");

        var temp = _cur_block.GetComponent<AttachableOnCollision>();
        if (!temp.canAttach)
        {
            temp.canAttach = true;
            attachCount--;
            Debug.Log($"블럭 {_cur_block.name} 붙일 준비 완료.");
        }
        else return;

        if (attachCount < 1)
        {
            Destroy(gameObject);
            Debug.Log($"아이템 '풀' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"블럭 붙일 횟수 {attachCount} 남음.");


        // 중력 끄기
        _rb.useGravity = false;
    }

    public void OnTriggerReleased(PointerEvent evt)
    {
        Debug.Log("[Glue.cs] 트리거 릴리즈 감지");
        _rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject block = collision.gameObject;
        if (((1 << block.layer) & blockLayer) == 0 || block == gameObject) return;
        _cur_block = block;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (attachCount < 1)
        {
            Destroy(gameObject);
            Debug.Log($"아이템 '풀' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"블럭 붙일 횟수 {attachCount} 남음.");
    }
}

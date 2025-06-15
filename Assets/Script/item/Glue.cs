using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Events;
using Oculus.Interaction.HandGrab;

public class Glue : MonoBehaviour, IHandGrabUseDelegate
{
    public int attachCount; // 붙일 횟수
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    [SerializeField] private GameObject _cur_block = null;
    [SerializeField] private AudioSource _audioSource = null;

    private Rigidbody _rb;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
    }

    public void OnTriggerPressed()
    {
        if (_cur_block == null)
        {
            Debug.LogWarning("[Glue] 블럭에 붙지 않았음 (_cur_block == null)");
            return;
        }
        Debug.Log("[Glue.cs] 트리거 입력 감지");

        var temp = _cur_block.GetComponent<AttachableOnCollision>();
        if (!temp.canAttach)
        {
            temp.canAttach = true;
            attachCount--;
            Debug.Log($"[Glue] 블럭 {_cur_block.name} 붙일 준비 완료.");
        }
        else return;

        if (attachCount < 1)
        {
            Destroy(gameObject);
            Debug.Log($"[Glue] 아이템 '풀' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"[Glue] 블럭 붙일 횟수 {attachCount} 남음.");


        // 중력 끄기
        _rb.useGravity = false;
        _audioSource.Play();
    }

    public void OnTriggerReleased()
    {
        Debug.Log("[Glue.cs] 트리거 릴리즈 감지");
        _rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogAssertion("[Glue] 콜라이전에 들어갔는가?");
        GameObject block = collision.gameObject;
        if (((1 << block.layer) & blockLayer) == 0 || block == gameObject || block.layer != 3) return;
        _cur_block = block;
    }

    private void OnCollisionExit(Collision collision)
    {
        _cur_block = null;
        Debug.LogAssertion("[Glue] 콜라이전에 나감.");
        if (attachCount < 1)
        {
            Destroy(gameObject);
            Debug.Log($"[Glue] 아이템 '풀' {gameObject.name} 삭제 완료.");
            return;
        }

        Debug.Log($"[Glue] 블럭 붙일 횟수 {attachCount} 남음.");
    }

    public void BeginUse()
    {
    }

    public void EndUse()
    {
        Debug.LogAssertion("[Glue] 끝남.");
        OnTriggerReleased();
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
}

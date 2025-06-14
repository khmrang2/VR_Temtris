using System.Collections;
using UnityEngine;

public class AttachableOnCollision : MonoBehaviour
{
    public bool canAttach = false;
    public LayerMask blockLayer; // layer를 block으로 설정해야함

    [Header("Attach 상태 표시용 파티클")]
    public ParticleSystem attachEffectPrefab; // 인스펙터에 프리팹 연결
    private ParticleSystem currentEffect; // 현재 이 블럭에 붙은 이펙트

    private void Update()
    {
        if (canAttach)
        {
            if (currentEffect == null && attachEffectPrefab != null)
            {
                currentEffect = Instantiate(attachEffectPrefab, transform.position, Quaternion.identity);
                currentEffect.transform.SetParent(transform);
                currentEffect.Play();
            }
        }
        else
        {
            if (currentEffect != null)
            {
                currentEffect.Stop();
                currentEffect.transform.SetParent(null);
                Destroy(currentEffect.gameObject, 2f); // 수명 끝나면 제거
                currentEffect = null;
            }
        }
    }

    // 해당 오브젝트의 collider가 다른 collider와 부딪히면 실행됨
    private void OnCollisionEnter(Collision collision)
    {
        if (!canAttach) return;
        if (((1 << collision.gameObject.layer) & blockLayer) == 0) return;
        if (collision.transform.IsChildOf(transform) || transform.IsChildOf(collision.transform)) return;

        GameObject other = collision.gameObject;
        if (((1 << other.layer) & blockLayer) == 0 || other == gameObject) return; // layer가 blockLayer가 아니면 종료

        ContactPoint contact = collision.contacts[0];
        Vector3 contactNormal = contact.normal;

        // 충돌 지점의 면이 앞/뒤면일시 종료
        if (BlockConnectHelper.IsForwardOrBackward(contactNormal, transform))
        {
            Debug.Log("앞/뒤 방향 블럭은 붙일 수 없습니다");
            StartCoroutine(ResetCanAttachAfterDelay(0.2f)); // 0.2초 쿨타임
            return;
        }

        BlockConnectHelper.ConnectByCollision(gameObject, other); // 내부에서 다시 앞.뒤면 확인, 아닐 시 오류를 발생시킴

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

    // 딜레이 주기
    private IEnumerator ResetCanAttachAfterDelay(float delay)
    {
        canAttach = false;
        yield return new WaitForSeconds(delay);
        canAttach = true;
    }
}

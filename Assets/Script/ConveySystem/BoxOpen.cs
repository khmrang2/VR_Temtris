using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine.UIElements;


public class BoxOpen : MonoBehaviour, IHandGrabUseDelegate
{
    public Animator animator;
    public Transform itemSpawnPoint;        // 랜덤 블럭 생성 위치
    public GameObject[] itemPrefabs;        // 블럭 프리팹 목록

    [SerializeField] private bool isOpened = false;      // 박스가 열렸는 지
    [SerializeField] private bool heldByGripper = true;      // Gripper에 의해 붙잡히고 있는 지
    [SerializeField] private Gripper holdingGripper = null;      // 붙잡히고 있는 Gripper 참조

    private int _itemIndex = 0;


    #region Hand grab 이벤트 구현

    public void OnSelect()
    {
        if (heldByGripper && holdingGripper != null)        // Gripper가 박스를 놓게 함
        {
            Debug.Log("[BoxOpen] 플레이어가 박스를 손으로 잡았습니다.");
            holdingGripper.Release(); // Gripper에서 박스를 놓게 하는 함수
            SetHeldByGripper(false);
        }    
    }

    public void OnUnSelect()
    {
        Debug.Log("[BoxOpen] 플레이어가 박스를 놧습니다.");
    }

    #endregion
    #region Hand Use After Grab 이벤트 구현
    public void BeginUse()
    {
    }

    public void EndUse()
    {
    }

    public float ComputeUseStrength(float strength)
    {
        if(strength > 0.5)
        {
            TryOpen();
        }
        return strength;
    }
    #endregion

    public void TryOpen()       // 박스를 오픈
    {
        Debug.LogWarning("[BoxOpen] : Try open callingg");
        if (isOpened || heldByGripper)  // 이미 열려있거나 Gripper에 붙잡혀 있다면
        {
            Debug.Log("이미 열렸거나 Gripper가 붙잡고 있습니다.");
            return;
        }

        isOpened = true;        // 열림 상태 설정
        animator.SetTrigger("OpenBox");     // 애니메이션 재생
        StartCoroutine(Wait(0.7f));
        SpawnItemWithEffect();
        StartCoroutine(Wait(0.1f));
        gameObject.SetActive(false);        // 박스 비활성화
    }

    public void ForcedOpen()       // 강제 오픈, gripper가 최종 위치까지 도달하였을 때 사용, 애니메이션 미출력
    {
        if (isOpened || heldByGripper)  // 이미 열려있거나 Gripper에 붙잡혀 있다면
        {
            Debug.Log("이미 열려있거나 Gripper가 붙잡고 있지만 강제 오픈이 실행되었습니다.");
            return;
        }

        isOpened = true;        // 열림 상태 설정
        gameObject.SetActive(false);        // 박스 비활성화

        // 강제 상황에서 아이템 index라면 (예: 7 이상), 블록 index로 대체
        if (_itemIndex >= 7)
        {
            _itemIndex = Random.Range(0, 7); // 블록 인덱스만
            Debug.Log($"[ForcedOpen] 아이템 index({_itemIndex}) 무시됨 → 블록 index({_itemIndex})로 대체");
        }

        SpawnItemWithEffect();
    }

    public void SpawnItemWithEffect()       // 이펙트 재생
    {
        GameObject effect = ObjectPoolManager.Instance.SpawnFromPool("SmokeEffect", transform.position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();


        if (itemPrefabs != null && itemPrefabs.Length > 0 && _itemIndex >= 0 && _itemIndex < itemPrefabs.Length)
        {
            GameObject prefab = itemPrefabs[_itemIndex];
            Vector3 localDir;
            Quaternion baseRotation = prefab.transform.rotation;
            if (prefab.name == "Block_J" || prefab.name == "Block_L")
            {
                localDir = baseRotation * Vector3.forward;
            }
            else
            {
                localDir = baseRotation * Vector3.up;
            }
            Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), localDir);
            Quaternion finalRotation = randomRotation * baseRotation;
            Instantiate(itemPrefabs[_itemIndex], itemSpawnPoint.position, finalRotation);
        }
        else
        {
            Debug.LogWarning($"[BoxOpen] 잘못된 itemIndex: {_itemIndex}");
        }
        // 실제 item(=tetrio)를 생성하는 부분. 
        /*if (itemPrefabs.Length > 0)
        {
            // seed를 통해서 Random.initState(int)로 시드를 항상 고정 시키자.
            int rand = Random.Range(0, itemPrefabs.Length);
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)); // Z축만 랜덤하게 설정
            Instantiate(itemPrefabs[rand], itemSpawnPoint.position, randomRotation);
        }*/
    }

    IEnumerator Wait(float sec)  // sec초 대기
    {
        yield return new WaitForSeconds(sec);
    }

    public void SetHeldByGripper(bool isHeld, Gripper gripper = null)
    {
        heldByGripper = isHeld;
        holdingGripper = gripper;
    }

    public void ResetBox(int index)      // Pool로 돌아갈 시 초기화
    {
        _itemIndex = index;
        isOpened = false;
        heldByGripper = false;
        holdingGripper = null;
        //currentInteractor = null;
        //heldDevice = default;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

}
/*    private void OnTriggerActivated(ActivateEventArgs arg)
    {
        Debug.LogWarning("[BoxOpen] : 설정됨.");
        TryOpen();
        heldDevice = default;
    }*/

/*    private void OnSelectEntered(SelectEnterEventArgs args)     // 플레이어에 의해 Grap 당했을 때
    {
        currentInteractor = args.interactorObject;

        if (heldByGripper && holdingGripper != null)        // Gripper가 박스를 놓게 함
        {
            holdingGripper.Release();
            heldByGripper = false;
            holdingGripper = null;
        }

        // XR 장치 입력 연결 ***** 아마 수정 필요
        if (currentInteractor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            Debug.Log("[BoxOpen] : 플레이어에게 잡혔는지?");
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)       // 플레이어에 의해 Grap 해제됐을 때
    {
        Debug.Log("[BoxOpen] : 유저가 박스를 놨다! ");
        currentInteractor = null;
        heldDevice = default;
    }*/

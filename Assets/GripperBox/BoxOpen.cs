using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class BoxOpen : MonoBehaviour
{
    public Animator animator;
    public Transform itemSpawnPoint;        // 랜덤 블럭 생성 위치
    public GameObject[] itemPrefabs;        // 블럭 프리팹 목록

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private InputDevice heldDevice;

    private bool isOpened = false;      // 박스가 열렸는 지
    private bool heldByGripper = true;      // Gripper에 의해 붙잡히고 있는 지
    private Gripper holdingGripper = null;      // 붙잡히고 있는 Gripper 참조

    private void Awake()        // 플레이어에 의한 Grap 확인
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);    // 플레이어에 의해 Grap 당했을 때
        grabInteractable.selectExited.AddListener(OnSelectExited);      // 플레이어에 의해 Grap 해제됐을 때
    }

    private void OnSelectEntered(SelectEnterEventArgs args)     // 플레이어에 의해 Grap 당했을 때
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
            if (controllerInteractor.xrController is XRController xrController)
            {
                heldDevice = xrController.inputDevice;
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)       // 플레이어에 의해 Grap 해제됐을 때
    {
        currentInteractor = null;
        heldDevice = default;
    }

    private void Update()
    {
        if (currentInteractor == null || isOpened)      // 플레이어에 의한 Grap 상태가 아니거나 이미 열려있다면 return 
            return;

        // 플레이어에 의한 Grap 상태에서 트리거 버튼이 입력되면 박스를 오픈
        if (heldDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            TryOpen();
        }
    }

    public void TryOpen()       // 박스를 오픈
    {
        if (isOpened || heldByGripper)  // 이미 열려있거나 Gripper에 붙잡혀 있다면
        {
            Debug.Log("이미 열렸거나 Gripper가 붙잡고 있습니다.");
            return;
        }

        isOpened = true;        // 열림 상태 설정
        animator.SetTrigger("OpenBox");     // 애니메이션 재생
        StartCoroutine(SpawnItemWithEffect());
    }

    IEnumerator SpawnItemWithEffect()       // 애니메이션 재생
    {
        yield return new WaitForSeconds(0.7f);

        GameObject effect = ObjectPoolManager.Instance.SpawnFromPool("SmokeEffect", transform.position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        if (itemPrefabs.Length > 0)
        {
            int rand = Random.Range(0, itemPrefabs.Length);
            Instantiate(itemPrefabs[rand], itemSpawnPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);        // 박스 비활성화
    }

    public void SetHeldByGripper(bool isHeld, Gripper gripper = null)
    {
        heldByGripper = isHeld;
        holdingGripper = gripper;
    }

    public void ResetBox()      // Pool로 돌아갈 시 초기화
    {
        isOpened = false;
        heldByGripper = false;
        holdingGripper = null;
        currentInteractor = null;
        heldDevice = default;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}
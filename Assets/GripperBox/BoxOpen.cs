using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class BoxOpen : MonoBehaviour
{
    public Animator animator;
    public Transform itemSpawnPoint;
    public GameObject[] itemPrefabs;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private InputDevice heldDevice;

    private bool isOpened = false;
    private bool heldByGripper = true;
    private Gripper holdingGripper = null; //  � Gripper�� �� �ڽ��� ��� �ִ���

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;

        //  �÷��̾ ��� �����ϸ� Gripper�� Release ��û
        if (heldByGripper && holdingGripper != null)
        {
            holdingGripper.Release();
            heldByGripper = false;
            holdingGripper = null;
        }

        // ���� �� XR ��Ʈ�ѷ� ����
        if (currentInteractor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            if (controllerInteractor.xrController is XRController xrController)
            {
                heldDevice = xrController.inputDevice;
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        currentInteractor = null;
        heldDevice = default;
    }

    private void Update()
    {
        if (currentInteractor == null || isOpened)
            return;

        if (heldDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            TryOpen();
        }
    }

    public void TryOpen()
    {
        if (isOpened || heldByGripper)
        {
            Debug.Log("�ڽ��� �̹� ���Ȱų� Gripper�� ���� �ֽ��ϴ�.");
            return;
        }

        isOpened = true;
        animator.SetTrigger("OpenBox");
        StartCoroutine(SpawnItemWithEffect());
    }

    IEnumerator SpawnItemWithEffect()
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
        gameObject.SetActive(false);
    }

    public void SetHeldByGripper(bool isHeld, Gripper gripper = null)
    {
        heldByGripper = isHeld;
        holdingGripper = gripper;
    }

    public void ResetBox()
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

//using UnityEngine;
//using System.Collections;
//using UnityEngine.XR;
//using UnityEngine.XR.Interaction.Toolkit;

//public class BoxOpen : MonoBehaviour
//{
//    public Animator animator;
//    public Transform itemSpawnPoint;
//    public GameObject[] itemPrefabs;

//    private XRController controller;
//    private InputDevice device;

//    private bool isOpened = false;
//    private bool heldByGripper = true;
//    void Start()
//    {
//        // XRController�� ã�ų� �Ҵ�
//        controller = GetComponent<XRController>();

//        // �Է� ����̽� �������� (��: ������ ��Ʈ�ѷ�)
//        device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
//    }

//    public void SetHeldByGripper(bool isHeld)
//    {
//        heldByGripper = isHeld;
//    }

//    public void TryOpen()
//    {
//        if (isOpened || heldByGripper)
//        {
//            Debug.Log("�̹� ���� �ڽ��ų� Gripper�� ���� �ִ� �ڽ�");
//            return;
//        }

//        isOpened = true;
//        animator.SetTrigger("OpenBox");
//        StartCoroutine(SpawnItemWithEffect());
//    }

//    public void Update()
//    {
//        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
//            TryOpen();
//    }

//    IEnumerator SpawnItemWithEffect()
//    {
//        yield return new WaitForSeconds(0.7f);

//        GameObject effect = ObjectPoolManager.Instance.SpawnFromPool("SmokeEffect", transform.position, Quaternion.identity);

//        var ps = effect.GetComponent<ParticleSystem>();
//        if (ps != null)
//        {
//            ps.Play();
//        }

//        if (itemPrefabs.Length > 0)
//        {
//            int rand = Random.Range(0, itemPrefabs.Length);
//            Instantiate(itemPrefabs[rand], itemSpawnPoint.position, Quaternion.identity);
//        }

//        yield return new WaitForSeconds(0.1f);
//        gameObject.SetActive(false);
//    }


//    public void ResetBox()
//    {
//        isOpened = false;
//        heldByGripper = false;

//        if (animator != null)
//        {
//            animator.Rebind();
//            animator.Update(0f);
//        }
//    }
//}
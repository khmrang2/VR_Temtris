using UnityEngine;
using EzySlice;
using Oculus.Interaction;
using Unity.VisualScripting;
using Oculus.Interaction.HandGrab;

public static class SliceUtility
{
    /// <summary>
    /// 절단된 블럭에 공통 세팅 적용: 레이어, 태그, 위치, Collider, Rigidbody 등
    /// </summary>
    public static void SetupSlicedPart(GameObject part, Transform original)
    {
        part.layer = LayerMask.NameToLayer("Block");
        part.tag = "Block";

        part.transform.SetPositionAndRotation(original.position, original.rotation);
        part.transform.localScale = original.localScale;

        var meshCol = part.AddComponent<MeshCollider>();
        meshCol.convex = true;

        var rb = part.AddComponent<Rigidbody>();
        rb.useGravity = true;

        var grab = part.AddComponent<Grabbable>();
        grab.TransferOnSecondSelection = true;
        grab.InjectOptionalRigidbody(rb);

        var grabInter = part.AddComponent<HandGrabInteractable>();
        grabInter.InjectOptionalPointableElement(grab);
        grabInter.InjectRigidbody(rb);
    }
}
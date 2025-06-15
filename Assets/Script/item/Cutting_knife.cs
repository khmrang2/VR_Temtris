using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cutting_knife : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public LayerMask sliceableLayer;

    public Material crossSectionMaterial;
    public float cutForce = 2000f;

    public GameObject fallbackPrefab; // BlockSliceInfo가 없을 경우 사용할 프리팹

    public float cnt = 3.0f;
    public float sliceCooldown = 1.0f; // 슬라이스 후 대기 시간 (초)
    private float cooldownTimer = 0f;

    [SerializeField] private AudioSource _audioSource = null;

    void FixedUpdate()
    {
        // 쿨타임 감소
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.fixedDeltaTime;
            return; // 쿨타임 중엔 자르지 않음
        }
        if (cnt <= 0)
        {
            Destroy(this.gameObject);
        }
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }

    public void Slice(GameObject target)
    {
        if (target.GetComponent<MeshFilter>() == null || target.GetComponent<MeshRenderer>() == null)
        {
            Debug.LogWarning("[Slicing] 대상에 MeshFilter 또는 MeshRenderer가 없습니다. 절단 불가 대상입니다.");
            return;
        }

        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 sliceDir = endSlicePoint.position - startSlicePoint.position;

        if (velocity.magnitude < 0.01f || sliceDir.magnitude < 0.01f)
        {
            Debug.LogWarning("[Slicing] 속도나 방향 벡터가 너무 작습니다. 절단 취소.");
            return;
        }

        Vector3 planeNormal = Vector3.Cross(sliceDir, velocity).normalized;
        Vector3 planePos = endSlicePoint.position;

        SlicedHull hull = target.Slice(planePos, planeNormal, crossSectionMaterial);
        if (hull != null)
        {
            GameObject upper = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lower = hull.CreateLowerHull(target, crossSectionMaterial);

            SliceUtility.SetupSlicedPart(lower, target.transform);
            SliceUtility.SetupSlicedPart(upper, target.transform);

            Destroy(target);
        }
        else
        {
            Debug.LogWarning("Slicing failed: Hull is null. (평면이 잘못됐을 수 있음)");
        }
        cnt--;
    }
}
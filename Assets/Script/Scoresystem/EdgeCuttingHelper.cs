using UnityEngine;
using EzySlice;
using Unity.VisualScripting;
using TMPro;

public class EdgeCuttingHelper : MonoBehaviour
{
    [Header("설정")]
    public Material crossSectionMaterial;
    public GameObject cutEffectPrefab;

    /// <summary>
    /// 블럭을 절단 처리한다.
    /// </summary>
    public void Cut(GameObject block, Vector3 planePos, Vector3 planeNormal)
    {
        // 약간 위로 보정 (절단 기준이 Plane보다 아래로 가는 문제 방지)
        planePos += planeNormal * 0.01f;
        {
            if (block == null)
            {
                Debug.LogWarning("[EdgeCuttingHelper] 잘라야 할 블럭이 null입니다.");
                return;
            }

            Debug.Log($"[EdgeCuttingHelper] '{block.name}' 절단 시도 - PlanePos: {planePos}, PlaneNormal: {planeNormal}");
            SliceAndDestroy(block, planePos, planeNormal);
        }
    }

    /// <summary>
    /// 절단된 결과물 생성 및 후처리 수행
    /// </summary>
    private void SliceAndDestroy(GameObject target, Vector3 planePos, Vector3 planeNormal)
    {
        var hull = target.Slice(planePos, planeNormal);
        if (hull == null)
        {
            Debug.LogWarning($"[EdgeCuttingHelper] '{target.name}' -> 원본을 아예 삭제 해버립니다.");
            Destroy(target);
            return;
        }

        Debug.Log($"[EdgeCuttingHelper] '{target.name}' 절단 성공");

        //crossSectionMaterial = GetCrossSectionMaterial(target);
        //Debug.Log($"{target.name}의 머터리얼 : {crossSectionMaterial.name}을 복사해옵니다.");
        GameObject upper = hull.CreateUpperHull(target, crossSectionMaterial);
        GameObject lower = hull.CreateLowerHull(target, crossSectionMaterial);

        if (upper != null)
        {
            SliceUtility.SetupSlicedPart(upper, target.transform);
            Debug.Log($"[EdgeCuttingHelper] Upper 생성됨: {upper.name}");
        }

        if (lower != null)
        {
            if (cutEffectPrefab != null)
                Instantiate(cutEffectPrefab, lower.transform.position, Quaternion.identity);

            Debug.Log($"[EdgeCuttingHelper] Lower 생성됨: {lower.name} → 제거됨");
            Destroy(lower);
        }

        Destroy(target);
        Debug.Log($"[EdgeCuttingHelper] 원본 블럭 제거됨: {target.name}");
    }

    /// <summary>
    /// 단면 머티리얼 설정
    /// </summary>
    private Material GetCrossSectionMaterial(GameObject target)
    {
        return crossSectionMaterial ?? GetFirstMaterialFromTarget(target);
    }

    /// <summary>
    /// 대상 오브젝트의 첫 머티리얼 반환 (자기 자신만)
    /// </summary>
    private Material GetFirstMaterialFromTarget(GameObject target)
    {
        Renderer r = target.GetComponent<Renderer>();
        if (r == null)
        {
            Debug.LogWarning($"[GetFirstMaterialFromTarget] '{target.name}'에 Renderer가 없습니다.");
            return null;
        }

        return r.sharedMaterial;
    }
}

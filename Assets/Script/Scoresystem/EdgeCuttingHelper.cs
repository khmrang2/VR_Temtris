using UnityEngine;
using EzySlice;

public class EdgeCuttingHelper : MonoBehaviour
{
    [Header("설정")]
    public Material crossSectionMaterial;
    public GameObject cutEffectPrefab;

    public void Cut(GameObject block, Vector3 planePos, Vector3 planeNormal)
    {
        if (block == null)
        {
            Debug.LogWarning("[EdgeCuttingHelper] 잘라야 할 블럭이 null입니다.");
            return;
        }

        Debug.Log($"[EdgeCuttingHelper] '{block.name}' 절단 시도 - PlanePos: {planePos}, PlaneNormal: {planeNormal}");

        // 자식이 여러 개 있다면 CombineMeshes
        MeshFilter[] meshFilters = block.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length > 1)
        {
            Debug.Log($"[EdgeCuttingHelper] 자식 Mesh가 {meshFilters.Length}개 → 병합 후 절단");

            GameObject combined = CombineChildMeshes(block);
            if (combined == null)
            {
                Debug.LogWarning("[EdgeCuttingHelper] Mesh 병합 실패 → 절단 중단");
                return;
            }

            Destroy(block); // 원본 블럭 제거 (자식 포함)
            SliceAndDestroy(combined, planePos, planeNormal);
            //Destroy(block); // 원본 블럭 제거 (자식 포함)
        }
        else
        {
            Debug.Log($"[EdgeCuttingHelper] 자식 없음 → 원본 그대로 절단");
            SliceAndDestroy(block, planePos, planeNormal);
        }
    }

    private void SliceAndDestroy(GameObject target, Vector3 planePos, Vector3 planeNormal)
    {
        var hull = target.Slice(planePos, planeNormal);
        if (hull == null)
        {
            Debug.LogWarning($"[EdgeCuttingHelper] '{target.name}' 절단 실패 - Hull이 null입니다.");
            return;
        }

        Debug.Log($"[EdgeCuttingHelper] '{target.name}' 절단 성공");

        GameObject upper = hull.CreateUpperHull(target, crossSectionMaterial);
        GameObject lower = hull.CreateLowerHull(target, crossSectionMaterial);

        if (upper != null)
        {
            SetupSlicedPart(upper, target.transform);
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

    private GameObject CombineChildMeshes(GameObject parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] == null || meshFilters[i].sharedMesh == null) return null;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        GameObject combinedObject = new GameObject(parent.name + "_Combined");
        combinedObject.transform.position = parent.transform.position;
        combinedObject.transform.rotation = parent.transform.rotation;
        combinedObject.transform.localScale = parent.transform.localScale;

        var mf = combinedObject.AddComponent<MeshFilter>();
        mf.mesh = combinedMesh;

        var mr = combinedObject.AddComponent<MeshRenderer>();
        mr.material = GetCrossSectionMaterial(parent);

        return combinedObject;
    }

    private void SetupSlicedPart(GameObject part, Transform original)
    {
        // 유저는 막고, 블록은 들어가게 구분짓기 위한 레이어 추가.
        part.layer = LayerMask.NameToLayer("Block");
        part.tag = "Block";

        part.transform.SetPositionAndRotation(original.position, original.rotation);
        part.transform.localScale = original.localScale;

        // Collider 변경 → BoxCollider 사용
        var box = part.AddComponent<BoxCollider>();

        var rb = part.AddComponent<Rigidbody>();
        rb.useGravity = true;

    }

    private Material GetCrossSectionMaterial(GameObject target)
    {
        return crossSectionMaterial ?? GetFirstMaterialFromTarget(target);
    }

    private Material GetFirstMaterialFromTarget(GameObject target)
    {
        Renderer r = target.GetComponentInChildren<Renderer>();
        return r != null ? r.sharedMaterial : null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class EdgeCuttingHelper : MonoBehaviour
{
    [Header("�ܸ� ���� (����θ� �ڵ����� ����)")]
    public Material crossSectionMaterial;

    [Header("���� ����Ʈ (Lower ���� �ı� �� ���)")]
    public GameObject cutEffectPrefab;

    public void Cut(GameObject target, Vector3 planePos, Vector3 planeNormal)
    {
        if (target == null) return;

        BlockSliceInfo sliceInfo = target.GetComponentInParent<BlockSliceInfo>();
        if (sliceInfo == null || sliceInfo.sliceReplacementPrefab == null)
        {
            Debug.LogWarning($"[EdgeCuttingHelper] {target.name}�� BlockSliceInfo �Ǵ� �������� �����ϴ�.");
            return;
        }

        MeshFilter meshFilter = FindMeshFilterInChildren(target);
        if (meshFilter == null)
        {
            Debug.LogWarning($"[EdgeCuttingHelper] {target.name} ���� MeshFilter�� ã�� ���߽��ϴ�.");
            return;
        }

        GameObject meshGO = meshFilter.gameObject;
        EzySlice.Plane slicingPlane = new EzySlice.Plane(planeNormal, planePos);
        Debug.DrawRay(planePos, planeNormal * 5f, Color.red, 5f);

        SlicedHull hull = meshGO.Slice(slicingPlane, GetCrossSectionMaterial(target));
        if (hull == null)
        {
            Debug.LogWarning($"[EdgeCuttingHelper] {target.name} ���� ���� (Hull �������� ����)");
            return;
        }

        Material originalMat = GetFirstMaterialFromTarget(target);
        Material crossMat = GetCrossSectionMaterial(target);

        // Upper ���� (������ ���)
        GameObject upper = Instantiate(sliceInfo.sliceReplacementPrefab, target.transform.position, target.transform.rotation);
        Mesh upperMesh = hull.CreateUpperHull(meshGO, originalMat).GetComponent<MeshFilter>().mesh;
        ReplaceMesh(upper, upperMesh, originalMat);

        // Lower�� ����
        GameObject lower = hull.CreateLowerHull(meshGO, crossMat);
        if (cutEffectPrefab != null)
            Instantiate(cutEffectPrefab, lower.transform.position, Quaternion.identity);

        Destroy(lower);
        Destroy(target);
    }

    private void ReplaceMesh(GameObject obj, Mesh newMesh, Material mat)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf != null) mf.mesh = newMesh;

        MeshCollider mc = obj.GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = newMesh;
            mc.convex = true;
        }

        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null && mat != null)
            mr.material = mat;
    }

    private Material GetFirstMaterialFromTarget(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        return renderers.Length > 0 ? renderers[0].sharedMaterial : null;
    }

    private Material GetCrossSectionMaterial(GameObject target)
    {
        return crossSectionMaterial != null ? crossSectionMaterial : GetFirstMaterialFromTarget(target);
    }
    private MeshFilter FindMeshFilterInChildren(GameObject obj)
    {
        return obj.GetComponentInChildren<MeshFilter>();
    }
}
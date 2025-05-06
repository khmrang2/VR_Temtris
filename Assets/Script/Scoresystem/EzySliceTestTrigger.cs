using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class EzySliceTestTrigger : MonoBehaviour
{
    public EdgeCuttingHelper cutter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject target = GameObject.Find("TestBlock");
            if (target == null)
            {
                Debug.LogWarning("[EzySliceTestTrigger] TestBlock�� ã�� �� �����ϴ�.");
                return;
            }

            GameObject combined = CombineChildren(target);
            if (combined == null)
            {
                Debug.LogWarning("[EzySliceTestTrigger] ���� ����");
                return;
            }

            // ���� ��� ���
            Renderer renderer = combined.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("[EzySliceTestTrigger] ���յ� ������Ʈ�� Renderer�� �����ϴ�.");
                return;
            }

            Vector3 center = renderer.bounds.center;
            Vector3 planePos = new Vector3(center.x, 0.6f, center.z); // Y�࿡�� �ڸ���
            Vector3 planeNormal = Vector3.up;

            // ���� �õ�
           // cutter.Cut(combined);
        }
    }

    GameObject CombineChildren(GameObject parent)
    {
        MeshFilter[] filters = parent.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
        {
            Debug.LogWarning("[Combine] ������ MeshFilter�� �����ϴ�.");
            return null;
        }

        CombineInstance[] combine = new CombineInstance[filters.Length];
        for (int i = 0; i < filters.Length; i++)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
        }

        GameObject combinedGO = new GameObject("CombinedTemp");
        MeshFilter mf = combinedGO.AddComponent<MeshFilter>();
        MeshRenderer mr = combinedGO.AddComponent<MeshRenderer>();

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine, true, true);
        mf.sharedMesh = newMesh;

        // ��Ƽ������ ù ��° �ڽ��� �� ����
        mr.sharedMaterial = filters[0].GetComponent<Renderer>().sharedMaterial;

        // BlockSliceInfo ����
        BlockSliceInfo sliceInfo = parent.GetComponent<BlockSliceInfo>();
        if (sliceInfo == null)
        {
            sliceInfo = parent.AddComponent<BlockSliceInfo>();
        }

        BlockSliceInfo newInfo = combinedGO.AddComponent<BlockSliceInfo>();
        newInfo.sliceReplacementPrefab = sliceInfo.sliceReplacementPrefab;

        return combinedGO;
    }
}

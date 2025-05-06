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
                Debug.LogWarning("[EzySliceTestTrigger] TestBlock을 찾을 수 없습니다.");
                return;
            }

            GameObject combined = CombineChildren(target);
            if (combined == null)
            {
                Debug.LogWarning("[EzySliceTestTrigger] 병합 실패");
                return;
            }

            // 절단 평면 계산
            Renderer renderer = combined.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("[EzySliceTestTrigger] 병합된 오브젝트에 Renderer가 없습니다.");
                return;
            }

            Vector3 center = renderer.bounds.center;
            Vector3 planePos = new Vector3(center.x, 0.6f, center.z); // Y축에서 자르기
            Vector3 planeNormal = Vector3.up;

            // 절단 시도
           // cutter.Cut(combined);
        }
    }

    GameObject CombineChildren(GameObject parent)
    {
        MeshFilter[] filters = parent.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
        {
            Debug.LogWarning("[Combine] 하위에 MeshFilter가 없습니다.");
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

        // 머티리얼은 첫 번째 자식의 걸 복사
        mr.sharedMaterial = filters[0].GetComponent<Renderer>().sharedMaterial;

        // BlockSliceInfo 연결
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

using UnityEngine;
using System.Collections.Generic;

public static class BlockConnectHelper
{
    class Face
    {
        public Vector3 center;
        public Vector3 normal;
        public BoxCollider collider;

        public Face(Vector3 center, Vector3 normal, BoxCollider collider)
        {
            this.center = center;
            this.normal = normal;
            this.collider = collider;
        }
    }

    public static void ConnectByCollision(GameObject blockA, GameObject blockB)
    {
        List<Face> facesA = GetAllFaces(blockA);
        List<Face> facesB = GetAllFaces(blockB);

        Face bestA = null, bestB = null;
        float minDistance = float.MaxValue;

        foreach (var faceA in facesA)
        {
            foreach (var faceB in facesB)
            {
                float dot = Vector3.Dot(faceA.normal, -faceB.normal);
                if (dot < 0.999f) continue; // 면이 반대 방향을 볼 때만 == 법선이 반대방향일 때만

                float dist = Vector3.Distance(faceA.center, faceB.center);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestA = faceA;
                    bestB = faceB;
                }
            }
        }

        if (bestA != null && bestB != null)
        {
            // 1. 회전 정렬 (z축 유지)
            Vector3 aEuler = blockA.transform.eulerAngles;
            Vector3 bEuler = blockB.transform.eulerAngles;
            blockB.transform.rotation = Quaternion.Euler(aEuler.x, aEuler.y, bEuler.z);

            // 2. 위치 정렬 (면 중심 기준)
            Vector3 offset = bestA.center - bestB.center;
            blockB.transform.position += offset;

            Debug.Log("블럭 연결 성공");

            // 3. FixedJoint 연결
            FixedJoint joint = blockA.AddComponent<FixedJoint>();
            joint.connectedBody = blockB.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;

            // 4. 자식으로 설정
            blockB.transform.SetParent(blockA.transform, true);
        }
        else
        {
            Debug.LogWarning("블럭 연결 실패");
        }
    }

    // Box Collider의 6개 면을 각각 가져옴
    static List<Face> GetAllFaces(GameObject block)
    {
        var faces = new List<Face>();
        var colliders = block.GetComponents<BoxCollider>();

        Vector3[] localDirs = {
            Vector3.up, Vector3.down,
            Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back
        };

        foreach (var col in colliders)
        {
            Vector3 worldCenter = col.transform.TransformPoint(col.center);
            Vector3 halfSize = Vector3.Scale(col.size * 0.5f, col.transform.lossyScale);

            foreach (var localDir in localDirs)
            {
                Vector3 worldNormal = col.transform.rotation * localDir;
                Vector3 localOffset = Vector3.Scale(localDir, halfSize);
                Vector3 worldOffset = col.transform.rotation * localOffset;
                Vector3 faceCenter = worldCenter + worldOffset;

                faces.Add(new Face(faceCenter, worldNormal, col));
            }
        }

        return faces;
    }
}

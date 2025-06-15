using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

public static class BlockConnectHelper
{
    private static float threshold = 0.9f; // face의 법선 A와 다른 face의 법선 B * -1의 곱이 1(수평일때)를 확인하기 위한 threshold
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

        Face bestA = null, bestB = null;
        float minDistance = float.MaxValue;

        Vector3 euler = blockB.transform.eulerAngles;
        SetRotation(blockA, blockB);

        List<Face> facesA = GetAllFaces(blockA);
        List<Face> facesB = GetAllFaces(blockB);

        foreach (var faceA in facesA)
        {
            foreach (var faceB in facesB)
            {
                float dot = Vector3.Dot(faceA.normal, -faceB.normal);
                if (dot < threshold) continue; // 면이 반대 방향을 볼 때만 == 법선이 거의 반대방향일 때만

                float dist = Vector3.Distance(faceA.center, faceB.center);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestA = faceA;
                    bestB = faceB;
                }
            }
        }

        // 블럭이 앞뒤로 붙는 상황인지 2차 확인
        if (IsForwardOrBackward(bestA.normal, bestA.collider.transform) || IsForwardOrBackward(bestB.normal, bestB.collider.transform))
        {
            blockB.transform.rotation = Quaternion.Euler(euler.x, euler.y, euler.z);
            bestB = null;
            Debug.Log("옳지 않은 블럭 방향");
            throw new System.Exception("아이템 '풀' 사용 중 옳지 않은 블럭 방향");
        }

        if (bestA != null && bestB != null)
        {
            // 1. 회전 정렬 (z축 유지)
            //Vector3 aEuler = blockA.transform.eulerAngles;
            //Vector3 bEuler = blockB.transform.eulerAngles;
            //blockB.transform.rotation = Quaternion.Euler(aEuler.x, aEuler.y, bEuler.z);

            // 2. 위치 정렬 (면 중심 기준)
            Vector3 offset = bestA.center - bestB.center;
            blockB.transform.position += offset;


            // 3. FixedJoint 연결
            FixedJoint joint = blockA.AddComponent<FixedJoint>();
            joint.connectedBody = blockB.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;

            // 4. 자식으로 설정
            blockB.transform.SetParent(blockA.transform, true);

            Debug.Log("블럭 연결 성공");
        }
        else
        {
            Debug.LogWarning("블럭 연결 실패, 가까운 면을 찾지 못함");
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

    // blockB의 회전을 설정, X은 blockA 기준, Y은 가장 가까운 후보, Z는 blockB 그대로
    static void SetRotation(GameObject blockA, GameObject blockB)
    {
        float aX = blockA.transform.eulerAngles.x;
        float aY = blockA.transform.eulerAngles.y;
        float bY = blockB.transform.eulerAngles.y;
        float bZ = blockB.transform.eulerAngles.z;

        // 후보 Y 회전: blockA 기준으로 같은 방향 or 180도 반대 방향
        float[] yCandidates = {
        Mathf.Repeat(aY, 360f),
        Mathf.Repeat(aY + 180f, 360f)
    };

        // 가장 가까운 Y 선택
        float bestY = yCandidates[0];
        float minDiff = Mathf.Abs(Mathf.DeltaAngle(bY, yCandidates[0]));

        for (int i = 1; i < yCandidates.Length; i++)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(bY, yCandidates[i]));
            if (diff < minDiff)
            {
                minDiff = diff;
                bestY = yCandidates[i];
            }
        }

        blockB.transform.rotation = Quaternion.Euler(aX, bestY, bZ);
    }

    // 법선이 블럭 기준은 앞뒤 방향인지 확인
    public static bool IsForwardOrBackward(Vector3 normal, Transform referenceTransform)
    {
        // 월드 법선을 로컬 기준으로 변환
        Vector3 localNormal = referenceTransform.InverseTransformDirection(normal).normalized;

        // 로컬 Z축 기준으로 앞/뒤 판단
        return Vector3.Dot(localNormal, Vector3.forward) > 0.99f || Vector3.Dot(localNormal, Vector3.back) > 0.99f;
    }
}

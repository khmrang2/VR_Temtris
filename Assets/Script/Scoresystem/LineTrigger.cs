using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    [Header("외부 연결")]
    [SerializeField] private EdgeCuttingManager _cuttingManager; // 절단 요청을 보낼 관리자 스크립트

    [Header("절단 기준 Plane")]
    [SerializeField] private Transform topPlane;   // 상단 평면 Transform
    [SerializeField] private Transform bottomPlane; // 하단 평면 Transform

    private float _triggerThreshold = 0.8f;       // 절단을 수행할 최소 점유율 기준
    private float _motionCheckDelay = 0.5f;       // 안정 상태 감지 대기 시간

    private Dictionary<GameObject, List<Collider>> blockColliders = new(); // 트리거 내 블럭과 콜라이더 연결 목록
    private float stillTime = 0f;                  // 정지 시간 누적값
    private bool isStable = false;                 // 현재 안정 상태 여부

    /// <summary>
    /// 초기화 시 절단 임계값과 정지 판정 대기 시간을 가져온다.
    /// </summary>
    private void Start()
    {
        _triggerThreshold = _cuttingManager.getThreshold();
        _motionCheckDelay = _cuttingManager.getMotionCheckDelay();
    }

    /// <summary>
    /// 콜라이더가 트리거에 진입할 때 블럭 목록에 추가한다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        GameObject block = FindTaggedParent(other.transform);
        if (block != null)
        {
            if (!blockColliders.ContainsKey(block))
                blockColliders[block] = new List<Collider>();
            blockColliders[block].Add(other);
        }
    }

    /// <summary>
    /// 콜라이더가 트리거를 벗어났을 때 목록에서 제거한다.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (GetComponent<Collider>().bounds.Intersects(other.bounds)) return;

        GameObject block = FindTaggedParent(other.transform);
        if (block != null && blockColliders.ContainsKey(block))
        {
            blockColliders[block].Remove(other);
            if (blockColliders[block].Count == 0)
                blockColliders.Remove(block);
        }
    }

    /// <summary>
    /// 주어진 Transform에서 시작하여 상위 부모 중 태그가 \"Block\"인 GameObject를 찾아 반환한다.
    /// </summary>
    /// <param name="child">현재 트랜스폼</param>
    /// <returns>Block 태그를 가진 GameObject 또는 null</returns>
    private GameObject FindTaggedParent(Transform child)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.CompareTag("Block"))
                return current.gameObject;
            current = current.parent; 
        }
        return null;
    }

    /// <summary>
    /// 매 프레임마다 블럭 정지 여부와 절단 조건을 검사한다.
    /// </summary>
    private void Update()
    {
        CleanUpNullEntries();

        // 모든 블럭이 정지 상태인지 확인
        bool allSleeping = true;
        foreach (var kvp in blockColliders.ToList())
        {
            if (kvp.Key == null)
            {
                blockColliders.Remove(kvp.Key);
                continue;
            }

            Rigidbody rb = kvp.Key.GetComponent<Rigidbody>();
            if (rb != null && !rb.IsSleeping())
            {
                allSleeping = false;
                break;
            }
        }

        if (allSleeping)
        {
            stillTime += Time.deltaTime;
            if (!isStable && stillTime >= _motionCheckDelay)
                isStable = true;
        }
        else
        {
            stillTime = 0f;
            isStable = false;
        }

        // ✅ 변경된 절단 조건 판단 (XZ 평면 기준 X 방향 점유율 계산)
        if (isStable)
        {
            float fillRatio = CalculateXLineFillRatioAtCenterPlane();
            Debug.Log($"[LineTrigger] X 평면 점유율: {fillRatio:P1}");

            if (fillRatio >= _triggerThreshold)
            {
                Debug.Log("[LineTrigger] 절단 조건 충족 → 절단 시도");
                bool success = _cuttingManager?.RequestCut(this) ?? false;
                if (success)
                {
                    Debug.Log("[LineTrigger] 절단 성공 → 트리거 초기화");
                    ResetTrigger();
                }
                else
                {
                    Debug.LogWarning("[LineTrigger] 절단 실패");
                }
            }
        }
    }

    /// <summary>
    /// 트리거의 중심 Y 위치에서 블럭들이 차지한 X 방향 면적의 비율을 계산한다.
    /// </summary>
    /// <returns>X 방향 점유율 (0~1)</returns>
    private float CalculateXLineFillRatioAtCenterPlane()
    {
        var bounds = GetComponent<BoxCollider>().bounds;
        float planeY = bounds.center.y;
        float triggerXMin = bounds.min.x;
        float triggerXMax = bounds.max.x;
        float triggerXSize = bounds.size.x;

        List<(float xMin, float xMax)> xRanges = new();

        foreach (var kvp in blockColliders)
        {
            GameObject block = kvp.Key;
            if (block == null) continue;

            Collider[] colliders = block.GetComponents<Collider>();
            foreach (var col in colliders)
            {
                if (col == null) continue;
                Bounds b = col.bounds;

                if (planeY >= b.min.y && planeY <= b.max.y)
                {
                    xRanges.Add((b.min.x, b.max.x));
                }
            }
        }

        xRanges.Sort((a, b) => a.xMin.CompareTo(b.xMin));
        float coveredX = 0f;

        if (xRanges.Count > 0)
        {
            float currentStart = xRanges[0].xMin;
            float currentEnd = xRanges[0].xMax;

            for (int i = 1; i < xRanges.Count; i++)
            {
                var (xMin, xMax) = xRanges[i];
                if (xMin > currentEnd)
                {
                    // 현재 덩어리 종료 → 누적
                    coveredX += currentEnd - currentStart;
                    currentStart = xMin;
                    currentEnd = xMax;
                }
                else
                {
                    // 병합 가능 → 범위 확장
                    currentEnd = Mathf.Max(currentEnd, xMax);
                }
            }
            // 마지막 병합 범위 반영
            coveredX += currentEnd - currentStart;
        }

        return Mathf.Clamp01(coveredX / triggerXSize);
    }

    /// <summary>
    /// 블럭이 트리거에 얼마나 포함되었는지를 AABB 기준 부피로 계산한다.
    /// </summary>
    /// <param name="block">평가할 블럭</param>
    /// <returns>포함 비율 (0~1)</returns>
    public float GetBlockFillRatio(GameObject block)
    {
        Collider triggerCol = GetComponent<Collider>();
        Collider[] allCols = block.GetComponents<Collider>();

        float totalVolume = 0f;
        float intersectVolume = 0f;

        foreach (var col in allCols)
        {
            Bounds b = col.bounds;
            float colVol = b.size.x * b.size.y * b.size.z;
            totalVolume += colVol;

            Bounds a = triggerCol.bounds;
            Vector3 min = Vector3.Max(a.min, b.min);
            Vector3 max = Vector3.Min(a.max, b.max);
            Vector3 size = max - min;
            if (size.x <= 0 || size.y <= 0 || size.z <= 0) continue;

            intersectVolume += size.x * size.y * size.z;
        }

        if (totalVolume == 0f) return 0f;
        return intersectVolume / totalVolume;
    }

    /// <summary>
    /// 트리거 내부 상태 초기화
    /// </summary>
    public void ResetTrigger()
    {
        stillTime = 0f;
        isStable = false;
        blockColliders.Clear();
    }

    /// <summary>
    /// null이 된 블럭 정보를 제거한다.
    /// </summary>
    private void CleanUpNullEntries()
    {
        var toRemove = blockColliders
            .Where(kvp => kvp.Key == null)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in toRemove)
            blockColliders.Remove(key);
    }

    /// <summary>
    /// 현재 트리거에 있는 블럭 목록 반환
    /// </summary>
    public List<GameObject> GetCurrentBlocks() => blockColliders.Keys.ToList();

    /// <summary>
    /// 상단 절단 기준 Plane의 위치
    /// </summary>
    public Vector3 GetUpperPlanePos() => topPlane.position;

    /// <summary>
    /// 상단 절단 기준 Plane의 노멀 방향
    /// </summary>
    public Vector3 GetUpperPlaneNormal() => topPlane.up;

    /// <summary>
    /// 하단 절단 기준 Plane의 위치
    /// </summary>
    public Vector3 GetLowerPlanePos() => bottomPlane.position;

    /// <summary>
    /// 하단 절단 기준 Plane의 노멀 방향
    /// </summary>
    public Vector3 GetLowerPlaneNormal() => bottomPlane.up;

    /*
    ============================
    기존 부피 기반 점유율 코드 (주석 처리)
    ============================

    private float triggerVolume;
    private float currentVolume = 0f;
    private Dictionary<Collider, float> colliderVolumes = new();

    private float EstimateVolume(Collider col)
    {
        Bounds b = col.bounds;
        return b.size.x * b.size.y * b.size.z;
    }

    private float EstimateIntersectionVolume(Collider trigger, Collider col)
    {
        Bounds a = trigger.bounds;
        Bounds b = col.bounds;

        Vector3 min = Vector3.Max(a.min, b.min);
        Vector3 max = Vector3.Min(a.max, b.max);
        Vector3 size = max - min;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return 0f;

        return size.x * size.y * size.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (colliderVolumes.ContainsKey(other)) return;

        float volume = EstimateVolume(other);
        colliderVolumes[other] = volume;
        currentVolume += volume;

        GameObject block = FindTaggedParent(other.transform);
        if (block != null)
        {
            if (!blockColliders.ContainsKey(block))
                blockColliders[block] = new List<Collider>();
            blockColliders[block].Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!colliderVolumes.ContainsKey(other)) return;

        if (GetComponent<Collider>().bounds.Intersects(other.bounds))
        {
            return;
        }

        float volume = colliderVolumes[other];
        currentVolume -= volume;
        colliderVolumes.Remove(other);

        GameObject block = FindTaggedParent(other.transform);
        if (block != null && blockColliders.ContainsKey(block))
        {
            blockColliders[block].Remove(other);
            if (blockColliders[block].Count == 0)
                blockColliders.Remove(block);
        }
    }
    */
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    [Header("외부 연결")]
    [SerializeField] private EdgeCuttingManager _cuttingManager;

    [Header("절단 기준 Plane")]
    [SerializeField] private Transform topPlane;
    [SerializeField] private Transform bottomPlane;

    private float _triggerThreshold = 0.8f; // default 0.8f;
    private float _motionCheckDelay = 0.5f; //  default 0.5f;

    private float triggerVolume;
    private float currentVolume = 0f;

    private Dictionary<Collider, float> colliderVolumes = new();
    private Dictionary<GameObject, List<Collider>> blockColliders = new();

    private float stillTime = 0f;
    private float lastFillRatio = -1f; // 변화 감지를 위한 필드 추가
    private bool isStable = false;

    private void Start()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        triggerVolume = box.size.x * box.size.y * box.size.z;
        Debug.Log($"[Init] 트리거 전체 부피: {triggerVolume:F2}");

        // 초기값 설정.
        _triggerThreshold = _cuttingManager.getThreshold();
        _motionCheckDelay = _cuttingManager.getMotionCheckDelay();
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

    private void Update()
    {
        CleanUpNullEntries();

        float fillRatio = currentVolume / triggerVolume;

        // 디버그: fillRatio 변화 감지 후 출력
        if (Mathf.Abs(fillRatio - lastFillRatio) > 0.01f)
        {
            //Debug.Log($"[LineTrigger] 현재 점유율: {fillRatio:P1} ({currentVolume:F2} / {triggerVolume:F2})");
            lastFillRatio = fillRatio;
        }

        bool allSleeping = true;
        foreach (var kvp in blockColliders.ToList()) // 리스트 복사 → 수정 중 안전
        {
            if (kvp.Key == null)
            {
                blockColliders.Remove(kvp.Key); // Clean up
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

        if (isStable && fillRatio >= _triggerThreshold)
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
        else
        {
            Debug.Log($"[LineTrigger] 현재 점유율: {fillRatio:P1} ({currentVolume:F2} / {triggerVolume:F2})");
        }
    }

    public float GetBlockFillRatio(GameObject block)
    {
        if (!blockColliders.ContainsKey(block)) return 0f;

        float totalVolume = 0f;
        float intersectVolume = 0f;

        Collider triggerCol = GetComponent<Collider>(); // 트리거 자신

        // 블럭의 모든 collider
        Collider[] allCols = block.GetComponents<Collider>();

        foreach (var col in allCols)
        {
            float colVol = EstimateVolume(col); // 원래 부피
            totalVolume += colVol;

            // 트리거와의 실제 겹치는 부피만 측정
            intersectVolume += EstimateIntersectionVolume(triggerCol, col);
        }

        if (totalVolume == 0f) return 0f;

        return intersectVolume / totalVolume;
    }
    public void ResetTrigger()
    {
        currentVolume = 0f;
        stillTime = 0f;
        isStable = false;
        colliderVolumes.Clear();
        blockColliders.Clear();
    }

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

    public List<GameObject> GetCurrentBlocks() => blockColliders.Keys.ToList();

    private void CleanUpNullEntries()
    {
        var toRemove = colliderVolumes
            .Where(kvp => kvp.Key == null || kvp.Key.Equals(null) || kvp.Key.attachedRigidbody == null)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var col in toRemove)
        {
            if (col == null) continue;

            if (colliderVolumes.ContainsKey(col))
                currentVolume -= colliderVolumes[col];

            colliderVolumes.Remove(col);

            if (col != null && col.transform != null)
            {
                GameObject block = FindTaggedParent(col.transform);
                if (block != null && blockColliders.ContainsKey(block))
                {
                    blockColliders[block].Remove(col);
                    if (blockColliders[block].Count == 0)
                        blockColliders.Remove(block);
                }
            }
        }
    }

    // Plane 위치 제공
    public Vector3 GetUpperPlanePos() => topPlane.position;
    public Vector3 GetUpperPlaneNormal() => topPlane.up;

    public Vector3 GetLowerPlanePos() => bottomPlane.position;
    public Vector3 GetLowerPlaneNormal() => bottomPlane.up;

}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    [Header("����")]
    public float triggerThreshold = 0.8f;
    public float motionCheckDelay = 0.5f;

    [Header("�ܺ� ����")]
    public EdgeCuttingManager cuttingManager;

    [Header("���� ���� Plane")]
    public Transform topPlane;
    public Transform bottomPlane;

    private float triggerVolume;
    private float currentVolume = 0f;

    private Dictionary<Collider, float> colliderVolumes = new();
    private Dictionary<GameObject, List<Collider>> blockColliders = new();

    private float stillTime = 0f;
    private float lastFillRatio = -1f; // ��ȭ ������ ���� �ʵ� �߰�
    private bool isStable = false;

    private void Start()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        triggerVolume = box.size.x * box.size.y * box.size.z;
        Debug.Log($"[Init] Ʈ���� ��ü ����: {triggerVolume:F2}");
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

        // �����: fillRatio ��ȭ ���� �� ���
        if (Mathf.Abs(fillRatio - lastFillRatio) > 0.01f)
        {
            //Debug.Log($"[LineTrigger] ���� ������: {fillRatio:P1} ({currentVolume:F2} / {triggerVolume:F2})");
            lastFillRatio = fillRatio;
        }

        bool allSleeping = true;
        foreach (var kvp in blockColliders)
        {
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
            if (!isStable && stillTime >= motionCheckDelay)
                isStable = true;
        }
        else
        {
            stillTime = 0f;
            isStable = false;
        }

        if (isStable && fillRatio >= triggerThreshold)
        {
            Debug.Log("[LineTrigger] ���� ���� ���� �� ���� �õ�");
            bool success = cuttingManager?.RequestCut(this) ?? false;
            if (success)
            {
                Debug.Log("[LineTrigger] ���� ���� �� Ʈ���� �ʱ�ȭ");
                ResetTrigger();
            }
            else
            {
                Debug.LogWarning("[LineTrigger] ���� ����");
            }
        }
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
            .Where(kvp => kvp.Key == null || kvp.Key.attachedRigidbody == null)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var col in toRemove)
        {
            currentVolume -= colliderVolumes[col];
            colliderVolumes.Remove(col);

            GameObject block = FindTaggedParent(col.transform);
            if (block != null && blockColliders.ContainsKey(block))
            {
                blockColliders[block].Remove(col);
                if (blockColliders[block].Count == 0)
                    blockColliders.Remove(block);
            }
        }
    }

    // Plane ��ġ ����
    public Vector3 GetUpperPlanePos() => topPlane.position;
    public Vector3 GetUpperPlaneNormal() => topPlane.up;

    public Vector3 GetLowerPlanePos() => bottomPlane.position;
    public Vector3 GetLowerPlaneNormal() => bottomPlane.up;

}
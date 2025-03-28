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

    private float triggerVolume;
    private float currentVolume = 0f;

    // Collider�� ���� ���� ����
    private Dictionary<Collider, float> colliderVolumes = new();

    // �׷����� ���� Block �� �ش� ���� ���� Collider ���
    private Dictionary<GameObject, List<Collider>> blockColliders = new();

    private float stillTime = 0f;
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

        Debug.Log($"[LineTrigger:{name}] ���: {other.name} �� ����: {volume:F2} | ������: {currentVolume:F2}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!colliderVolumes.ContainsKey(other)) return;

        // ������ Ʈ���� �ȿ� �ִ��� �� �� �� Ȯ��
        if (GetComponent<Collider>().bounds.Intersects(other.bounds))
        {
            // ���� �� �����ٰ� �ǴܵǸ� return
            Debug.LogWarning($"[LineTrigger:{name}] {other.name} �� ������ Ʈ���� �ȿ� ���� (Exit ����)");
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

        Debug.Log($"[LineTrigger:{name}] ����: {other.name} | ���� ����: {volume:F2} | ������: {currentVolume:F2}");
    }

    private void Update()
    {
        CleanUpNullEntries();

        float fillRatio = currentVolume / triggerVolume;
       // Debug.Log($"[LineTrigger:{name}] ���� ������: {fillRatio:F2}");

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
            bool success = cuttingManager?.RequestCut(this) ?? false;
            if (success) ResetTrigger();
        }
    }

    private void ResetTrigger()
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

    public List<GameObject> GetCurrentBlocks()
    {
        return blockColliders.Keys.ToList();
    }

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
}
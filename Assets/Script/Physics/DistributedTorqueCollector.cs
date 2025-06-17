using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DistributedTorqueCollector : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 previousTorque = Vector3.zero;
    private Vector3 lastAppliedTorque = Vector3.zero;

    private bool isGrounded = false;
    private int stillFrameCounter = 0;

    [Header("Torque ����")]
    public float torqueLevel = 20f;
    public float torqueUnit = 5000f;
    public float torqueThreshold = 0.01f;
    public float alignmentThreshold = 0.95f;
    public float torqueLerpFactor = 0.1f;
    public float maxRotationSpeed = 15f;

    [Header("���� ����")]
    public float baseAngularDrag = 0.01f;

    [Header("Sleep ����")]
    public int requiredStillFrames = 10;

    [Header("����� �ɼ�")]
    public bool drawTorqueGizmo = true;
    public float torqueGizmoScale = 0.5f;

    private float TorqueMultiplier => torqueLevel * torqueUnit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //���� ���� ����
        rb.useGravity = false;                // �߷� ���� ���� ��� �ڵ� ����
        rb.angularDrag = baseAngularDrag * torqueLevel;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        float v = rb.velocity.magnitude;
        float av = rb.angularVelocity.magnitude;

        // 1. Sleep ���� �˻�: ���� ���� + ���� ������ ����
        if (isGrounded && v < 0.01f && av < 0.01f)
        {
            stillFrameCounter++;
            if (stillFrameCounter >= requiredStillFrames)
            {
               // Debug.Log($"[SLEEP] {gameObject.name} �� ���� ����. Sleep()");
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
                return;
            }
        }
        else
        {
            stillFrameCounter = 0;
        }

        // 2. �߷� ���� ���� ���� �� useGravity = true�� ��ü
        rb.AddForce(Physics.gravity * rb.mass);

        // 3. �ڽ� ��� ��ũ ���
        Vector3 totalTorque = Vector3.zero;
        Vector3 com = rb.worldCenterOfMass;

        int childCount = transform.childCount;
        float massPerChild = rb.mass / Mathf.Max(1, childCount - 1);

        foreach (Transform child in transform)
        {
            var emitter = child.GetComponent<ChildGravityTorqueEmitter>();
            if (emitter != null)
                totalTorque += emitter.ComputeTorque(com, massPerChild);
        }

        Vector3 rawTorque = totalTorque * TorqueMultiplier;

        //  4. �̼� ��ũ ����
        if (rawTorque.sqrMagnitude < 1e-6f)
            return;

        Vector3 torqueDir = rawTorque.normalized;
        Vector3 angularDir = rb.angularVelocity.normalized;
        float alignment = Vector3.Dot(torqueDir, angularDir);
        float currentSpeed = rb.angularVelocity.magnitude;

        Vector3 smoothTorque = Vector3.Lerp(previousTorque, rawTorque, torqueLerpFactor);
        previousTorque = smoothTorque;

        //  5. ���� ���� �� ��ũ ����
        if (!(alignment > alignmentThreshold && rawTorque.magnitude < torqueThreshold) &&
            currentSpeed < maxRotationSpeed)
        {
            rb.angularDrag = baseAngularDrag * torqueLevel + 0.2f;  // ���� ��ȭ
            rb.AddTorque(smoothTorque);
        }

        lastAppliedTorque = smoothTorque;
    }

    //  ���� ����
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
       // Debug.Log($"[Ground] OnCollisionStay �����: {collision.gameObject.name}");
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    //  ��ũ �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !drawTorqueGizmo) return;

        Gizmos.color = Color.cyan;
        Vector3 start = rb.worldCenterOfMass;
        Vector3 end = start + lastAppliedTorque * torqueGizmoScale;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
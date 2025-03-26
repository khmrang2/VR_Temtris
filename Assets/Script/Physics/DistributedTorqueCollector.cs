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

    [Header("Torque ����")]
    [Tooltip("��ũ ���� ���� (�� ��ũ ����)")]
    public float torqueLevel = 20f;

    [Tooltip("���� ���� ��� ���� (���� 5000)")]
    public float torqueUnit = 5000f;

    [Tooltip("ȸ�� ���� �ּ� �Ӱ谪")]
    public float torqueThreshold = 0.01f;

    [Tooltip("ȸ�� ���� ��ġ ���ġ (0~1), �������� ��ġ �� ��ũ ����")]
    public float alignmentThreshold = 0.95f;

    [Tooltip("�ε巯�� ȸ�� ���� ��� (0~1), 0�̸� ���, 1�̸� ������")]
    public float torqueLerpFactor = 0.1f;

    [Tooltip("�ִ� ��� ȸ�� �ӵ� (�� �̻��̸� ��ũ ����)")]
    public float maxRotationSpeed = 15f;

    [Header("���� ����")]
    [Tooltip("�⺻ angularDrag �� (torqueLevel=1 ����)")]
    public float baseAngularDrag = 0.01f;

    [Header("����� �ɼ�")]
    public bool drawTorqueGizmo = true;
    public float torqueGizmoScale = 0.5f;

    private float TorqueMultiplier => torqueLevel * torqueUnit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularDrag = baseAngularDrag * torqueLevel;
    }

    void FixedUpdate()
    {
        float v = rb.velocity.magnitude;
        float av = rb.angularVelocity.magnitude;

        //  ���� �����̰�, �ӵ��� ����� ������ Sleep ó��
        if (isGrounded && v < 0.01f && av < 0.01f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            return;
        }

        //  �߷��� �׻� ����
        rb.AddForce(Physics.gravity * rb.mass);

        //  ��ũ ���
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
        Vector3 torqueDir = rawTorque.normalized;
        Vector3 angularDir = rb.angularVelocity.normalized;
        float alignment = Vector3.Dot(torqueDir, angularDir);
        float currentSpeed = rb.angularVelocity.magnitude;

        Vector3 smoothTorque = Vector3.Lerp(previousTorque, rawTorque, torqueLerpFactor);
        previousTorque = smoothTorque;

        if (!(alignment > alignmentThreshold &&
              rawTorque.magnitude < torqueThreshold) &&
              currentSpeed < maxRotationSpeed)
        {
            rb.AddTorque(smoothTorque);
        }

        lastAppliedTorque = smoothTorque;

#if UNITY_EDITOR
        Debug.Log($"[Torque] {gameObject.name} �� Mult: {TorqueMultiplier}, Applied: {smoothTorque:F3}, Align: {alignment:F2}, Speed: {currentSpeed:F2}, Grounded: {isGrounded}");
#endif
    }

    //  ���� ���� ó��
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    //  �ð�ȭ
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
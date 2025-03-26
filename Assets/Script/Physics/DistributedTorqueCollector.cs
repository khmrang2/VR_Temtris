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

    [Header("Torque 설정")]
    [Tooltip("토크 강도 레벨 (× 토크 단위)")]
    public float torqueLevel = 20f;

    [Tooltip("실제 물리 배수 단위 (보통 5000)")]
    public float torqueUnit = 5000f;

    [Tooltip("회전 적용 최소 임계값")]
    public float torqueThreshold = 0.01f;

    [Tooltip("회전 방향 일치 허용치 (0~1), 높을수록 일치 시 토크 생략")]
    public float alignmentThreshold = 0.95f;

    [Tooltip("부드러운 회전 감쇠 계수 (0~1), 0이면 즉시, 1이면 느리게")]
    public float torqueLerpFactor = 0.1f;

    [Tooltip("최대 허용 회전 속도 (이 이상이면 토크 생략)")]
    public float maxRotationSpeed = 15f;

    [Header("감쇠 설정")]
    [Tooltip("기본 angularDrag 값 (torqueLevel=1 기준)")]
    public float baseAngularDrag = 0.01f;

    [Header("디버그 옵션")]
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

        //  접지 상태이고, 속도가 충분히 느리면 Sleep 처리
        if (isGrounded && v < 0.01f && av < 0.01f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            return;
        }

        //  중력은 항상 적용
        rb.AddForce(Physics.gravity * rb.mass);

        //  토크 계산
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
        Debug.Log($"[Torque] {gameObject.name} → Mult: {TorqueMultiplier}, Applied: {smoothTorque:F3}, Align: {alignment:F2}, Speed: {currentSpeed:F2}, Grounded: {isGrounded}");
#endif
    }

    //  접지 감지 처리
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    //  시각화
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
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

    [Header("Torque 설정")]
    public float torqueLevel = 20f;
    public float torqueUnit = 5000f;
    public float torqueThreshold = 0.01f;
    public float alignmentThreshold = 0.95f;
    public float torqueLerpFactor = 0.1f;
    public float maxRotationSpeed = 15f;

    [Header("감쇠 설정")]
    public float baseAngularDrag = 0.01f;

    [Header("Sleep 설정")]
    public int requiredStillFrames = 10;

    [Header("디버그 옵션")]
    public bool drawTorqueGizmo = true;
    public float torqueGizmoScale = 0.5f;

    private float TorqueMultiplier => torqueLevel * torqueUnit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //물리 설정 권장
        rb.useGravity = false;                // 중력 직접 적용 대신 자동 적용
        rb.angularDrag = baseAngularDrag * torqueLevel;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        float v = rb.velocity.magnitude;
        float av = rb.angularVelocity.magnitude;

        // 1. Sleep 조건 검사: 접지 상태 + 정지 프레임 누적
        if (isGrounded && v < 0.01f && av < 0.01f)
        {
            stillFrameCounter++;
            if (stillFrameCounter >= requiredStillFrames)
            {
               // Debug.Log($"[SLEEP] {gameObject.name} → 조건 충족. Sleep()");
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

        // 2. 중력 직접 적용 제거 → useGravity = true로 대체
        rb.AddForce(Physics.gravity * rb.mass);

        // 3. 자식 기반 토크 계산
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

        //  4. 미세 토크 제거
        if (rawTorque.sqrMagnitude < 1e-6f)
            return;

        Vector3 torqueDir = rawTorque.normalized;
        Vector3 angularDir = rb.angularVelocity.normalized;
        float alignment = Vector3.Dot(torqueDir, angularDir);
        float currentSpeed = rb.angularVelocity.magnitude;

        Vector3 smoothTorque = Vector3.Lerp(previousTorque, rawTorque, torqueLerpFactor);
        previousTorque = smoothTorque;

        //  5. 조건 만족 시 토크 적용
        if (!(alignment > alignmentThreshold && rawTorque.magnitude < torqueThreshold) &&
            currentSpeed < maxRotationSpeed)
        {
            rb.angularDrag = baseAngularDrag * torqueLevel + 0.2f;  // 감쇠 강화
            rb.AddTorque(smoothTorque);
        }

        lastAppliedTorque = smoothTorque;
    }

    //  접지 감지
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
       // Debug.Log($"[Ground] OnCollisionStay 실행됨: {collision.gameObject.name}");
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    //  토크 시각화
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
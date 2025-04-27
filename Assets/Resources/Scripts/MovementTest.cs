using UnityEngine;
using UnityEngine.InputSystem;
using Oculus.Interaction.Input;

public class VRPlayerMovement : MonoBehaviour
{
    [Header("필수 설정")]
    public Transform rigRoot; // 실제 움직일 root 오브젝트.
    public InputActionProperty moveInput; // 움직임을 담당할 컨트롤러.
    public Transform forwardSource; // 중간 카메라.

    [Header("옵션")]
    public float moveSpeed = 1.5f; // 이동 속도
    public bool enableStrafe = true;

    void Update()
    {
        if (moveInput == null || rigRoot == null) return;
        // 1. 컨트롤러의 입력값을 받음.
        Vector2 input = Readinput();
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 move = ComputeDesiredMove(input);
        rigRoot.position += move;
    }

    private Vector2 Readinput(){
        var leftjoystickReadValue = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.LTouch);
        return leftjoystickReadValue;
    }

    /// <summary>
    /// <seealso cref="Unity XR Interaction Toolkit / ContinuousMoveProvider"/>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private Vector3 ComputeDesiredMove(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector3.zero;

        // 입력값 정규화.
        var inputMove = Vector3.ClampMagnitude(
            new Vector3(enableStrafe ? input.x : 0f, 0f, input.y), 1f);

        // 기준 방향(카메라) 설정.
        var forwardSourceTransform = forwardSource == null ? 
            Camera.main.transform : forwardSource;
        var inputForwardInWorldSpace = forwardSourceTransform.forward;

        // 속도 계산.
        var speedFactor = moveSpeed * Time.deltaTime * rigRoot.localScale.x;

        // 일반 이동.
        var originUp = rigRoot.up;
        if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, originUp)), 1f))
        {
            inputForwardInWorldSpace = -forwardSourceTransform.up;
        }

        // 카메라 front 벡터.
        var inputForwardProjectedInWorldSpace = 
            Vector3.ProjectOnPlane(inputForwardInWorldSpace, originUp);

        // 카메라(유저가 보는 방향)과 rigRoot(현재 오브젝트가 보는 방향)의 회전 차이를 계산.
        var forwardRotation = Quaternion.FromToRotation(
            rigRoot.forward, inputForwardProjectedInWorldSpace);

        // 카메라 방향으로 이동.
        var translationInRigSpace = forwardRotation * inputMove * speedFactor;
        var translationInWorldSpace = 
            rigRoot.TransformDirection(translationInRigSpace);

        return translationInWorldSpace;
    }
}
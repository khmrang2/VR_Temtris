using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandUIManager : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform leftController;
    public GameObject uiWindowPrefab;

    [Header("Button Settings")]
    public InputActionProperty showButton;  // X 버튼 바인딩

    [Header("Instance Settings")]
    public Vector3 localPosition = new Vector3(0f, 0.1f, 0.2f);
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one * 0.001f;

    private GameObject _uiInstance;
    private bool _isOpen = false;

/*    
    private void FixedUpdate()
    {
        _uiInstance.transform.LookAt(head.position, Vector3.up);
        _uiInstance.transform.Rotate(0f, 180f, 0f);
    }*/

    void OnEnable()
    {
        // 액션 활성화
        showButton.action.Enable();
        // 버튼 눌림 이벤트 구독
        showButton.action.performed += OnShowButtonPressed;
    }

    void OnDisable()
    {
        // 이벤트 해제
        showButton.action.performed -= OnShowButtonPressed;
        showButton.action.Disable();
    }

    private void OnShowButtonPressed(InputAction.CallbackContext ctx)
    {
        _isOpen = !_isOpen;
        ToggleUI();
        Debug.Log($"[HandUIManager] {(_isOpen ? "왼손 보드 ON" : "왼손 보드 OFF")}");
    }

    void ToggleUI()
    {
        if (_uiInstance == null)
        {
            _uiInstance = Instantiate(uiWindowPrefab, leftController);
            _uiInstance.transform.localPosition = localPosition;
            _uiInstance.transform.localEulerAngles = localEuler;
            _uiInstance.transform.localScale = localScale;
        }
        else
        {
            _uiInstance.SetActive(_isOpen);
        }
        
    }
}

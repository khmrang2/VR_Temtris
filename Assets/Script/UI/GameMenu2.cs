using UnityEngine;
using UnityEngine.InputSystem;

public class temp2 : MonoBehaviour
{
    public GameObject ovrRig;
    public GameObject xrRig;
    private bool _toggle = false;

    void SwitchToOVR()
    {
        xrRig.SetActive(false);
        ovrRig.SetActive(true);
    }

    void SwitchToXR()
    {
        ovrRig.SetActive(false);
        xrRig.SetActive(true);
    }

    public InputActionProperty showButton;  // 메뉴버튼

    bool isOpen;


    void Update()
    {
        /* ① 입력 감지 */
        if (showButton.action.WasPressedThisFrame())
        {
            if (_toggle)
            {
                SwitchToXR();
            }
            else
            {
                SwitchToOVR();
            }
            _toggle = !_toggle;
        }
    }
}

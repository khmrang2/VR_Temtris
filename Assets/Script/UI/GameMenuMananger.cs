using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameMenuManager : MonoBehaviour
{
    public Transform head;
    public float spawnDistance = 1;
    public GameObject menu;
    public InputActionProperty showButton;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (showButton.action.WasPressedThisFrame())
        {
            menu.SetActive(!menu.activeSelf);

            if (menu.activeSelf)
            {
                Vector3 spawnDir = head.forward.normalized;
                menu.transform.position = head.position + spawnDir * spawnDistance;
            }
        }

        if (menu.activeSelf)
        {
            menu.transform.LookAt(head.position, Vector3.up);
            menu.transform.Rotate(0f, 180f, 0f);
        }
    }
}

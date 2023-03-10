using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceInput : MonoBehaviour
{
    public static DeviceInput a;
    //PlayerControls inputActions;
    public Vector2 LookInput; // { get => inputActions.Player.Look.ReadValue<Vector2>(); }
    public Camera cam;

    private void Awake()
    {
        if (!a)
        { 
            a = this;
            //inputActions = new PlayerControls();
        }
        else
        {
            Destroy(this);
        }    
    }
    public void SetLookInput(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }
}

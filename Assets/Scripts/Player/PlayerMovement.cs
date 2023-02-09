using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("camera")]
    [SerializeField] Camera FPCamera;
    [SerializeField] Transform[] camPositions;
    [SerializeField] float camVSpeed;
    //[SerializeField] [Range(-1, 1)] 
    float camT;
    Vector2 lookInput { get => inputs.Player.Look.ReadValue<Vector2>(); }

    [Header("movement")]
    [SerializeField] CharacterController cc;
    [SerializeField] float walkSpeed;
    [SerializeField] float turnSpeed;
    Vector2 moveInput { get => inputs.Player.Move.ReadValue<Vector2>(); }

    [Header("jump")]
    [SerializeField] float jumpVelocity;
    [SerializeField] float standardGravity;
    [SerializeField] float fallMultiplier;
    float VerticalVelocity = 0;
    [SerializeField] float CoyoteTime;
    float longGrounded;
    bool isGrounded { get => longGrounded > 0; }

    [Header("inventory")]
    //inventory

    public static PlayerMovement active;
    PlayerControls inputs;

    private void Awake()
    {
        active = this;

        inputs = new PlayerControls();
        inputs.Player.Jump.performed += ctx => Jump();

        inputs.Player.Interact.performed += ctx => PlayerGrab.active.GrabOrRelease();
        inputs.Player.Back.performed += ctx => PlayerGrab.active.Release();
        inputs.Player.Inventory.performed += ctx => PlayerGrab.active.ToInventory();
    }
    private void OnEnable()
    {
        inputs.Player.Enable();
    }
    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    void Start()
    {
        if (!cc)
            cc = GetComponent<CharacterController>();
        camT = 0;
    }

    void Update()
    {
        camT += lookInput.y * camVSpeed * Time.deltaTime;
        camT = Mathf.Clamp(camT, -1, 1);
        switch (camT < Mathf.Epsilon -1)
        {
            case true:
                //inventory
                break;
            case false:
                MoveCameraV();
                Move();
                CalculateGrounded();
                break;
        }
    }

    void MoveCameraV()
    {
        switch (camT < 0)
        {
            case false:
                FPCamera.transform.position = Vector3.Lerp(camPositions[1].position, camPositions[2].position, camT);
                FPCamera.transform.rotation = Quaternion.Slerp(camPositions[1].rotation, camPositions[2].rotation, camT);
                break;
            case true:
                float t = 1 + camT;
                FPCamera.transform.position = Vector3.Lerp(camPositions[0].position, camPositions[1].position, t);
                FPCamera.transform.rotation = Quaternion.Slerp(camPositions[0].rotation, camPositions[1].rotation, t);
                break;
        }
    }
    void Move()
    {
        float rotate = lookInput.x * turnSpeed * Time.deltaTime;
        transform.Rotate(0, rotate, 0);

        Vector3 moveLocal = new Vector3(moveInput.x, 0, moveInput.y) * (walkSpeed * Time.deltaTime);
        switch (VerticalVelocity < 0)
        {
            case false: //going up
                VerticalVelocity += standardGravity * Time.deltaTime;
                break;
            case true: //going down
                VerticalVelocity += standardGravity * Time.deltaTime * fallMultiplier;
                break;
        }
        moveLocal.y = VerticalVelocity;
        Vector3 move = transform.TransformDirection(moveLocal);
        cc.Move(move);
        if (cc.isGrounded)
            VerticalVelocity = standardGravity;
        /*
        if (IsGrounded(out float distance))
        {
            VerticalVelocity = Mathf.Max(VerticalVelocity, distance);
            Debug.Log("velocity is " + VerticalVelocity + ", grounded");
        }
        else
            Debug.Log("velocity is " + VerticalVelocity + ", airborne");
        */
    }
    
    void Jump()
    {
        if (isGrounded)
        {
            VerticalVelocity = jumpVelocity;
        }
    }
    void CalculateGrounded()
    {
        switch (cc.isGrounded)
        {
            case true:
                longGrounded = CoyoteTime;
                break;
            case false:
                longGrounded -= Time.deltaTime;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (!cc)
            cc = GetComponent<CharacterController>();
        Gizmos.DrawWireCube(transform.position + cc.center, new Vector3(cc.radius * 2, cc.height, cc.radius * 2));

        Gizmos.color = Color.cyan;
        if (!FPCamera || camPositions.Length <= 0)
            return;
        for (int i = 0; i < camPositions.Length; i++)
        {
            Gizmos.DrawLine(camPositions[i].position, camPositions[i].position + camPositions[i].forward);
            Gizmos.DrawSphere(camPositions[i].position, 0.05f);
        }
    }
}

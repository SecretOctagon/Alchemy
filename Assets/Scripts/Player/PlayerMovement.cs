using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("camera")]
    [SerializeField] public Camera FPCamera;
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

    public static PlayerMovement active;
    PlayerControls inputs;

    private void Awake()
    {
        active = this;

        inputs = new PlayerControls();
        inputs.Player.Jump.started += ctx => Jump();

        inputs.Player.Grab.performed += ctx => PlayerGrab.active.GrabOrRelease();
        inputs.Player.Inventory.performed += ctx => PlayerGrab.active.ToInventory();
        inputs.Player.Use.performed += ctx => PlayerGrab.active.UseItem();
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
            case false: //upper half
                FPCamera.transform.position = Vector3.Lerp(camPositions[1].position, camPositions[2].position, camT);
                FPCamera.transform.rotation = Quaternion.Slerp(camPositions[1].rotation, camPositions[2].rotation, camT);
                break;
            case true: //lower half
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
    void Jump()
    {
        if (isGrounded)
        {
            VerticalVelocity = jumpVelocity;
        }
    }
    
    public void InverseCameraT()
    {
        float tDownX = Mathf.InverseLerp(camPositions[0].rotation.x, camPositions[1].rotation.x, FPCamera.transform.rotation.x);
        float tDownY = Mathf.InverseLerp(camPositions[0].rotation.y, camPositions[1].rotation.y, FPCamera.transform.rotation.y);
        float tDownZ = Mathf.InverseLerp(camPositions[0].rotation.z, camPositions[1].rotation.z, FPCamera.transform.rotation.z);
        float tDownW = Mathf.InverseLerp(camPositions[0].rotation.w, camPositions[1].rotation.w, FPCamera.transform.rotation.w);
        float medTDown = (tDownX + tDownY + tDownZ + tDownW) / 4;
        Quaternion invDown = Quaternion.Lerp(camPositions[0].rotation, camPositions[1].rotation, medTDown);
        camT = medTDown - 1;
        /*
        float angleDown = Quaternion.Angle(invDown, FPCamera.transform.rotation);
                
        float tUpX = Mathf.InverseLerp(camPositions[1].rotation.x, camPositions[2].rotation.x, FPCamera.transform.rotation.x);
        float tUpY = Mathf.InverseLerp(camPositions[1].rotation.y, camPositions[2].rotation.y, FPCamera.transform.rotation.y);
        float tUpZ = Mathf.InverseLerp(camPositions[1].rotation.z, camPositions[2].rotation.z, FPCamera.transform.rotation.z);
        float tUpW = Mathf.InverseLerp(camPositions[1].rotation.w, camPositions[2].rotation.w, FPCamera.transform.rotation.w);
        float medTUp = (tUpX + tUpY + tUpZ + tUpW) / 4;
        Quaternion invUp = Quaternion.Lerp(camPositions[1].rotation, camPositions[2].rotation, medTDown);
        float angleUp = Quaternion.Angle(invDown, FPCamera.transform.rotation);
        
        Debug.Log("down reconstruction angle is " + angleDown + ", up is " + angleUp);
        switch (angleDown < angleUp)
        {
            case true: //down is closer to original
                camT = medTDown - 1;
                break;
            case false: //up is closer to original
                camT = medTUp;
                break;
        }
        */
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

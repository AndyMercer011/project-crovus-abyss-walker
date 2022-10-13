using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using RootMotion.Demos;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMoveController : MonoBehaviour
{

    private InputMaster inputMaster;
    private CharacterController characterController;

    private PlayerAnimatorController animatorController;

public float moveSpeed = 10;
#if UNITY_EDITOR
    private void OnValidate()
    {
        characterController = GetComponent<CharacterController>();
    }
#endif
    
    void Start(){
        animatorController = new PlayerAnimatorController(GetComponent<Animator>());
    }
    
    private void Awake()
    {
        inputMaster = new InputMaster();
        inputMaster.Enable();
    }

    private void Update()
    {
        Vector2 vector2d = inputMaster.Player.Move.ReadValue<Vector2>();
        if (vector2d != Vector2.zero)
        {
            animatorController.SetLowerBodyState(PlayerAnimatorController.LowerBodyState.WALK);
            Vector3 vector3d = new Vector3(vector2d.x, 0, vector2d.y);
            Vector3 direction = transform.TransformDirection(vector3d);
            characterController.Move(direction * moveSpeed * Time.deltaTime);
        } else
        {
            animatorController.SetLowerBodyState(PlayerAnimatorController.LowerBodyState.STAND);
        }
    }


}

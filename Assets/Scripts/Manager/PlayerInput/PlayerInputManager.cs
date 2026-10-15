using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager:Singleton<PlayerInputManager>
{
    private PlayerInput inputActions;

    public Vector2 _run => inputActions.Input.Run.ReadValue<Vector2>();
    public bool _walk =>inputActions.Input.Walk.triggered;
    public bool _attack =>inputActions.Input.Attack.triggered;
    public bool _jump =>inputActions.Input.Jump.triggered;
    public bool _sprint =>inputActions.Input.Sprint.triggered;

    public bool parry =>inputActions.Input.Parry.triggered;

    public bool rightAttack =>inputActions.Input.RightCombo.triggered;


    public Vector2 _look =>inputActions.Input.Look.ReadValue<Vector2>();

    public Vector2 _scoll =>inputActions.Input.Scoll.ReadValue<Vector2>();



    protected override void Awake()
    {
        base.Awake();
        inputActions ??= new PlayerInput();
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
        Cursor.visible = false;                   // 隐藏鼠标指针
    }

    private void OnEnable()
    {
        inputActions.Enable();       //启用场景中所有的actionmap
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

}


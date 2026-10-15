using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum ExecuteState
{
    Idle,
    Move
}

public class ButtonTrigger : BaseTrigger
{
    
    private ExecuteState executeState = ExecuteState.Idle;
    private Animator animator;

    void SwithPlayerState()
    {
        switch (executeState)
        {
            case ExecuteState.Idle:
                animator.Play("Ani_Avatar_Girl_Standby");
                break;
            case ExecuteState.Move:
                animator.Play("Movement");
                break;

        }
       

    }

    

    protected void Start()
    {

        animator = GetComponent<Animator>();
        RegisterSaveTypeEvent();
    }
    protected void OnDestroy()
    {
        

        DeleteSaveTypeEvent();
    }

    public override void RegisterSaveTypeEvent()
    {
        
    }

    public override void DeleteSaveTypeEvent()
    {
        
    }
}
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

public class EnemyCombatController : PlayerComboControllerBase
{
    //1.在离我1.5m之外，就尝试走近
    //2.进入1.5m之后，开始攻击
    //3.攻击完成后又进入自由移动
    [SerializeField]private bool attackCommand;

    

    public bool getAttackCommand()=>attackCommand;


    private void Start()
    {
        attackCommand = false;
        canAttack =true;
    }
    private void ResetAttackCommand()
    {
        attackCommand = false;
    }

    public void AIBaseAttackInput()
    {
        if (!attackCommand) return;
        if(!canAttack)return;
        ChangeAttackCombo(baseCombo);
        ExecuteAction();
    }
    private bool CanAttackCommand()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Parry")) return false;
        if(animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("FinishHit")) return false;
        if(attackCommand) return false;
        return true;    
    }

    public void SetAttackCommand()
    {
        if (!CanAttackCommand())
        {
            ResetAttackCommand();   //如果不允许攻击，那么就重置是否允许攻击
            return;
        }
        attackCommand = true;
    }


    //等待冷却时间
    protected override void UpdateColdTime()
    {
        currentComboIndex++;
        ResetAttackCommand();
        maxColdTime = 0f;
        canAttack = true;
    }

    
    public void StopEnemyCombat()
    {
        if(attackCommand)ResetAttackCommand();
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            animator.Play("Idle",0,0f);
        }
    }
    
}

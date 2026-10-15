using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboController : PlayerComboControllerBase
{
    //1.首先我们要传入组合技，普通组合技，重击组合技
    //2，我们当前的组合技

    [SerializeField, Header("重击组合技")] private CharacterComboSO heavyCombo;



    protected override void Awake()
    {
        base.Awake();
        if (Camera.main != null) { cameraGameObject = Camera.main.transform; }


    }
    private void Start()
    {
        canAttack = true;


    }


    protected override void Update()
    {
        base.Update();
        //CharacterBaseAttackInput();
        //UpdateDetectDirection(); 
        MatchPosition();//matchtarget放在播放处决动画之前
    }


    //在触发处决时，偏移量


    protected override void MatchPosition()
    {
        base.MatchPosition();
        if (!animator) return;
        if (currentEnemy == null) return;
        if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1f) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish"))
        {
            transform.rotation = Quaternion.LookRotation(-currentEnemy.forward);    //使得敌人与我面对面
            RunningMatching(currentCombo, currentComboIndex);
        }
    }



    #region 玩家基础攻击

    #region 角色的普通攻击

    private bool CanBaseAttackInput()
    {
        /*如果在攻击
        *如果在格挡
        *如果在处决敌人
        */
        if (!canAttack) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Parry")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Dash")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish")) return false;
        return true;

    }
    #endregion



    #region 玩家攻击输入
    public override void CharacterBaseAttackInput()
    {
        
        //如果不能攻击
        if (!CanBaseAttackInput() || !canAttack) return;
        if (PlayerInputManager.MainInstance._attack)
        {
            //判断当前的攻击是否为空或基础组合技

            if (currentCombo == null || currentCombo != baseCombo)
            {
                ChangeAttackCombo(baseCombo);
                ResetCombo();   //重置攻击 
            }

            //如果不空，就执行组合技
            ExecuteAction();
        }
        else if (PlayerInputManager.MainInstance.rightAttack)
        {
            Debug.Log("进来饿了");
            ChangeAttackCombo(heavyCombo);
            ExecuteAction();
            attackCount = 0;

        }

    }

    #endregion

    #endregion

}

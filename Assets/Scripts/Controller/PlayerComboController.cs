using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboController : PlayerComboControllerBase
{
    //1.???????????????????????????????????
    //2??????????????

    [SerializeField, Header("???????")] private CharacterComboSO heavyCombo;



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
        MatchPosition();//matchtarget????????????????
    }


    //?????????????????


    protected override void MatchPosition()
    {
        base.MatchPosition();
        if (!animator) return;
        if (currentEnemy == null) return;
        if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1f) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish"))
        {
            transform.rotation = Quaternion.LookRotation(-currentEnemy.forward);    //???????????????
            RunningMatching(currentCombo, currentComboIndex);
        }
    }



    #region ??????????

    #region ????????????

    private bool CanBaseAttackInput()
    {
        /*????????
        *??????
        *????????????
        */
        if (!canAttack) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Parry")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish")) return false;
        return true;

    }
    #endregion



    #region ??????????
    public override void CharacterBaseAttackInput()
    {
        
        //??????????
        if (!CanBaseAttackInput() || !canAttack) return;
        if (PlayerInputManager.MainInstance._attack)
        {
            //?§Ø??????????????????????

            if (currentCombo == null || currentCombo != baseCombo)
            {
                ChangeAttackCombo(baseCombo);
                ResetCombo();   //???¨´??? 
            }

            //?????????????????
            ExecuteAction();
        }
        else if (PlayerInputManager.MainInstance.rightAttack)
        {
            Debug.Log("????????");
            ChangeAttackCombo(heavyCombo);
            ExecuteAction();
            attackCount = 0;

        }

    }

    #endregion

    #endregion

}

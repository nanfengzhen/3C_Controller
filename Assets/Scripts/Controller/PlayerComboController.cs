using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboController : PlayerComboControllerBase
{
    //1.��������Ҫ������ϼ�����ͨ��ϼ����ػ���ϼ�
    //2�����ǵ�ǰ����ϼ�

    [SerializeField, Header("�ػ���ϼ�")] private CharacterComboSO heavyCombo;



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
        MatchPosition();//matchtarget���ڲ��Ŵ�������֮ǰ
    }


    //�ڴ�������ʱ��ƫ����


    protected override void MatchPosition()
    {
        base.MatchPosition();
        if (!animator) return;
        if (currentEnemy == null) return;
        if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1f) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish"))
        {
            transform.rotation = Quaternion.LookRotation(-currentEnemy.forward);    //ʹ�õ������������
            RunningMatching(currentCombo, currentComboIndex);
        }
    }



    #region ��һ�������

    #region ��ɫ����ͨ����

    private bool CanBaseAttackInput()
    {
        /*����ڹ���
        *����ڸ�
        *����ڴ�������
        */
        if (!canAttack) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Parry")) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish")) return false;
        return true;

    }
    #endregion



    #region ��ҹ�������
    public override void CharacterBaseAttackInput()
    {
        
        //������ܹ���
        if (!CanBaseAttackInput() || !canAttack) return;
        if (PlayerInputManager.MainInstance._attack)
        {
            //�жϵ�ǰ�Ĺ����Ƿ�Ϊ�ջ������ϼ�

            if (currentCombo == null || currentCombo != baseCombo)
            {
                ChangeAttackCombo(baseCombo);
                ResetCombo();   //���ù��� 
            }

            //������գ���ִ����ϼ�
            ExecuteAction();
        }
        else if (PlayerInputManager.MainInstance.rightAttack)
        {
            Debug.Log("��������");
            ChangeAttackCombo(heavyCombo);
            ExecuteAction();
            attackCount = 0;

        }

    }

    #endregion

    #endregion

}

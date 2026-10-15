using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerComboControllerBase :MonoBehaviour
{
    protected Animator animator;
    [SerializeField, Header("角色组合技")] protected CharacterComboSO baseCombo;
    protected CharacterComboSO currentCombo;


    [SerializeField, Header("检测范围")] protected float detectionRange;
    [SerializeField] protected float detectDistance;
    protected Vector3 detectDirection;

    /*1.需要一个索引来记录是当前组合技的哪一招
     * 2.攻击的间隔时间
     * 3.是否允许输入攻击信号
     */
    protected int currentComboIndex;
    protected float maxColdTime;
    [SerializeField]protected bool canAttack;
    protected int hitIndex;
    protected int attackCount;

    [SerializeField] protected Transform currentEnemy;
    [SerializeField] protected LayerMask enemyLayer;



    protected Transform cameraGameObject;

    protected Collider[] Units;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        Units = new Collider[0];
    }


    protected virtual void Update()
    {
        MatchPosition();
        LookAtTarget();
        OnEndAttack();
        GetNearUnits();
        ClearEnemy();
        CheckEnemyIsDie();
    }
    private void FixedUpdate()  //执行在Update之后
    {
        //DetectTarget();
        GetOneEnemyUnit();
    }

    #region 检测敌人是否死亡,移除敌人
    public void CheckEnemyIsDie()
    {
        if (currentEnemy == null) return;
        if (currentEnemy.TryGetComponent(out IHealth health))
        {
            if (health.OnDie() && !animator.IsInTransition(0))
            {
                currentEnemy = null;
            }
        }
    } 
    #endregion

    #region 检测敌人（通过人物朝向范围选择敌人）
    /// <summary>
    /// 1.获取玩家朝向
    /// 2.获取玩家范围
    /// 3.玩家何时攻击
    /// 4.玩家攻击内容

    //攻击的内容（受伤+播放受伤动画+播放音效）


    #region 范围检测
    //1.以玩家为中心，在一个半径范围内取敌人
    //2.在当前玩家没有目标的时候，取距离自己最近的一个敌人为当前目标
    //3.自选：(1)当前有目标，不再更新目标，知道当前目标小时或者距离太远
    private void GetNearUnits()
    {
        if (animator.GetFloat("Speed") > 0.7f) return;
        if (currentEnemy != null && DevelopmentToos.DistanceForTarget(currentEnemy, transform) < 1.5f) return;
        //if (animator.GetFloat("Speed") > 0.7f) return;
        Units = Physics.OverlapSphere((transform.position) + transform.up * 0.7f,
        detectionRange, enemyLayer, QueryTriggerInteraction.Ignore);
    }


    private void GetOneEnemyUnit()
    {
        if (currentEnemy != null && DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1.5f)
        {
            currentEnemy = null;
            return;
        } 
        if (Units.Length == 0 || Units == null) return;//当前附近没有敌人
        if (currentEnemy != null && DevelopmentToos.DistanceForTarget(currentEnemy, transform) < 1.5f) return;   //如果当前有敌人，就不再查找(当前距离小于1.5)
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Finish")) return;
        if (animator.GetFloat("Speed") > 0.7f) return;
        float distance = Mathf.Infinity;
        Transform temp_enemy = null;

        //1.依次遍历选择与玩家最近的敌人作为目标
        foreach (var e in Units)
        {
            var dis = DevelopmentToos.DistanceForTarget(transform, e.transform);
            if (dis < distance)
            {
                temp_enemy = e.transform;
                distance = dis;
            }

        }
        currentEnemy = temp_enemy != null ? temp_enemy : currentEnemy;


    }

    private void ClearEnemy()
    {
        if (currentEnemy == null) return;
        if (animator.GetFloat("Speed") > 1f)
        {
            currentEnemy = null;
        }
    }
    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(((transform.position) + transform.up * 0.7f) + detectDirection * detectDistance, detectionRange);
    //}

    private void UpdateDetectDirection()
    {
        detectDirection = (cameraGameObject.forward * PlayerInputManager.MainInstance._run.y)
            + (cameraGameObject.right * PlayerInputManager.MainInstance._run.x);    //获取玩家输入和摄像机共同的朝向
        detectDirection.Set(detectDirection.x, 0f, detectDirection.z);
        detectDirection = detectDirection.normalized;
    }

    #endregion

    #region 招式表变化

    //招式表变化
    protected virtual void ChangeAttackCombo(CharacterComboSO comboSO)
    {
        if (currentCombo != comboSO)
        {
            currentCombo = comboSO;
            ResetCombo();
        }
    }




    #endregion
    #region 朝向目标
    protected virtual void LookAtTarget()
    {
        if (currentEnemy == null) return;
        if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) < 0.1f) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
        {
            transform.Look(currentEnemy.transform.position,150f);
        }
    }
    #endregion

    #region 位置同步
    protected virtual void MatchPosition()
    {
        if (currentEnemy == null) return;
        if (!animator) return;


    }

    protected virtual void RunningMatching(CharacterComboSO comboSO, int index)
    {
        if (!animator.isMatchingTarget)
        {
            animator.MatchTarget(currentEnemy.position + (-transform.forward * currentCombo.GetPositionOffset(currentComboIndex)),
                Quaternion.identity, AvatarTarget.Body, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.35f);

        }


    }
    #endregion


    #region 攻击事件
    protected virtual void ATK()//ATK这个函数是在动画里触发的
    {
        DevelopmentToos.WTF("ATK进来了");
        TriggerDamage();
        UpdateHitInfo();    //放在播放动画后，因为即使未打到别人，如果在动画之前更新，那么会导致后面的hit动画错位

        GamePoolManager.MainInstance.TryGetPoolItem("ATKSound", transform.position, Quaternion.identity);
    }

    protected virtual void DetectTarget()
    {
        if (Physics.SphereCast((transform.position) + transform.up * 0.7f, detectionRange, detectDirection, out var hit, detectDistance, 1 << 9, QueryTriggerInteraction.Ignore))
        {
            currentEnemy = hit.transform;
        }
        //else
        //{
        //    if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1.3f)
        //    {
        //        currentEnemy = null;
        //        DevelopmentToos.WTF("当前敌人已置空");
        //    }
        //}
    }
    #endregion

    #region 伤害触发
    protected virtual void TriggerDamage()
    {

        
        /*1.要确保有目标
         *2.要确保敌人处于我们能触发伤害的距离和范围
         *3.去呼叫事件中心，帮我触发伤害这个函数
         */

        if (currentEnemy == null) return;
        if (Vector3.Dot(transform.forward, DevelopmentToos.DirectionForTarget(currentEnemy, transform)) > 0.75f) return;    //肩并肩或者在后面 
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            if (DevelopmentToos.DistanceForTarget(currentEnemy, transform) > 1.3f) return;
            GameEventManager.MainInstance.CallEvent("对角色造成伤害", currentCombo.GetDamage(currentComboIndex),
                currentCombo.GetComboHitName(currentComboIndex, hitIndex),
                currentCombo.GetComboParryName(currentComboIndex, hitIndex), transform, currentEnemy);//函数名，伤害值，受伤，格挡，当前，敌人
        }


    }
    #endregion


    #region 更新连招信息
    //更新新的招式
    protected virtual void UpdateColdTime()
    {
        currentComboIndex++;
        if (currentComboIndex == currentCombo.GetAllComboCounts())
        {
            currentComboIndex = 0;
        }
        maxColdTime = 0f;
        canAttack = true;
        //Debug.Log("更新新招式:" + currentComboIndex);
    }

    protected virtual void UpdateHitInfo()
    {
        hitIndex++;
        if (hitIndex == currentCombo.GetComboHitAndParryMaxCount(currentComboIndex))
        {
            hitIndex = 0;
        }
    }

    #endregion


    #region 重置攻击
    protected virtual void ResetCombo()
    {
        maxColdTime = 0;
        currentComboIndex = 0;
    }

    //每次人物移动以后，就要重置攻击
    protected virtual void OnEndAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Move") && canAttack)
        {
            ResetCombo();
        }
    }



    #endregion


    #region 基础攻击输入
    public virtual void CharacterBaseAttackInput()
    {

    }
    #endregion


    #region 执行攻击
    protected virtual void ExecuteAction()
    {
        attackCount += (currentCombo == baseCombo) ? 1 : 0;
        //DevelopmentToos.WTF("累计次数：" + attackCount);
        hitIndex = 0;   //受击动画重置
        if (currentComboIndex == currentCombo.GetAllComboCounts()) //到达连招的最后一个就要重置为0
        {
            currentComboIndex = 0;
        }
        Debug.Log("成功播放动画:" + currentComboIndex);
        maxColdTime = currentCombo.GetColdTime(currentComboIndex);
        animator.CrossFadeInFixedTime(currentCombo.GetComboName(currentComboIndex), 0.155555f, 0, 0);
        animator.CrossFadeInFixedTime(currentCombo.GetComboName(currentComboIndex), 0.155555f, 1, 0);
        GameTimerManager.MainInstance.TryGetOneTimer(maxColdTime, UpdateColdTime);   //获得一个计时器,等一段CD以后，再去攻击重置攻击，进行下一段攻击
        canAttack = false;


    }
    #endregion
}

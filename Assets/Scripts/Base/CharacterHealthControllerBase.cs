using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZZZ.HealthData;

public abstract class CharacterHealthControllerBase : MonoBehaviour, IHealth
{
    /*1.都有受伤函数
    *2.都有被处决函数
    *3.都有格挡
    *4.他们的生命值
    */
    protected Transform currentAttacker;
    protected Animator animator;
    [SerializeField, Header("生命值")] protected CharacterHealthInfo healthInfo;


    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        healthInfo.InitCharacterHealthInfo();   //初始化生命体力值
    }

    protected virtual void OnEnable()
    {
        GameEventManager.MainInstance.AddEventListener<float, string, string, Transform, Transform>("对角色造成伤害", OnCharacterHitEventHandle);
        GameEventManager.MainInstance.AddEventListener<string, Transform, Transform>("触发被处决动画", OnCharacterBeFinishEvent);
        GameEventManager.MainInstance.AddEventListener<float, Transform, Transform>("处决伤害", FinishDamage);
    }

    protected virtual void OnDisable()
    {
        GameEventManager.MainInstance.RemoveEvent<float, string, string, Transform, Transform>("对角色造成伤害", OnCharacterHitEventHandle);
        GameEventManager.MainInstance.RemoveEvent<string, Transform, Transform>("触发被处决动画", OnCharacterBeFinishEvent);
        GameEventManager.MainInstance.RemoveEvent<float, Transform, Transform>("处决伤害", FinishDamage);

    }

    protected virtual void Update()
    {
        OnHitLook();
        SetDie();
    }

    /// <summary>
    /// 玩家受伤行为
    /// </summary>
    /// <param name="hitname"，玩家招式表里的受伤名字></param>
    /// <param name="parryname",玩家招式表里的格挡名字></param>
    protected virtual void CharacterHitAction(float damage, string hitname, string parryname)
    {


    }

    protected virtual void SetDie()
    {
        if (healthInfo.IsDie)
        {
            animator.SetBool("isDie", true);

        }
    }

    /// <summary>
    /// 设置攻击者
    /// </summary>
    /// <param name="attacker"></param>
    /// (currentAttacker如果未设置或者需要更换attacker）
    private void SetAttacker(Transform attacker)
    {
        if (currentAttacker != attacker || currentAttacker == null)
        {
            currentAttacker = attacker;
        }
    }



    public bool OnDie() => healthInfo.IsDie;



    //事件--------------------------------------------------

    //设置被打以后该怎么做
    private void OnCharacterHitEventHandle(float damage, string hitname, string parryname, Transform attacker, Transform self)
    {
        //如果当前被打的人不是自己，那说明自己没被打，那么就不格挡
        if (self != transform) return;
        SetAttacker(attacker);  //设置攻击者
        CharacterHitAction(damage, hitname, parryname); //设置被hit的动作和声音
    }

    //1.敌人死亡以后不能看着玩家
    //2.当前玩家未检测到不能看着玩家
    private void OnHitLook()
    {

        if (healthInfo.CurrentHP <= 0f) return;
        if (currentAttacker == null) return;
        if (DevelopmentToos.DistanceForTarget(currentAttacker, transform) < 0.1f) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Parry") 
            || animator.GetCurrentAnimatorStateInfo(0).IsTag("FinishHit")
            && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
        {

            transform.Look(currentAttacker.position,150f);
        }
        if (DevelopmentToos.DistanceForTarget(currentAttacker, transform) > 3f) return;
    }

    private void OnCharacterBeFinishEvent(string finishName, Transform attacker, Transform self)
    {
        if (self != transform) return;   //说明打的不是自己
        SetAttacker(attacker);
        animator.Play(finishName);
        if (healthInfo.CurrentHP <= 0)
        {
            
        }

    }

    private void FinishDamage(float damage, Transform self, Transform attacker)
    {
        if (self != transform) return;
        SetAttacker(attacker);  //设置攻击者
        TakeDamage(damage);

    }

    int i = 0;
    protected void TakeDamage(float damage)
    {
        i++;
        DevelopmentToos.WTF("当前受到伤害次数" + i);
        healthInfo.DamageHP(damage);
        //GameEventManager.MainInstance.CallEvent("更新玩家UI", this.healthInfo);

        //if(healthInfo.CurrentHP <= 0)
        //{
        //    //设置玩家已经死了
        //}
    }

}

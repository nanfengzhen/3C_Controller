using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyHealthControl : CharacterHealthControllerBase
{
    public HealthUI healthUI; // 添加这行：拿到刚才写的 UI 脚本的引用

    protected override void Awake()
    {
        base.Awake();
        EnemyManager.MainInstance.AddEnemy(this.gameObject);
    }


    protected void FadeOutUI()
    {
        if (healthInfo.IsDie)
        {
            // 触发血条渐变消失（例如耗时 1 秒）
            if (healthUI != null)
            {
                healthUI.FadeOut(1.0f);
                Debug.Log("开始消失");
            }
        }

    }

    protected override void CharacterHitAction(float damage, string hitname, string parryname)
    {
        //DevelopmentToos.WTF("确实触发攻击");
        //animator.Play(hitname);
        //如果体力值大于0那么就是格挡,会扣除韧条
        //如果伤害值大于30，那就是破防伤害，扣除生命值
        //if (damage < 30f && healthInfo.StrenthFull)
        //{

        //    //说明要进行格挡
        //    if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        //    {
        //        animator.Play(parryname);
        //        GamePoolManager.MainInstance.TryGetPoolItem("BLOCKSound", transform.position, Quaternion.identity);

        //        TakeDamage(damage);
        //    }
        //    if (!healthInfo.StrenthFull)
        //    {
        //        //韧条被打破，可以被处决
        //        GameEventManager.MainInstance.CallEvent<bool>("触发处决", true);

        //    }

        //}
        //else
        //{
        ////如果生命小于30，也可以进行处决
        //if (healthInfo.CurrentHP < 30f)
        //{
        //    GameEventManager.MainInstance.CallEvent<bool>("触发处决", true);
        //}
        //Debug.Log("进来了");
        
        animator.Play(hitname);
        //GamePoolManager.MainInstance.TryGetPoolItem("HITSound", transform.position, Quaternion.identity);
        TakeDamage(damage);
        DevelopmentToos.WTF("当前生命值" + healthInfo.CurrentHP);
        // 直接通知它自己的 UI 更新血量
        if (healthUI != null)
        {
            healthUI.UpdateHealthImg(healthInfo);
        }
        if (healthInfo.IsDie)
        {
            EnemyManager.MainInstance.RemoveEnemy(this.gameObject);
            FadeOutUI();


        }


        //}


    }





}
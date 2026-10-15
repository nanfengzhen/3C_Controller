using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZZZ.Health;

namespace ZZZ.Health
{
    public class PlayerHealthController : CharacterHealthControllerBase
    {
        public HealthUI healthUI;

        protected override void Update()
        {
            base.Update();
        }

        //先播放，再扣生命值，再更新血量
        protected override void CharacterHitAction(float damage, string hitname, string parryname)
        {
            //GamePoolManager.MainInstance.TryGetPoolItem("HITSound", transform.position, Quaternion.identity);
            animator.Play(hitname, 0, 0f);
            animator.Play(hitname, 1, 0f);
            TakeDamage(damage);
            if (healthUI != null)
            {
                healthUI.UpdateHealthImg(healthInfo);
            }

        }

    }
}

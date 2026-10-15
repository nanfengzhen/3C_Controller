using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

namespace ZZZ.Movement
{
    public class EnemyMovementController : PlayerControllerBase
    {
        // 用于防止在同一个 "SpecialHit" 动画期间重复触发击飞
        private bool isKnockedBack = false;
        private bool applyMovement;
        //1.在移动状态下，敌人应该看着玩家
        //2.在非移动状态下，把移动的值设置为0
        //3.对动画的控制

        protected void Start()
        {
            SetApplyMovement(applyMovement);
            applyMovement = true;
        }
        protected override void Update()
        {
            base.Update();
            EnemyLookAtPlayer();
            DrawDirection();
            // 加入击飞检测
            CheckSpecialHitKnockback();

        }
        private void EnemyLookAtPlayer()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Move"))
            {
                transform.LookAt(EnemyManager.MainInstance.getPlayer());
            }
        }

        // 检查是否需要触发击飞
        private void CheckSpecialHitKnockback()
        {
            // 检查当前动画状态的 Tag 是否为 "SpecialHit"
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("SpecialHit"))
            {
                if (!isKnockedBack)
                {
                    // 启动击飞协程：水平距离 3f，最高点高度 1.5f，持续时间 0.5秒 (时间可根据你的动画长度微调)
                    StartCoroutine(KnockbackRoutine(3f, 1.5f, 0.5f));
                }
            }
            else
            {
                // 动画结束后（离开 SpecialHit tag），重置标志位，以便下次还能被击飞
                isKnockedBack = false;
            }
        }

        // 处理平滑击飞的协程
        private System.Collections.IEnumerator KnockbackRoutine(float horizontalDistance, float jumpHeight, float duration)
        {
            isKnockedBack = true;

            // 1. 获取玩家 Transform
            Transform player = EnemyManager.MainInstance.getPlayer().transform;

            // 2. 计算击飞方向（玩家看向敌人的方向，也就是从玩家位置指向敌人位置）
            Vector3 direction = (transform.position - player.position);
            direction.y = 0; // 忽略Y轴，确保水平方向计算纯粹
            direction.Normalize();

            // 记录起点和计算水平目标点
            Vector3 startPos = transform.position;
            Vector3 targetHorizontalPos = startPos + direction * horizontalDistance;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                // 计算进度比例 (0 到 1)
                float t = elapsedTime / duration;

                // 水平方向：线性插值移动
                Vector3 currentFramePos = Vector3.Lerp(startPos, targetHorizontalPos, t);

                // 垂直方向：使用 Sin 曲线模拟抛物线跳跃 (t=0.5 时达到最高点 jumpHeight)
                float currentYOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                currentFramePos.y += currentYOffset;

                // 执行移动
                if (characterController != null && characterController.enabled)
                {
                    // CharacterController.Move 需要的是【位移差】，所以用当前计算的目标点减去当前实际位置
                    characterController.Move(currentFramePos - transform.position);
                }

                yield return null; // 等待下一帧
            }
        }


        public void SetAnimationMovementValue(float vertical, float horizontal)
        {
            if(applyMovement)
            {
                animator.SetBool("HasInput", true);
                animator.SetFloat("SpeedX", horizontal, 0.2f, Time.deltaTime);
                animator.SetFloat("SpeedY", vertical, 0.2f, Time.deltaTime);
            }
            else
            {
                animator.SetBool("HasInput", false);
                animator.SetFloat("SpeedX", 0f, 0.2f, Time.deltaTime);
                animator.SetFloat("SpeedY", 0f, 0.2f, Time.deltaTime);
            }
        }


        public void SetApplyMovement(bool applyMovement)
        {
            this.applyMovement = applyMovement;
        }

        private void DrawDirection()
        {
            Debug.DrawRay(transform.position + transform.up * 0.75f, (EnemyManager.MainInstance.getPlayer().position - transform.position),Color.yellow);
        }


        public void EnableChracterControl(bool enable)
        {
            characterController.enabled = enable;
        }
    }
}

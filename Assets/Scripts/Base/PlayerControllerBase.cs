using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZZZ {

    [RequireComponent(typeof(CharacterController))]
    public abstract class PlayerControllerBase : MonoBehaviour
    {
        public Animator animator;
        protected CharacterController characterController;
        protected Vector3 moveDirection;





        //地面检测
        [SerializeField,Header("地面检测")]
        public bool isGround;
        [SerializeField] protected float groundDetectionOffset;
        [SerializeField] protected float sphereRange;
        [SerializeField] protected LayerMask whatIsGround;


        //下落速度
        protected float gravity = -9.8f;
        [SerializeField] protected float verticalVelocity;
        protected Vector3 characterVerticalDirection;

        protected PlayerComboController comboController;


        protected virtual void Awake()
        {
            comboController = GetComponent<PlayerComboController>();
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            isGround = true;
        }






        protected virtual void Update()
        {
            CaculateSpeed();
            UpdatePosition();
        }


        //执行在动画播放完之后，用以处理位移
        //protected virtual void OnAnimatorMove()
        //{
        //    animator.ApplyBuiltinRootMotion();
        //    UpdateCharacterMoveDirection(animator.deltaPosition);       //获得动画的每一帧移动方向，以此来控制水平移动方向

        //}


       
        protected virtual void OnAnimatorMove()
        {
            animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(animator.deltaPosition);
        }

        //以下函数在模型位置最终确定后，再进行处理受ik影响的身体部分最终位置
        //private void OnAnimatorIK(int layerIndex)
        //{

        //}

        protected bool GroundCheck()
        {
            var detectionPosition = new Vector3(transform.position.x, transform.position.y - groundDetectionOffset, transform.position.z);
            return Physics.CheckSphere(detectionPosition, sphereRange, whatIsGround, QueryTriggerInteraction.Ignore);
        }


        //private void OnDrawGizmos()
        //{
        //    Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - groundDetectionOffset, transform.position.z), sphereRange);
        //}


        protected virtual void CaculateSpeed()
        {
            isGround = GroundCheck();
            
            if (isGround)
            {
                //落地以后，速度固定为-2f

                verticalVelocity = -2f;

            }
            else //不在地面就开始累加速度
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            animator.SetBool("isGround", isGround);


        }


        //实现人物垂直方向坠落位移
        private void UpdatePosition()
        {
            characterVerticalDirection.Set(0f, verticalVelocity * Time.deltaTime, 0f);
            characterController.Move(characterVerticalDirection);
        }


        private Vector3 slopeDetection(Vector3 moveDirection)
        {
            if(Physics.Raycast(transform.position + transform.up*0.5f,Vector3.down,out var hit, characterController.height * 0.85f,whatIsGround, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Dot(Vector3.up, hit.normal) != 0)
                {
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
                return moveDirection;
            }
            return moveDirection;
        }



        //更新移动的方向
        protected void UpdateCharacterMoveDirection(Vector3 direction)
        {
            moveDirection = slopeDetection(direction);
            characterController.Move(moveDirection);
        }




        


    }
}



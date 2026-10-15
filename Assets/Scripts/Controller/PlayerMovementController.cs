using GGG.Tool;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using ZZZ;

public class PlayerMovementController :PlayerControllerBase
{

    public GameObject weaponObject;
    public Transform CameraTransform;
    static Transform PlayerTransform;



    [SerializeField, Header("玩家状态")]
    public StateType PlayerState;
    public bool isDash = false;
    public bool isSprint = false;
    public bool isWalk = false;


    [SerializeField, Header("状态时间")]
    
    private float nextTime;
    [SerializeField]private float walkTime;
    [SerializeField]private float runTime;
    [SerializeField]private float sprintTime;
    public float keepDashTime = 0.7f;     //为了保证能够在dash状态亦能冲刺，所以keepdash时间设置成dash的时长
    public float currentDashTime = 0;



    [SerializeField, Header("跳跃相关")]
    public float Feet;
    public float characterHeight;
    private bool isJump;
    [SerializeField]private float fallDeltaTime;
    private float fallTime;
    public float JumpVelocity;      //自定义初始速度

    //求水平初速度
    static readonly int CACHE_SIZE = 3;
    Vector3[] velcache = new Vector3[CACHE_SIZE];
    int currentCacheIndex;
    private Vector3 averageVel;


    RaycastHit hit;
    






    protected override void Awake()
    {
        base.Awake();
        PlayerFSM.MainInstance.AddState(StateType.Walk, new PlayerWalkState(this,comboController));
        PlayerFSM.MainInstance.AddState(StateType.Idle, new PlayerIdleState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Run, new PlayerRunState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Attack, new PlayerAttackState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Jump, new PlayerJumpState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.RunJump, new PlayerRunJumpState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.WalkJump, new PlayerWalkJumpState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Sprint, new PlayerSprintState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Dash, new PlayerDashState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.SprintJump, new PlayerSprintJumpState(this, comboController));
        PlayerFSM.MainInstance.AddState(StateType.Landing, new PlayerLandingState(this, comboController));
    }


    private void Start()
    {
        fallTime = 0.05f;
        fallDeltaTime = fallTime;
        CameraTransform = Camera.main.transform;
        PlayerTransform = transform;
        PlayerFSM.MainInstance.SetState(StateType.Idle);
    }

    protected override void Update()
    {
        HeightCheck();
        PlayerFSM.MainInstance.OnTink();
        GetCurrentState();
        SetShowWeapon(); 
        

        GroundSearch();
        PlayerJump();
        CaculateSpeed();
        UpdatePlayerMoveState();
        FollowCameraRotation();
       
    }

    private bool CanShowWeapon()
    {
        if (animator.AnimationAtTag("Attack") || animator.AnimationAtTag("WeaponIdle")) return true;
        return false;
    }

    private void SetShowWeapon()
    {
        if(weaponObject != null)
        {
            weaponObject.SetActive(CanShowWeapon());
        }
        
    }

    #region 持续性动画更新

    public void ResetState()
    {
        isDash = false;
        isWalk = false;
        isSprint = false;
        currentDashTime = 0;
    }
    private void SetDash()
    {
        if (PlayerInputManager.MainInstance._sprint)
        {

            isDash = true;
            isSprint = false;
            isWalk = false;
            //    //isSprint = !isSprint;
            //    //if(isSprint) isWalk = false;

            if (currentDashTime <= 0)
            {
                currentDashTime = keepDashTime;
            }
        }
        else if (!animator.GetBool("HasInput"))
        {
            isDash = false;
            isSprint = false;
            currentDashTime = keepDashTime;
        }


    }

    //是否继续坚持冲刺
    public void KeepSprint()
    {
        if (isDash)
        {
            currentDashTime -= Time.deltaTime;
            if (currentDashTime < 0)
            {
                isSprint = true;
                isDash = false;
                isWalk = false;

            }
        }
        //冲刺跳跃落地以后，也能进行冲刺
        if (animator.GetBool("HasInput") && animator.GetCurrentAnimatorStateInfo(0).IsTag("SprintBuffer"))
        {
            isSprint = true;
        }
    }


    private void SetWalk()
    {
        if (PlayerInputManager.MainInstance._walk)
        {
            isWalk = !isWalk;
            isSprint = false;
            isDash = false;
        }
        if (animator.GetBool("HasInput") && animator.GetCurrentAnimatorStateInfo(0).IsTag("WalkBuffer"))
        {
            isWalk = true;
        }


    }

    private void UpdatePlayerMoveState()
    {
        float targetSpeed = 0f;
        if (!isGround) return;
        animator.SetBool("HasInput", PlayerInputManager.MainInstance._run != Vector2.zero);

        SetWalk();
        SetDash();
        KeepSprint();

        if (animator.GetBool("HasInput"))
        {
            targetSpeed = 4f;
            animator.SetBool("IsRun", true);

            if (isWalk)
            {
                animator.SetBool("IsRun", false);
                targetSpeed = 1f;
            }
            else if (isSprint || isDash)
            {
                targetSpeed = 9f;
            }
            
            PlayFootSound();
            animator.SetFloat("Speed", targetSpeed, 0.2f, Time.deltaTime);

           
        }
        //松开键盘
        else
        {
            targetSpeed = 0f;
            if (animator.GetFloat("Speed") < 2.5f)
            {
                animator.SetBool("IsRun", false);//如果涉及runend与walkend，将run的设值放在speed设值的后面
            }
            animator.SetFloat("Speed", targetSpeed, 0.2f, Time.deltaTime);


        }

    } 
    #endregion


    private bool CanRotation()
    {
        if (animator.AnimationAtTag("Attack")) return false;
        if (animator.AnimationAtTag("WeaponIdle")) return false;
        if(PlayerInputManager.MainInstance._run == Vector2.zero) return false;
        return true;
    }

    void FollowCameraRotation()
    {
        float _TargetRotation = 0f;
        float _CurrentRotaion = 0f;
        float _rotationVelocity = 0f;
        //Vector3 PlayerMoveDirection = new Vector3(CameraTransform.forward.x, 0f, CameraTransform.forward.z);
        Vector3 InputDirection = new Vector3(PlayerInputManager.MainInstance._run.x, 0f, PlayerInputManager.MainInstance._run.y);

        //PlayerMoveDirection = PlayerTransform.InverseTransformDirection(PlayerMoveDirection);
        if (CanRotation())
        {
            _TargetRotation = Mathf.Atan2(InputDirection.x, InputDirection.z) * Mathf.Rad2Deg + CameraTransform.transform.eulerAngles.y;
            _CurrentRotaion = Mathf.SmoothDampAngle(transform.eulerAngles.y, _TargetRotation, ref _rotationVelocity, 0.02f);
            //PlayerTransform.rotation = Quaternion.Euler(0f, _CurrentRotaion, 0f);
            PlayerTransform.eulerAngles = new Vector3(0f, _CurrentRotaion, 0f);
        }

        
    }


    #region 高度射线检测
    private void HeightCheck()
    {
         if(Physics.Raycast(transform.position, Vector3.down,out hit, whatIsGround))
        {
            characterHeight = hit.distance;
            //DevelopmentToos.WTF("当前的高度:" + characterHeight);
        }
    }
    #endregion


    #region 地面检测和跳跃计算
    protected void GroundSearch()
    {
        // 1. 正在跳跃上升阶段
        if (verticalVelocity > 0f)
        {
            isGround = false;
            fallDeltaTime = fallTime;
            animator.SetBool("isGround", isGround);
            return;
        }

        // 2. 物理上贴合地面
        if (GroundCheck())
        {
            isGround = true;
            fallDeltaTime = fallTime;
        }
        
        else
        {
            if (fallDeltaTime > 0 && PlayerState != StateType.Jump && PlayerState != StateType.SprintJump && PlayerState != StateType.RunJump && PlayerState != StateType.WalkJump)
            {
                fallDeltaTime -= Time.deltaTime;
                isGround = true;
            }
            else
            {
                isGround = false;
            }
        }

        // 4. 更新动画状态机
        animator.SetBool("isGround", isGround);
    }

   


    protected override void CaculateSpeed()
    {
        if (isGround)
        {
            //落地以后，速度固定为-2f

            verticalVelocity = -2f;

        }
        else //不在地面就开始累加速度
        {
            verticalVelocity += gravity * Time.deltaTime;
        }



    }
    void PlayerJump()
    {
        if (!CanJump()) return;
        if (isGround && PlayerInputManager.MainInstance._jump)
        {
            //DevelopmentToos.WTF("进来没");
            verticalVelocity = JumpVelocity;
            //播放跳跃动画
            isGround = false;
        }


    }


    public bool CanJump()
    {
        if (!isGround) return false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("WalkBuffer") || 
            animator.GetCurrentAnimatorStateInfo(0).IsTag("RunBuffer")  || 
            animator.GetCurrentAnimatorStateInfo(0).IsTag("SprintBuffer"))
        { 
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.2f)    //缓冲时间为0.5f
            {
                return false;
            }

        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Landing"))
        {
            return false;
        }
        return true;
    }

    public bool LoadingBufferTime()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("WalkBuffer")
            || animator.GetCurrentAnimatorStateInfo(0).IsTag("RunBuffer")
            || animator.GetCurrentAnimatorStateInfo(0).IsTag("SprintBuffer"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.15f)    //缓冲时间为0.5f
            {
                return true;
            }

        }
        return false;
    }

    #endregion

    #region 播放声音
    public void PlayFootSound()
    {
        if (animator.GetFloat("Speed") > 0.1f && animator.GetCurrentAnimatorStateInfo(0).IsTag("Move"))
        {
            nextTime -= Time.deltaTime;
            if (nextTime < 0)
            {
                SwitchSound();
            }
        }

    }

    private void SwitchSound()
    {
        if (animator.GetFloat("Speed") > 3f && animator.GetFloat("Speed") < 8f)
        {
            nextTime = runTime;
        }
        else if (animator.GetFloat("Speed") < 3f)
        {
            nextTime = walkTime;
        }
        else if (animator.GetFloat("Speed") > 6f)
        {
            nextTime = sprintTime;
        }
        if (nextTime == walkTime)
        {
            GamePoolManager.MainInstance.TryGetPoolItem("WALKSound", transform.position, Quaternion.identity);
        }
        else if (nextTime == runTime)
        {
            GamePoolManager.MainInstance.TryGetPoolItem("RUNSound", transform.position, Quaternion.identity);
        }
        else if (nextTime == sprintTime)
        {
            GamePoolManager.MainInstance.TryGetPoolItem("RUNSound", transform.position, Quaternion.identity);
        }

    } 
    #endregion

  

    protected override void OnAnimatorMove()
    {

        Vector3 playerDeltaMovement = Vector3.zero;
        //将脚本里定义的下落速度与游戏内一致
        //if (isGrounded)         //在地面上计算
        //{
        //    averageVel = AverageVel(animator.velocity);//将下坠前最后三帧的水平方向平均速度作为平抛运动惯性
            playerDeltaMovement = animator.deltaPosition;

        //}
        //else if(!isGrounded)
        //{
        //    playerDeltaMovement.x = averageVel.x * Time.deltaTime;
        //    playerDeltaMovement.z = averageVel.z * Time.deltaTime;
        playerDeltaMovement.y = verticalVelocity * Time.deltaTime;
        //}

        characterController.Move(playerDeltaMovement);

    }


    public void GetCurrentState()
    {
        if (animator.AnimationAtTag("Attack"))
        {
            PlayerState = StateType.Attack;
        }
        if (animator.AnimationAtTag("Move"))
        {
            if(animator.GetFloat("Speed") < 1f)
            {
                PlayerState = StateType.Walk;
            }
            else if(animator.GetBool("IsRun") && animator.GetFloat("Speed") < 8f)
            {
                PlayerState = StateType.Run;
            }
            else if(animator.GetFloat("Speed") > 8f)
            {
                PlayerState = StateType.Sprint;
            }
            
        }
        if (animator.AnimationAtTag("Jump"))
        {
            PlayerState = StateType.Jump;
        }
        if (animator.AnimationAtTag("WalkJump"))
        {
            PlayerState = StateType.WalkJump;
        }
        if (animator.AnimationAtTag("RunJump"))
        {
            PlayerState = StateType.RunJump;
        }
        if (animator.AnimationAtTag("SprintJump"))
        {
            PlayerState = StateType.SprintJump;
        }
        if (animator.AnimationAtTag("Dash"))
        {
            PlayerState = StateType.Dash;
        }
        if(animator.AnimationAtTag("WeaponIdle") || animator.AnimationAtTag("Idle"))
        {
            PlayerState = StateType.Idle;
        }
        if (animator.AnimationAtTag("Landing") || animator.AnimationAtTag("WalkBuffer") || animator.AnimationAtTag("RunBuffer") || animator.AnimationAtTag("SprintBuffer"))
        {
            PlayerState = StateType.Landing;
        }
    }

    public Vector3 AverageVel(Vector3 newvel)
    {
        velcache[currentCacheIndex] = newvel;
        currentCacheIndex++;
        currentCacheIndex %= CACHE_SIZE;
        Vector3 average = Vector3.zero;
        foreach (Vector3 vel in velcache)
        {
            average += vel;

        }

        return average / CACHE_SIZE;
    }
}

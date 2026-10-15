using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerFootIK : MonoBehaviour
{
    private Animator animator;

    [Header("调试与可视化")]
    [Tooltip("在Scene窗口中绘制IK射线和目标点")]
    public bool showDebugGizmos = true;

    [Header("IK 设置 (核心探测)")]
    public bool enableFeetIK = true;
    public LayerMask environmentLayer;
    public float footOffsetY = 0.1f;
    public float raycastDistance = 1.0f;
    public float maxStepHeight = 0.4f;
    public float footRadius = 0.15f;

    [Header("平滑过渡 (防拖拽)")]
    public float footPositionSpeed = 15f;
    public float footRotationSpeed = 15f;
    public float pelvisUpSpeed = 5f;
    public float pelvisDownSpeed = 5f;

    [Header("盆骨约束 (防蹲走)")]
    public float maxPelvisDrop = 0.35f;
    public float footHeightTolerance = 0.3f;

    [Header("脊椎弯曲 (Spine Bend)")]
    public bool enableSpineBend = true;
    public float spineBendMultiplier = 100f;
    public float maxSpineBendAngle = 40f;
    public float spineBendSpeed = 4f;

    [Header("头部视线 (Head LookAt)")]
    public bool enableHeadLook = true;
    public float lookAheadDistance = 2.0f;
    public float headLookSpeed = 5.0f;
    [Range(0f, 1f)] public float headWeight = 0.7f;
    public float lookUpThreshold = 0.2f;
    public float lookUpMultiplier = 0.4f;
    public float lookDownMultiplier = 1.2f;
    public float maxDropLimit = 1.5f;
    public float maxWallLimit = 1.5f;

    // 状态缓存
    private float lastPelvisOffset = 0f;
    private Transform spine;
    private Transform chest;
    private float currentSpineBendAngle = 0f;
    private float defaultHeadHeight;
    private Vector3 currentLookPosition;
    private float currentHeadIkWeight = 0f;

    private float leftFootIkOffset = 0f;
    private float rightFootIkOffset = 0f;
    private Quaternion leftFootIkRot = Quaternion.identity;
    private Quaternion rightFootIkRot = Quaternion.identity;
    private bool isFirstFrame = true;

    // ================= 调试专用缓存变量 =================
    private Vector3 dbgLeftOrigin, dbgLeftHit, dbgRightOrigin, dbgRightHit;
    private bool dbgLeftValid, dbgRightValid;
    private Vector3 dbgHeadRayOrigin, dbgHeadGroundHit, dbgHeadTarget;
    private Color dbgHeadColor = Color.white;
    private string dbgHeadState = "平视";

    void Start()
    {
        animator = GetComponent<Animator>();

        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);

        Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone != null)
        {
            defaultHeadHeight = headBone.position.y - transform.position.y;
        }
        else
        {
            defaultHeadHeight = 1.5f;
        }

        currentLookPosition = transform.position + transform.forward * lookAheadDistance + Vector3.up * defaultHeadHeight;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !enableFeetIK) return;

        if (isFirstFrame)
        {
            leftFootIkRot = animator.GetIKRotation(AvatarIKGoal.LeftFoot);
            rightFootIkRot = animator.GetIKRotation(AvatarIKGoal.RightFoot);
            isFirstFrame = false;
        }

        HandleHeadLookAt();

        UpdateFootIK(AvatarIKGoal.LeftFoot, ref leftFootIkOffset, ref leftFootIkRot);
        UpdateFootIK(AvatarIKGoal.RightFoot, ref rightFootIkOffset, ref rightFootIkRot);

        MovePelvisHeight();
    }

    private void LateUpdate()   //发生在所有动画处理完之后
    {
        SpineBend();
    }

    private void SpineBend()
    {
        if (!enableSpineBend || spine == null) return;

        float targetBendAngle = 0f;
        if (lastPelvisOffset < 0)   //人物下沉
        {
            targetBendAngle = Mathf.Abs(lastPelvisOffset) * spineBendMultiplier;    //总弯曲角度=下沉量*spineBendMultiplier
            targetBendAngle = Mathf.Clamp(targetBendAngle, 0f, maxSpineBendAngle);
        }

        currentSpineBendAngle = Mathf.Lerp(currentSpineBendAngle, targetBendAngle, Time.deltaTime * spineBendSpeed);

        if (currentSpineBendAngle > 0.1f)
        {
            float halfAngle = currentSpineBendAngle / 2f;//分摊弯曲量，一半给spine，一半给chest
            spine.rotation = Quaternion.AngleAxis(halfAngle, transform.right) * spine.rotation;

            if (chest != null)
            {
                chest.rotation = Quaternion.AngleAxis(halfAngle, transform.right) * chest.rotation;
            }
        }
    }

    private void UpdateFootIK(AvatarIKGoal foot, ref float currentYOffset, ref Quaternion currentRot)
    {
        Vector3 animPos = animator.GetIKPosition(foot); //这个只是脚踝的高度
        Quaternion animRot = animator.GetIKRotation(foot);

        Vector3 rayOrigin = animPos + Vector3.up * raycastDistance; //把检测的起始点抬高，然后再射下来
        float virtualGroundY = animPos.y - footOffsetY; //初始状态下，地面的高度就是脚的高度
        Vector3 hitNormal = Vector3.up;
        bool hitValid = false;

        Vector3 sphereCastOrigin = rayOrigin + Vector3.up * footRadius;

        if (Physics.SphereCast(sphereCastOrigin, footRadius, Vector3.down, out RaycastHit hit, raycastDistance * 2f, environmentLayer))
        {
            //要进行ik检测的条件（地面与角色实际位置的高度之差是否小于阈值）
            if (hit.point.y - transform.position.y <= maxStepHeight)    
            {
                virtualGroundY = rayOrigin.y - hit.distance;
                hitNormal = hit.normal;
                hitValid = true;
            }
        }

        // --- 记录 Debug 信息 ---
        if (foot == AvatarIKGoal.LeftFoot)
        {
            dbgLeftOrigin = sphereCastOrigin;
            dbgLeftValid = hitValid;
            dbgLeftHit = hitValid ? hit.point : sphereCastOrigin + Vector3.down * (raycastDistance * 2f);
        }
        else
        {
            dbgRightOrigin = sphereCastOrigin;
            dbgRightValid = hitValid;
            dbgRightHit = hitValid ? hit.point : sphereCastOrigin + Vector3.down * (raycastDistance * 2f);
        }
        // ----------------------

        float targetYOffset = 0f;
        Quaternion targetRot = animRot;

        // 【新增逻辑】：检测当前 Animator 播放的状态标签
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isJumping = stateInfo.IsTag("Jump") || stateInfo.IsTag("RunJump") || stateInfo.IsTag("WalkJump") || stateInfo.IsTag("SprintJump");
        // 注：如果你后续加了 WalkJump，可以在后面加上 || stateInfo.IsTag("WalkJump")

        // 【逻辑修改】：只有在探测到地面，且“不处于跳跃状态”时，才进行 IK 贴合计算
        if (hitValid && !isJumping)
        {
            // 【史诗级核心修复】：判断动画是否抬脚，必须基于人物的Root节点高度！
            // 彻底切断了地形高低对抬脚判定的错误干扰
            float footHeightFromRoot = (animPos.y - footOffsetY) - transform.position.y;    //计算出当前脚与地面的相对位置，一旦抬起的高度为footHeightTolerance的最大限制高度，那就会归为0，

            // 现在的 weight 只有在动画真正播放“腾空跨步”时才会平滑减少
            float weight = 1f - Mathf.Clamp01(footHeightFromRoot / footHeightTolerance);    //越小，受footIK影响越小，越大，受footIK影响越大
            //Debug.Log("当前的权重值" + weight);
            // 算出目标高度差，并乘上权重
            float requiredOffset = (virtualGroundY + footOffsetY) - animPos.y;  //计算动画脚的位置与实际地面的相对高度差（注意加上脚的厚度）
            //Debug.Log("当前的偏移量" + requiredOffset);
            targetYOffset = requiredOffset * weight;

            Quaternion ikRot = Quaternion.FromToRotation(Vector3.up, hitNormal) * animRot;
            targetRot = Quaternion.Slerp(animRot, ikRot, weight);
        }

        // 因为这里用的是 Lerp 插值，所以当起跳瞬间 (isJumping 变 true) 时：
        // targetYOffset 瞬间归零，但 currentYOffset 会非常平滑地退回到零。
        // 这完美模拟了真实人类起跳时“顺滑收腿”的动态感，告别生硬抽搐！
        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, Time.deltaTime * footPositionSpeed); //通过插值计算出offset的偏移值
        currentRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * footRotationSpeed);
        //Debug.Log("当前的offset" + currentYOffset);
        Vector3 finalPos = animPos  + Vector3.up * currentYOffset ;   //将当前的偏移量加到animPos去

        animator.SetIKPositionWeight(foot, 1f);
        animator.SetIKRotationWeight(foot, 1f);
        animator.SetIKPosition(foot, finalPos);
        animator.SetIKRotation(foot, currentRot);
    }

    private void MovePelvisHeight()
    {
        float targetOffset = Mathf.Min(leftFootIkOffset, rightFootIkOffset);
        targetOffset = Mathf.Clamp(targetOffset, -maxPelvisDrop, 0f);   //下沉量为负数

        float speed = (targetOffset < lastPelvisOffset) ? pelvisDownSpeed : pelvisUpSpeed;  //下沉快，上升慢
        lastPelvisOffset = Mathf.Lerp(lastPelvisOffset, targetOffset, Time.deltaTime * speed);

        animator.bodyPosition += Vector3.up * lastPelvisOffset;
    }

    private void HandleHeadLookAt()
    {
        if (!enableHeadLook) return;

        float currentGroundY = transform.position.y;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit rootHit, 2f, environmentLayer))
        {
            currentGroundY = rootHit.point.y;
        }

        Vector3 aheadPos = transform.position + transform.forward * lookAheadDistance;
        Vector3 rayOrigin = aheadPos + Vector3.up * 2f;
        Vector3 targetLookPos = aheadPos;

        float targetYOffset = 0f;
        float targetWeight = 0f;

        // --- Debug 初始化 ---
        dbgHeadRayOrigin = rayOrigin;
        dbgHeadGroundHit = aheadPos;
        dbgHeadColor = Color.yellow;
        dbgHeadState = "平地 (忽略)";
        // ------------------

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 4f, environmentLayer))
        {
            float heightDiff = hit.point.y - currentGroundY;
            dbgHeadGroundHit = hit.point; // 记录前方探测点

            if (heightDiff > lookUpThreshold && heightDiff <= maxWallLimit)
            {
                targetYOffset = (heightDiff - lookUpThreshold) * lookUpMultiplier;
                targetWeight = headWeight;
                dbgHeadColor = Color.green; // 绿色代表抬头
                dbgHeadState = "上坡/上楼梯 (抬头)";
            }
            else if (heightDiff < -0.1f && heightDiff >= -maxDropLimit)
            {
                targetYOffset = heightDiff * lookDownMultiplier;
                targetWeight = headWeight;
                dbgHeadColor = new Color(1f, 0.5f, 0f); // 橙色代表低头
                dbgHeadState = "下坡/下楼梯 (低头)";
            }
            else if (heightDiff > maxWallLimit)
            {
                dbgHeadColor = Color.red; // 红色代表高墙悬崖
                dbgHeadState = "死路/高墙 (阻断)";
            }
            else if (heightDiff < -maxDropLimit)
            {
                dbgHeadColor = Color.red;
                dbgHeadState = "悬崖/跳空 (阻断)";
            }
        }

        targetLookPos.y = currentGroundY + defaultHeadHeight + targetYOffset;
        currentLookPosition = Vector3.Lerp(currentLookPosition, targetLookPos, Time.deltaTime * headLookSpeed);
        currentHeadIkWeight = Mathf.Lerp(currentHeadIkWeight, targetWeight, Time.deltaTime * headLookSpeed);

        dbgHeadTarget = currentLookPosition; // 记录最终视线位置

        if (currentHeadIkWeight > 0.01f)
        {
            animator.SetLookAtWeight(currentHeadIkWeight, 0.1f * currentHeadIkWeight, currentHeadIkWeight, 1f, 0.5f);
            animator.SetLookAtPosition(currentLookPosition);
        }
        else
        {
            animator.SetLookAtWeight(0f);
        }
    }

    // ================= 可视化绘制模块 =================

    private void Update()
    {
        // 当按下 Q 键时，对 showDebugGizmos 取反 (true 变 false，false 变 true)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            showDebugGizmos = !showDebugGizmos;
            Debug.Log("IK调试辅助线显示状态: " + (showDebugGizmos ? "开启" : "关闭"));
        }
    }
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying || animator == null) return;

        // 1. 绘制腿部 IK (SphereCast球体和触碰点)
        DrawFootGizmo(dbgLeftOrigin, dbgLeftHit, dbgLeftValid, Color.cyan);   // 左脚青色
        DrawFootGizmo(dbgRightOrigin, dbgRightHit, dbgRightValid, Color.blue); // 右脚深蓝色

        // 2. 绘制头部 IK
        if (enableHeadLook)
        {
            // 画出视线前方的探测射线
            Gizmos.color = Color.white;
            Gizmos.DrawLine(dbgHeadRayOrigin, dbgHeadGroundHit);
            Gizmos.DrawWireCube(dbgHeadGroundHit, new Vector3(0.2f, 0.05f, 0.2f)); // 前方地面实际高度点

            // 绘制头部到目标点的视线
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null)
            {
                Gizmos.color = dbgHeadColor; // 颜色会根据状态改变
                Gizmos.DrawLine(head.position, dbgHeadTarget);

                // 在视线终点画一个代表“焦点”的球
                Gizmos.DrawSphere(dbgHeadTarget, 0.08f);
            }
        }
    }

    private void DrawFootGizmo(Vector3 origin, Vector3 hitPoint, bool isValid, Color color)
    {
        // 绘制球体扫描器的起始点和范围
        Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
        Gizmos.DrawWireSphere(origin, footRadius);

        // 如果打到了地面，绘制连接线和落脚点
        if (isValid)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(origin, hitPoint);

            // 画一个绿色小方块代表最终确定的踩踏点
            Gizmos.color = Color.green;
            Gizmos.DrawCube(hitPoint, new Vector3(0.1f, 0.05f, 0.1f));
        }
        else
        {
            // 没打到地面画红色警告线
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, hitPoint);
        }
    }
}
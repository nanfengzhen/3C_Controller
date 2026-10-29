using System;
using UnityEngine;

namespace ArticleFootIK
{
    /// <summary>
    /// 纸橙《【UE5】关于预测IK的个人思考与小Trick》的 Unity Humanoid 移植。
    /// 注释中的 Sxx 对应 ARTICLE_MAPPING.md 的原文位置和摘录。
    /// 四条曲线只读；所有原动画输入先快照，再写 IK Goal，禁止读取上轮输出作为原动画。
    /// Unity 适配：以 Animator.bodyPosition 应用身体高度差，不修改 Actor、胶囊或 root Transform。
    /// 这不是 UE Mesh 位移的逐接口复刻，也不声称修复了作者的 Mesh 反馈累积问题。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class ArticlePredictiveFootIK : MonoBehaviour
    {
        [Header("角色引用：脚本必须与 Animator 在同一物体")]
        [Tooltip("负责角色世界移动的根物体；不要拖入 Hips/脚骨。未指定时为 Animator.transform。")]
        public Transform actorRoot;
        [Tooltip("动画模型的基准原点；不是 Hips。默认 Animator.transform。不会直接移动它。")]
        public Transform poseRoot;
        public CharacterController characterController;
        [Min(0)] public int ikLayer = 0;

        [Header("四条动画 Float 曲线 [S01]，名称与截图一致")]
        public string leftState = "LeftFootState";
        public string rightState = "RightFootState";
        public string leftDownTime = "LeftFootDownTime";
        public string rightDownTime = "RightFootDownTime";
        [Range(0.01f, 0.99f)] public float contactThreshold = 0.5f;

        [Header("运动状态与模式 [S16][S17]")]
        [Tooltip("项目现有 Bool。留空时必须指定 CharacterController 或通过 SetMotionContext 提供状态。")]
        public string groundedParameter = "isGround";
        [Tooltip("项目循环移动 State 的 Tag。留空时仅检查 loop 和速度。")]
        public string locomotionTag = "Move";
        public string dashTag = "Dash";
        public string attackTag = "Attack";
        public string hitTag = "Hit";
        public string landing = "Landing";




        [Min(0)] public float minimumPredictionSpeed = 0.1f;
        [Min(0)] public float landingTraditionalSeconds = 0.15f;
        [Tooltip("工程保守降级：混合期间用传统 IK，过渡结束后重新记录落脚。并非作者已解决混合预测误差。")]
        public bool traditionalDuringTransitions = true;
        [Tooltip("曲线存储原始 Clip 秒数时开启；按 Animator 与 State 实际播放倍率换算。")]
        public bool timeCurveUsesClipSeconds = true;

        [Header("步距与落点 [S04][S05]；世界单位默认米")]
        [Tooltip("仅修正记录位置相对 Actor 的 XZ 偏移，不再次缩放真实速度。已有步距处理时须保持一致。")]
        [Min(0.01f)] public float strideScale = 1f;
        [Min(0)] public float reusePredictionDistance = 0.03f;
        [Min(0.01f)] public float maximumPredictionSeconds = 2f;

        [Header("地形检测 [S07]：Ground Mask 必须排除角色自身")]
        public LayerMask groundMask;
        [Min(0.001f)] public float footHeight = 0.08f;
        [Min(0.01f)] public float probeUp = 0.65f;
        [Min(0.01f)] public float probeDown = 1.2f;
        [Range(0, 89)] public float maximumGroundSlope = 65f;
        [Range(1, 32)] public int maximumPathInsertions = 12;

        [Header("FootPath 艺术调整 [S08][S10][S14]，默认关闭额外修饰")]
        [Min(0)] public float pathMiddleLift = 0f;
        [Min(0)] public float uphillLateLift = 0f;
        [Min(0)] public float runningPathLift = 0f;
        [Min(0)] public float runSpeedThreshold = 2.5f;
        [Range(0, 1)] public float runningAnimationLiftOnSlope = 0.5f;
        [Min(0)] public float slopeHeightThreshold = 0.04f;

        [Header("采样平滑与踩地锁定 [S09][S11]")]
        [Min(0.01f)] public float walkingHeightSpeed = 16f;
        [Min(0.01f)] public float runningHeightSpeed = 24f;
        [Min(1)] public float downwardSpeedMultiplier = 1.5f;
        [Min(0.01f)] public float plantedPositionSpeed = 30f;
        [Min(0.01f)] public float ikBlendSpeed = 15f;

        [Header("较低脚重建身体高度 [S13][S14]")]
        public bool reconstructBody = true;
        [Min(0.001f)] public float bodySmoothTime = 0.08f;
        [Min(0)] public float maximumBodyOffset = 0.45f;
        [Tooltip("只在确定进入楼梯区域时由关卡/移动代码打开；不把所有斜坡都认成楼梯。")]
        public bool stairMode;
        [Min(0)] public float ascendingBodyDrop = 0.05f;
        [Min(0)] public float descendingBodyDrop = 0.08f;

        [Header("脚底贴坡 [S15]")]
        public bool alignPlantedFeet = true;
        [Min(0.01f)] public float rotationSpeed = 20f;
        public bool drawGizmos = true;

        [Header("转向时暂时关闭 IK")]
        [Tooltip("角色水平转向速度超过阈值时，暂时释放双脚 IK，避免旧落脚点在转向过程中继续约束角色。")]
        public bool disableIKDuringTurn = true;
        [Min(0f)] public float turnAngularSpeedThreshold = 45f;

        public Vector3 HorizontalVelocity { get; private set; }
        public bool PredictionActive { get; private set; }
        public float TargetVirtualRootHeight { get; private set; }
        public float AppliedBodyOffset { get { return bodyOffset; } }
        public string Status { get; private set; }

        private sealed class Foot
        {
            public AvatarIKGoal goal;
            public int stateId, timeId;
            public Vector3 pose, target, anchor, predicted, referenceLocal;
            public Quaternion poseRotation, targetRotation;
            public Vector3 anchorNormal = Vector3.up, predictedNormal = Vector3.up;
            public bool contact, observed, previousContact, hasAnchor, hasReference, predictionValid;
            public bool landed, validTarget, smoothed, rotationReady;
            public float downTime, weight, simpleHeight;
            public readonly PredictedFootPath path = new PredictedFootPath();
            public void ResetPrediction()
            {
                observed = false;
                hasAnchor = hasReference = predictionValid = false;
                path.Clear();
            }
        }

        private Animator animator;
        private Foot left, right;
        private int groundedId;
        private bool hasGroundedParameter, wasGrounded, wasPredicting, havePosition;
        private Vector3 previousActorPosition;
        private Vector3 previousActorForward;
        private bool haveActorForward;
        private float landingTimer, bodyOffset, bodyVelocity, elapsed;
        private float startedAt;
        private int callbackCount;
        private bool externalMotion;
        private Vector3 suppliedVelocity;
        private bool suppliedGrounded, suppliedPredictionAllowed;
        private float suppliedPlaybackRate = 1f;
        private bool rearIsLeft, stepSelectionReady, stepValid;
        private Vector3 stepStart, stepEnd;

        private void OnEnable()
        {
            animator = GetComponent<Animator>();
            if (actorRoot == null) actorRoot = transform;
            if (poseRoot == null) poseRoot = transform;
            left = new Foot { goal = AvatarIKGoal.LeftFoot };
            right = new Foot { goal = AvatarIKGoal.RightFoot };
            if (!ValidateConfiguration()) { enabled = false; return; }
            ResetTracking();
            startedAt = Time.realtimeSinceStartup;
            callbackCount = 0;
        }

        /// <summary>
        /// 可选接入点：移动控制器每帧提供实际世界速度、是否着地、是否允许预测。
        /// 不是 SetFloat(Speed) 的动画混合参数！clipPlaybackRate 为曲线所在 Clip 的真实播放倍率。
        /// BlendTree 中不同子 Clip 的时长/倍率不能由四条混合 Float 唯一还原，需由调用方保证。[S01][S04][S16]
        /// </summary>
        public void SetMotionContext(Vector3 worldVelocity, bool grounded, bool allowPrediction,
            float clipPlaybackRate = 1f)
        {
            externalMotion = true;
            suppliedVelocity = worldVelocity;
            suppliedGrounded = grounded;
            suppliedPredictionAllowed = allowPrediction;
            suppliedPlaybackRate = clipPlaybackRate;
        }

        public void UseAutomaticMotionContext() { externalMotion = false; ResetTracking(); }

        /// <summary>瞬移、换控制器、重生等由调用方显式重置，禁止继续使用旧世界落脚点。[S17 + 移植防护]</summary>
        public void ResetTracking()
        {
            if (left != null) { left.ResetPrediction(); left.smoothed = false; left.rotationReady = false; left.weight = 0f; }
            if (right != null) { right.ResetPrediction(); right.smoothed = false; right.rotationReady = false; right.weight = 0f; }
            havePosition = false;
            haveActorForward = false;
            wasPredicting = wasGrounded = stepSelectionReady = stepValid = false;
            bodyOffset = bodyVelocity = landingTimer = 0f;
            PredictionActive = false;
        }

        private bool ValidateConfiguration()
        {
            if (animator == null || animator.avatar == null || !animator.isHuman || animator.runtimeAnimatorController == null)
                return Fail("需要有效 Humanoid Avatar 和 Animator Controller。");
            if (ikLayer < 0 || ikLayer >= animator.layerCount) return Fail("IK Layer 超出 Animator 层数。");
            if (groundMask.value == 0) return Fail("请设置 Ground Mask，并排除角色自身碰撞层。");
            left.stateId = Animator.StringToHash(leftState);
            right.stateId = Animator.StringToHash(rightState);
            left.timeId = Animator.StringToHash(leftDownTime);
            right.timeId = Animator.StringToHash(rightDownTime);
            if (!HasParameter(leftState, AnimatorControllerParameterType.Float) ||
                !HasParameter(rightState, AnimatorControllerParameterType.Float) ||
                !HasParameter(leftDownTime, AnimatorControllerParameterType.Float) ||
                !HasParameter(rightDownTime, AnimatorControllerParameterType.Float))
                return Fail("四个脚部参数必须全部存在且类型为 Float；它们应由动画曲线驱动。");
            hasGroundedParameter = !string.IsNullOrEmpty(groundedParameter) &&
                                   HasParameter(groundedParameter, AnimatorControllerParameterType.Bool);
            groundedId = string.IsNullOrEmpty(groundedParameter) ? 0 : Animator.StringToHash(groundedParameter);
            if (!hasGroundedParameter && characterController == null && !externalMotion)
                return Fail("找不到 Grounded Bool。请指定 CharacterController 或在启用前提供 SetMotionContext。");
            // 同一个 Animator 上多个脚 IK 写入者会污染输入。只诊断，不私自禁用用户组件。
            foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
                if (behaviour != null && behaviour != this && behaviour.enabled && behaviour.GetType().Name == "PlayerFootIK")
                    return Fail("同物体已有启用的 PlayerFootIK。请停用旧脚 IK 后再启用此组件。");
            return true;
        }

        private bool Fail(string message) { Debug.LogError("[ArticlePredictiveFootIK] " + message, this); return false; }
        private bool HasParameter(string name, AnimatorControllerParameterType type)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
                if (p.name == name && p.type == type) return true;
            return false;
        }

        private void Update()
        {
            if (callbackCount == 0 && Time.realtimeSinceStartup - startedAt > 3f)
            {
                Debug.LogWarning("预测 IK 尚未收到 OnAnimatorIK：检查对应层的 IK Pass、Animator 激活状态及 Culling。", this);
                callbackCount = -1;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != ikLayer || animator == null || left == null) return;
            callbackCount = Mathf.Max(1, callbackCount + 1);
            elapsed = animator.updateMode == AnimatorUpdateMode.UnscaledTime ? Time.unscaledDeltaTime :
                      animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;
            if (elapsed <= 0f) return;
            // [S12] 一次性快照，所有目标计算完成后才允许写入 Animator。
            CapturePose(left);
            CapturePose(right);
            Vector3 originalBody = animator.bodyPosition;
            float rootHeight = poseRoot.position.y;
            bool grounded = externalMotion ? suppliedGrounded :
                hasGroundedParameter ? animator.GetBool(groundedId) : characterController.isGrounded;
            Vector3 measured = havePosition ? (actorRoot.position - previousActorPosition) / elapsed : Vector3.zero;
            previousActorPosition = actorRoot.position;
            havePosition = true;
            HorizontalVelocity = Vector3.ProjectOnPlane(externalMotion ? suppliedVelocity : measured, Vector3.up);

            // 用 Actor 的水平 forward 变化检测转向。转向期间释放 IK，避免旧的世界落脚点
            // 继续约束角色，造成脚被“钉住”或拖拽。转向结束后会重新建立落脚参考。
            Vector3 currentActorForward = Vector3.ProjectOnPlane(actorRoot.forward, Vector3.up);
            bool validActorForward = currentActorForward.sqrMagnitude > 0.000001f;
            bool turning = false;
            if (validActorForward)
            {
                currentActorForward.Normalize();
                if (disableIKDuringTurn && haveActorForward && elapsed > 0f)
                {
                    float angularSpeed = Mathf.Abs(Vector3.SignedAngle(
                        previousActorForward, currentActorForward, Vector3.up)) / elapsed;
                    turning = angularSpeed >= turnAngularSpeedThreshold;
                }
                previousActorForward = currentActorForward;
                haveActorForward = true;
            }
            if (turning)
            {
                left.ResetPrediction();
                right.ResetPrediction();
                left.smoothed = right.smoothed = false;
                left.rotationReady = right.rotationReady = false;
                bodyOffset = bodyVelocity = 0f;
                wasPredicting = false;
                stepSelectionReady = false;
                stepValid = false;
                PredictionActive = false;
                ClearIK(left);
                ClearIK(right);
                Status = "转向中：已暂时关闭 IK";
                return;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(ikLayer);
            float rate = externalMotion ? suppliedPlaybackRate : animator.speed * state.speed * state.speedMultiplier;
            if (grounded && !wasGrounded) landingTimer = landingTraditionalSeconds;
            else landingTimer = Mathf.Max(0f, landingTimer - elapsed);
            bool allowed = externalMotion ? suppliedPredictionAllowed :
                state.loop && (string.IsNullOrEmpty(locomotionTag) || state.IsTag(locomotionTag));
            bool predicting = grounded && allowed && landingTimer <= 0f && rate > 0f &&
                HorizontalVelocity.magnitude >= minimumPredictionSpeed &&
                !(traditionalDuringTransitions && animator.IsInTransition(ikLayer));
            if (!grounded || predicting != wasPredicting)
            {
                left.ResetPrediction(); right.ResetPrediction(); stepSelectionReady = false;
            }
            wasPredicting = predicting;
            wasGrounded = grounded;
            PredictionActive = predicting;
            if (!grounded || state.IsTag(dashTag) || state.IsTag(attackTag) || state.IsTag(hitTag) || state.IsTag(landing))
            {
                // [S17] 空中清掉历史；不在空中继续拉脚到旧地面。
                bodyOffset = bodyVelocity = 0f;
                ClearIK(left); ClearIK(right);
                left.smoothed = right.smoothed = false;
                Status = "空中：已重置预测";
                return;
            }

            if (predicting)
            {
                UpdateContactAndPrediction(left, rate);
                UpdateContactAndPrediction(right, rate);
                SelectCurrentStep();
            }
            else stepValid = false;

            bool onSlope = stepValid && Mathf.Abs(stepEnd.y - stepStart.y) > slopeHeightThreshold;
            SolveFootTarget(left, predicting, onSlope, rootHeight);
            SolveFootTarget(right, predicting, onSlope, rootHeight);
            // [S13] 用简单估算的较低脚重建高度，不用跑步艺术修正后的最终脚高度反复反馈。
            float requestedOffset = 0f;
            if (reconstructBody && left.validTarget && right.validTarget)
            {
                float animationLowestOffset = Mathf.Min(left.pose.y, right.pose.y) - rootHeight;
                TargetVirtualRootHeight = Mathf.Min(left.simpleHeight, right.simpleHeight) - animationLowestOffset;
                requestedOffset = TargetVirtualRootHeight - rootHeight;
                if (stairMode && onSlope)
                    requestedOffset -= stepEnd.y > stepStart.y ? ascendingBodyDrop : descendingBodyDrop;
                // 工程限幅防止单次错误检测造成大位移；不是作者对反馈累积的最终解法。
                requestedOffset = Mathf.Clamp(requestedOffset, -maximumBodyOffset, maximumBodyOffset);
            }
            else TargetVirtualRootHeight = rootHeight;
            bodyOffset = Mathf.SmoothDamp(bodyOffset, requestedOffset, ref bodyVelocity,
                bodySmoothTime, Mathf.Infinity, elapsed);
            animator.bodyPosition = originalBody + Vector3.up * bodyOffset;
            ApplyFoot(left);
            ApplyFoot(right);
            Status = predicting ? (left.path.Valid && right.path.Valid ? "预测路径有效" : "等待落脚参考或局部检测失败：传统 IK")
                                : "传统 IK：Idle/非循环/过渡/落地缓冲";
        }

        private void CapturePose(Foot foot)
        {
            foot.pose = animator.GetIKPosition(foot.goal);
            foot.poseRotation = animator.GetIKRotation(foot.goal);
        }

        /// <summary>
        /// [S02][S03][S05] 只在 FootState 从抬起转为踩下时记录一次。
        /// 新启用/切换模式时若已经处于支撑期，等待下一次完整边沿，不能把支撑中段当落地首帧。
        /// 比较的是本帧更新前的预测点，防止用“下一周期”的预测覆盖本次落地。
        /// </summary>
        /// 用以确定最终的落脚点，检测时机：
        /// 固定点确定时检测一次，预测点快要着地时，在进行一次，比较哪个更接近误差容许选择哪个。
        private void UpdateContactAndPrediction(Foot foot, float playbackRate)
        {
            float stateValue = animator.GetFloat(foot.stateId);
            float timeValue = animator.GetFloat(foot.timeId);
            foot.contact = stateValue >= contactThreshold;
            foot.landed = foot.observed && foot.contact && !foot.previousContact;
            foot.observed = true;
            foot.previousContact = foot.contact;
            foot.downTime = timeCurveUsesClipSeconds ? timeValue / playbackRate : timeValue;
            if (float.IsNaN(stateValue) || float.IsInfinity(stateValue) ||
                float.IsNaN(foot.downTime) || float.IsInfinity(foot.downTime) ||
                foot.downTime < 0f || foot.downTime > maximumPredictionSeconds)
            {
                foot.ResetPrediction();
                return;
            }

            //B：开始踩下时，从当前动画脚位置再进行一次向下检测
            if (foot.landed)        //是否着地
            {
                foot.hasAnchor = ProbeGround(foot.pose, out Vector3 landed, out Vector3 normal);    
                foot.hasReference = foot.hasAnchor;
                if (foot.hasAnchor)     //脚固定以后
                {
                    // [S05] 水平误差小时沿用旧预测落点；仍要求当前位置有有效地面。
                    bool reuse = foot.predictionValid &&
                        Vector3.ProjectOnPlane(landed - foot.predicted, Vector3.up).magnitude <= reusePredictionDistance;
                    foot.anchor = reuse ? foot.predicted : landed;
                    foot.anchorNormal = reuse ? foot.predictedNormal : normal;
                    foot.referenceLocal = actorRoot.InverseTransformPoint(foot.pose);
                }
            }

            //A：预测下一次落脚位置，再向下检测地面
            foot.predictionValid = false;
            if (!foot.hasReference || !foot.hasAnchor) { foot.path.Clear(); return; }
            Vector3 reference = foot.referenceLocal;
            reference.x *= strideScale;
            reference.z *= strideScale;
            // [S04] 这里的速度已经是真实位移速度；不可再次乘步距倍率，否则重复修正。
            Vector3 next = actorRoot.TransformPoint(reference) + HorizontalVelocity * foot.downTime;
            if (!ProbeGround(next, out foot.predicted, out foot.predictedNormal))
            {
                foot.hasAnchor = foot.hasReference = false;
                foot.path.Clear();
                return;
            }
            foot.predictionValid = true;
            float middleLift = pathMiddleLift +
                (HorizontalVelocity.magnitude >= runSpeedThreshold ? runningPathLift : 0f);
            foot.path.Build(foot.anchor, foot.predicted, groundMask, footHeight,
                probeUp, probeDown, maximumGroundSlope, maximumPathInsertions, middleLift, uphillLateLift);
        }

        /// <summary>
        /// [S06] 当前“步伐”跨左右脚：后脚落点→前脚落点/预测点，共四种组合。
        /// 落地或前后次序改变时切换组合；选定组合的预测端点仍逐帧更新。
        /// 前后按实际移动方向判断，侧走时不能只用角色 forward。
        /// </summary>
        private void SelectCurrentStep()
        {
            Vector3 forward = HorizontalVelocity.sqrMagnitude > 0.0001f ? HorizontalVelocity.normalized : actorRoot.forward;
            bool currentRearIsLeft = Vector3.Dot(left.pose - right.pose, forward) <= 0f;
            if (!stepSelectionReady || left.landed || right.landed || rearIsLeft != currentRearIsLeft)
            {
                rearIsLeft = currentRearIsLeft;
                stepSelectionReady = true;
            }
            Foot rear = rearIsLeft ? left : right;
            Foot front = rearIsLeft ? right : left;
            stepValid = rear.hasAnchor && (front.contact ? front.hasAnchor : front.predictionValid);
            if (!stepValid) return;
            stepStart = rear.anchor;
            stepEnd = front.contact ? front.anchor : front.predicted;
        }

        /// <summary>
        /// [S09][S10][S11] 摆动：原动画 + (FootPath - BasePath)；支撑：锁到已落点。
        /// simpleHeight 专门供身体重建使用，保持作者回退后的简单算法；target 则可含跑步修饰。
        /// 未命中/路径无效时采用当前脚下的传统检测，不复用旧世界目标。[S16 + 工程防护]
        /// </summary>
        private void SolveFootTarget(Foot foot, bool predicting, bool onSlope, float rootHeight)
        {
            Vector3 desired = foot.pose;
            Vector3 normal = Vector3.up;
            bool planted = !predicting || foot.contact;
            foot.validTarget = false;
            float baseHeight = rootHeight + footHeight;
            if (predicting && foot.hasAnchor && foot.contact)
            {
                desired = foot.anchor;
                normal = foot.anchorNormal;
                foot.simpleHeight = foot.pose.y + foot.anchor.y - baseHeight;
                foot.validTarget = true;
            }
            else if (predicting && foot.path.Valid)
            {
                float height = foot.path.SampleHeight(foot.pose);
                foot.simpleHeight = foot.pose.y + height - baseHeight;
                desired.y = foot.simpleHeight;
                if (onSlope && HorizontalVelocity.magnitude >= runSpeedThreshold)
                {
                    float originalLift = Mathf.Max(0f, foot.pose.y - baseHeight);
                    desired.y -= originalLift * (1f - runningAnimationLiftOnSlope);
                }
                foot.validTarget = true;
            }
            else if (ProbeGround(foot.pose, out Vector3 ground, out normal))
            {
                // 传统 IK 适配实现：摆动时保留动画抬脚，非预测状态直接贴地。
                // 原文没有公布传统 IK 完整蓝图，此处不宣称逐节点复现。
                desired = ground;
                if (predicting && !foot.contact) desired.y += Mathf.Max(0f, foot.pose.y - baseHeight);
                foot.simpleHeight = desired.y;
                foot.validTarget = true;
            }
            if (!foot.validTarget)
            {
                foot.simpleHeight = foot.pose.y;
                foot.smoothed = false;
                foot.rotationReady = false;
                // 未命中时本帧立即释放，禁止权重淡出仍把脚拉向已失效的旧地面。
                foot.weight = 0f;
                return;
            }
            Quaternion rotation = foot.poseRotation;
            if (alignPlantedFeet && planted)
            {
                // [S15] 将动画脚底 up 对齐地面法线，并补偿脚腕到脚底的高度向量。
                // Humanoid Goal 的局部 Y 作为脚底法向；特殊 Avatar 需先校验此轴约定。
                rotation = Quaternion.FromToRotation(foot.poseRotation * Vector3.up, normal) * foot.poseRotation;
                desired += (normal - Vector3.up) * footHeight;
            }
            float speed = HorizontalVelocity.magnitude >= runSpeedThreshold ? runningHeightSpeed : walkingHeightSpeed;
            if (desired.y < foot.target.y) speed *= downwardSpeedMultiplier;
            if (!foot.smoothed) { foot.target = desired; foot.smoothed = true; }
            else if (planted) foot.target = Vector3.Lerp(foot.target, desired, Damp(plantedPositionSpeed));
            else
            {
                // 只平滑高度，XZ 跟随当帧原动画，避免把整条动画拖慢。
                float smoothedY = Mathf.Lerp(foot.target.y, desired.y, Damp(speed));
                foot.target = new Vector3(desired.x, smoothedY, desired.z);
            }
            if (!foot.rotationReady) { foot.targetRotation = rotation; foot.rotationReady = true; }
            else foot.targetRotation = Quaternion.Slerp(foot.targetRotation, rotation, Damp(rotationSpeed));
        }

        private bool ProbeGround(Vector3 reference, out Vector3 ankle, out Vector3 normal)
        {
            ankle = reference;
            normal = Vector3.up;
            if (!Physics.Raycast(reference + Vector3.up * probeUp, Vector3.down,
                out RaycastHit hit, probeUp + probeDown, groundMask, QueryTriggerInteraction.Ignore)) return false;
            if (Vector3.Angle(hit.normal, Vector3.up) > maximumGroundSlope) return false;
            ankle = hit.point + Vector3.up * footHeight;
            normal = hit.normal;
            return true;
        }

        private float Damp(float speed) { return 1f - Mathf.Exp(-speed * elapsed); }

        /// <summary>[S12] 唯一写入脚 IK 的位置。目标数据与参考输入分离，不直接写 foot Transform。</summary>
        private void ApplyFoot(Foot foot)
        {
            foot.weight = Mathf.Lerp(foot.weight, foot.validTarget ? 1f : 0f, Damp(ikBlendSpeed));
            animator.SetIKPositionWeight(foot.goal, foot.weight);
            animator.SetIKRotationWeight(foot.goal, alignPlantedFeet ? foot.weight : 0f);
            if (!foot.validTarget) return;
            animator.SetIKPosition(foot.goal, foot.target);
            animator.SetIKRotation(foot.goal, foot.targetRotation);
        }


        private void ClearIK(Foot foot)
        {
            foot.weight = 0f;
            animator.SetIKPositionWeight(foot.goal, 0f);
            animator.SetIKRotationWeight(foot.goal, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || !Application.isPlaying) return;
            DrawFoot(left); DrawFoot(right);
            if (stepValid) { Gizmos.color = Color.yellow; Gizmos.DrawLine(stepStart, stepEnd); }
        }

        private static void DrawFoot(Foot foot)
        {
            if (foot == null) return;
            if (foot.hasAnchor) { Gizmos.color = Color.green; Gizmos.DrawWireCube(foot.anchor, Vector3.one * 0.06f); }
            if (foot.predictionValid) { Gizmos.color = Color.red; Gizmos.DrawWireCube(foot.predicted, Vector3.one * 0.06f); }
            Gizmos.color = foot.path.Valid ? Color.cyan : Color.gray;
            for (int i = 1; i < foot.path.Points.Count; i++) Gizmos.DrawLine(foot.path.Points[i - 1], foot.path.Points[i]);
            if (foot.validTarget) { Gizmos.color = Color.magenta; Gizmos.DrawSphere(foot.target, 0.025f); }
        }
    }
}

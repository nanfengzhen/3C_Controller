# 文章版预测脚步 IK：Unity 移植与接入

来源：[纸橙《【UE5】关于预测IK的个人思考与小Trick》](https://zhuanlan.zhihu.com/p/1993765835039342710)，含 2026-02-28 更新。
源码注释的 `[S01]` 等编号对应 `ARTICLE_MAPPING.md`。这是依文章文字重建的 Unity 实现，不是作者原工程。

## 当前交付边界

- 目标为 Unity 2022.3.46f1c1 / Humanoid / Animator IK，单位为米，世界 Y 向上。
- `ArticlePredictiveFootIK.cs`：曲线、落点、路径采样、落地锁定、身体高度、模式切换和 IK 输出。
- `PredictedFootPath.cs`：双端迭代地形检测和水平位置采样。
- 运行需要有效 Avatar、曲线以及碰撞层配置。没有自动创建或修改场景、动画资源、Prefab。
- 原文没有发布完整源码/参数表。射线方式、容差、插值速度等具体数值属于移植选择，不是“作者原值”。
- 本版本调整 `Animator.bodyPosition`；**不移动模型 Transform / Actor / CharacterController，也不直接重定位 root 骨骼**。`TargetVirtualRootHeight` 是由公式得到的虚拟模型基准高度，通过身体高度差传入 Humanoid 求解器。这是明确的 Unity 输出适配，与 UE 直接移动 Mesh 不完全等价。
- 原文 2 月更新中的“Mesh 越升越高/越降越低”没有被作者彻底解决。本实现不把高度限幅或输入输出分离宣称为该缺陷的通用解法。

## 1. 安装和场景接线

1. 将本文件夹中的两个 `.cs` 放入 `Assets/Scripts/TestIK`，等待 Unity 编译。
2. 在与 Animator **同一个 GameObject** 上添加 `ArticlePredictiveFootIK`。首次配置前先停用该组件，完成引用和 Ground Mask 后再启用。
3. 停用同一角色上的旧 `PlayerFootIK`、FinalIK Grounder、Animation Rigging 脚约束等重叠写入者。代码会对同物体上启用的 `PlayerFootIK` 报错，其他求解器需要手动检查。
4. `Actor Root` 指实际角色移动根物体；`Pose Root` 指模型基准原点，通常为 Animator.transform，不是 Hips。
5. Animator 对应层开启 **IK Pass**；本项目读取到的 `Assets/animator/lumine.controller` Base Layer 已开启，四个 Float 参数已存在。此读取不代表当前场景一定使用它，也不代表 Clip 已有正确曲线。
6. `Ground Mask` 只选静态地形/台阶碰撞层，排除角色自身。地形须有 Collider。忽略 Trigger。
7. `Grounded Parameter` 默认项目现有 Bool `isGround`。没有此参数时，指定 CharacterController，或在组件启用前调用 `SetMotionContext`。
8. `Locomotion Tag` 默认 `Move`。它是 Animator **State Tag**，不是物体 Tag。自动模式还要求 State.loop 为 true 且有实际水平速度。
9. `Foot Height` 默认 0.08 m，源自文章的 8 cm 示例，必须按实际角色脚腕到脚底距离调整。
10. 测试时设 Animator Culling Mode 为 Always Animate；状态本身若有 Unity 内置 Foot IK，建议关闭，避免重复求解。脚本不自动改这些资源。

## 2. 四条动画曲线怎样制作

| Animator Float | 含义 |
|---|---|
| LeftFootState | 左脚支撑为 1，摆动为 0 |
| RightFootState | 右脚支撑为 1，摆动为 0 |
| LeftFootDownTime | 到左脚下一次“刚踩实”还剩多少秒 |
| RightFootDownTime | 到右脚下一次“刚踩实”还剩多少秒 |

Walk、Run、Sprint Clip 上分别制作同名曲线，通过 Animator 的动画曲线驱动 Float。**仅在 Parameters 面板声明四个 Float 不会自动产生数据。**不要让移动代码再用 SetFloat 覆盖同名曲线。

- 所有动画统一把“刚踩实”作为记录姿势。不要有的用脚越过 root、有的用脚刚踩实。
- `FootState` 曲线应清晰表示 0/1，落脚边界尽量采用阶梯切线，避免光滑切线在接触前提前超过 0.5。
- `FootDownTime` 不是归一化播放进度，也不是从上一落脚累计的时间。
- 若周期为 T，同脚触地相位为 τ，Clip 内时间为 t，则倒计时可按 `((τ - t) mod T + T) mod T` 制作。落脚前接近 0，越过事件后重新开始下一周期倒计时。事件处存在不连续跳变，不要用平滑曲线跨越整段过渡；边界帧需在 Unity 中检查。
- 脚在地面上的整段支撑期，DownTime 仍是到**下一次**同脚落地的剩余时间，不是全程填 0。
- 代码只在 State 由 0 进入 1 的边沿记录一次。首次启用如果已经在支撑中段，会等待下一次完整边沿，因此启动的一小段时间使用传统 IK 是预期行为。
- 默认曲线存 Clip 秒数：代码除以 `Animator.speed × State.speed × State.speedMultiplier` 换算现实秒数。已自行提供现实秒数时关闭 `Time Curve Uses Clip Seconds`。
- BlendTree 子动画有独立时间缩放时，这个简单倍率不一定等于每个子 Clip 的倍率，四个混合 Float 也不能恢复所有源动画信息。应统一落脚相位和时间约定，或由运动系统通过 `SetMotionContext` 提供已确定的有效倍率；这仍不等于解决任意混合的预测误差。

## 3. 每帧数据链

```text
OnAnimatorIK 指定层进入
  → 快照双脚原动画 IK Goal 和原始 bodyPosition
  → 获取实际水平速度 / 着地状态 / 动画状态 / 播放倍率
  → 空中重置；刚落地先传统 IK；循环移动才允许预测
  → FootState 落脚边沿：记录 Actor 本地参考点 + 世界固定落脚点
  → Actor.TransformPoint(参考点) + 水平速度 × 剩余现实秒数
  → 预测 XZ 向下查地，得到世界预测落脚点
  → 同脚“固定落点 → 预测落点”双向迭代检测生成 FootPath
  → 当前原动画脚的 XZ 在 FootPath 上采样高度
  → 摆动：原动画高度 + FootPath高度 − (模型基准高度 + 脚高)
  → 支撑：向固定落点插值，不追踪新 FootPath
  → 用未加跑步艺术修饰的简单脚高度估算较低脚
  → 实际较低脚高度 − 原动画较低脚相对模型原点的偏移
  → 得到身体高度差，平滑后写 bodyPosition
  → 设置独立 IK Goal；支撑脚按地面法线修正脚腕与旋转
```

`SetIKPosition` 写的是 Unity Animator 求解目标，不是对参考 foot Transform 直接赋值。所有脚原动画输入均在任何输出写入前读取。不要在 LateUpdate 从已经求解的脚骨重新取输入。

## 4. 关键函数

| 函数 | 职责 |
|---|---|
| CapturePose | 在输出之前快照当帧原动画 IK Goal，避免同一帧读写污染 |
| UpdateContactAndPrediction | 落脚边沿检测；小误差沿用旧预测点；记录本地姿势；计算下一落点 |
| SelectCurrentStep | 切换左右脚前后关系对应的四种步伐组合，给坡度/楼梯修正提供信息 |
| PredictedFootPath.Build | 双端检测、向下求新路径点、迭代；未解决碰撞时返回无效 |
| PredictedFootPath.SampleHeight | 按原动画脚水平位置取样，不用倒计时驱动匀速摆脚 |
| SolveFootTarget | 地形增量叠加、跑步抬脚缩减、落地锁定、传统 IK 回退与贴坡 |
| OnAnimatorIK 身体高度段 | 以实际较低脚和原动画较低脚分别取最小值，估算身体高度差 |
| ApplyFoot | 最后的脚目标输出入口 |
| ResetTracking | 空中/模式切换的历史清理；外部瞬移/重生时也可主动调用 |
| SetMotionContext | 对接真实移动速度、着地状态、预测许可和 Clip 倍率 |

## 5. 接入现有移动代码（可选）

自动模式按两次 Animator IK 回调之间 Actor 的真实水平位移求速度。它不会把项目 `Speed=1/4/9` 这种动画混合参数当成米/秒，也不接管 `OnAnimatorMove` 或 CharacterController.Move。

如果运动系统已有可信速度，在移动更新后、Animator IK 执行前提供：

```csharp
// 放进你自己的移动更新链；并非自动修改 PlayerMovementController 的补丁。
predictiveIK.SetMotionContext(
    worldVelocity: actualWorldVelocity,
    grounded: isGrounded,
    allowPrediction: isCyclicLocomotion,
    clipPlaybackRate: actualClipPlaybackRate);
```

此 API 一旦使用会持续采用外部状态，必须每帧更新；切回自动用 `UseAutomaticMotionContext()`。

步距修正：默认 `Stride Scale=1`。只有参考动画脚位置未被你的步距系统修正时，才用此系数缩放 Actor 局部 XZ。若输入已经经过 Stride Warping，不要二次修正。作者只提醒要考虑步距，未公开一套 Unity 步距算法。

## 6. 调参顺序

先平地走路验证绿点固定、红点提前出现、落点一致，再验证单台阶/下台阶；之后才加跑步、冲刺、斜坡和连续楼梯。

- 绿色框：已落地点；红色框：预测点；青线：有效 FootPath；灰线：未通过验证的路径；紫点：平滑后输出；黄线：当前跨左右脚的步伐。
- `Path Middle Lift`、`Uphill Late Lift`、`Running Path Lift` 默认 0，先检查基础检测再加艺术修正。
- 上下楼身体额外下降只在显式 `Stair Mode` 时应用，默认关闭。它需要你的关卡/移动逻辑确定楼梯环境；代码不虚构楼梯识别算法。
- 原文的缩短步距/降低速度/改专用上下楼动画需要在运动和动画系统中完成，本组件不擅自改运动速度。
- `Status`、`PredictionActive`、`HorizontalVelocity`、`AppliedBodyOffset` 为公开只读调试属性，可在调试器或自有监视脚本中查看。

## 7. 必须人工验证的场景

| 场景 | 需要观察的结果 |
|---|---|
| 平地 Walk/Run/Sprint | 四条曲线变化正确；绿点不每帧移动；修正接近零，保留原动画 |
| 0.2–0.3 m 单台阶 | 预测点在脚落地前抬高；路径绕过阶沿；实际落地不会追逐下一路径 |
| 左右脚分别处于高低平台 | 身体依较低脚稳定，不随左右步交替上蹿下跳 |
| 上/下斜坡 | 脚腕贴坡，留意作者指出的陡坡过度折叠问题 |
| 30/60/120 FPS | 每次落脚只记录一次；插值结果应相近，但跳过曲线接触区间的低采样率仍可能漏事件 |
| 动画倍率 0.5/1/2 | 倒计时按真实播放速度换算；步距与移动速度保持匹配 |
| Walk↔Run↔Sprint 过渡 | 默认暂用传统 IK，之后重新取得参考，不复用上个状态的旧落点 |
| Jump→Land、Idle→Move、停止 | 空中不拉地；落地缓冲后再预测；没有有效落脚参考时安全回退 |
| 地面 Mask 错误、空洞、检测预算耗尽 | 失效检测不复用旧目标，不因上一次命中而持续锁脚 |
| 开启旧 PlayerFootIK | 清楚报冲突，不让两个脚解算器互相覆盖 |

## 8. 明确保留的限制

没有实现作者尚未解决/未公布完整方法的：任意混合动画准确预测、脚水平主动选择台阶中心、脚掌/腿体积防穿墙、完整发力脚下沉、陡坡仅前脚掌接触、胸腔头部重心响应。碰撞检测只检查世界静态地形的射线，不能保证脚掌或腿部体积不穿透。

还不支持移动平台锚点、旋转重力、非 Humanoid、反向播放、镜像曲线自动互换、动画几帧内跨过完整 FootState 接触区间的补事件。需要对应系统另行设计，不能把这份文章视为现成解法。

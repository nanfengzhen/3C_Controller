# 实际验证记录

- 将当前项目原版 ArticlePredictiveFootIK.cs、PredictedFootPath.cs 与三个新增方案脚本共同编译，引用本机 Unity 2022.3.46f1c1 的 CoreModule/AnimationModule/PhysicsModule 和 netstandard 2.1。
- C# 9，warnings-as-errors，离线编译成功，无错误或警告。最后一次修改旋转输出后重新编译并运行检查。
- 4,008 个断言通过：初始无碰撞、无自由高度、零抬升预算、台阶阈值、预算边界、低顶棚间隙、非法参数、随机碰撞高度的有限查询次数和误差、随机脚旋转下脚尖目标换算还原、零偏移退化。
- 物理搜索测试使用可控碰撞谓词，未在 Unity 中执行真实 Physics.CheckSphere。向量和四元数换算调用实际编译的方案代码。
- 9 个原有 TestIK 文件（含已有 meta）SHA-256 与执行前完全一致。目标 Plan 目录尚未新增文件。
- 安装脚本通过 PowerShell 语法解析检查，未执行。它预先拒绝任何同名目标，File.Copy 使用 overwrite=false。
- 未连接 Unity Editor、未验证活动场景、未修改场景或 Prefab、未进行角色 IK/Collider 运行测试。目标目录写入被当前会话审批设置拒绝，交付暂存于本目录。

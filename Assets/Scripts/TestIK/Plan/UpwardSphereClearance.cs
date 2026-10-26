using System;

namespace ArticleFootIK.Plans
{
    /// <summary>
    /// 方案 A 的有限高度搜索；用谓词分离物理查询，便于验证搜索边界。
    /// 只返回检查过且不重叠的终点，不保证从起点到终点的移动过程连续无碰撞。
    /// 算法可能跨过很薄的空隙，因此不能称为全局最小抬升或完整碰撞求解。
    /// </summary>
    public static class UpwardSphereClearance
    {
        /// <summary>
        /// 从 0 开始向上逐级试探。找到自由高度后，在最后一段做二分细化。
        /// blockedAtHeight(h) 为 true 表示上移 h 后球仍碰撞。
        /// 预算内找不到空位时返回 false，不假装 maximumLift 一定安全。
        /// </summary>
        public static bool TryFindLift(Func<float, bool> blockedAtHeight, float maximumLift,
            int searchSteps, int refineSteps, out float lift)
        {
            if (blockedAtHeight == null) throw new ArgumentNullException(nameof(blockedAtHeight));
            if (float.IsNaN(maximumLift) || float.IsInfinity(maximumLift) || maximumLift < 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumLift));
            if (searchSteps < 1 || searchSteps > 256 || refineSteps < 0 || refineSteps > 16)
                throw new ArgumentOutOfRangeException(nameof(searchSteps));
            lift = 0f;
            if (!blockedAtHeight(0f)) return true;
            if (maximumLift == 0f) return false;
            float low = 0f;
            for (int i = 1; i <= searchSteps; i++)
            {
                float high = maximumLift * ((float)i / searchSteps);
                if (blockedAtHeight(high)) { low = high; continue; }
                // high 始终是实际测试过的自由端点；即使场景非单调也不输出已知碰撞点。
                for (int j = 0; j < refineSteps; j++)
                {
                    float middle = (low + high) * 0.5f;
                    if (blockedAtHeight(middle)) low = middle;
                    else high = middle;
                }
                lift = high;
                return true;
            }
            return false;
        }
    }
}

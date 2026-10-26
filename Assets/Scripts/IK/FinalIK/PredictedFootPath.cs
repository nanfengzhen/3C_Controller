using System.Collections.Generic;
using UnityEngine;

namespace ArticleFootIK
{
    /// <summary>
    /// [S07] 单脚的地形增量路径。端点为该脚的落脚点和预测点，均为世界空间脚腕高度。
    /// 原文只描述双向迭代检测流程；下面的 Raycast、容差和失败返回是 Unity 移植细节。
    /// 此类不生成动画，也不处理整条腿/脚掌的碰撞。
    /// </summary>
    public sealed class PredictedFootPath
    {
        public readonly List<Vector3> Points = new List<Vector3>(16);
        public bool Valid { get; private set; }

        /// <summary>
        /// 从一段的两个端点向对方检测；遇阻后向下寻找障碍表面，将新点插入并再次检测。
        /// 迭代预算耗尽仍有阻挡时返回 false，不把未验证路径冒充可通过路径。[S07]
        /// </summary>
        public bool Build(Vector3 start, Vector3 end, LayerMask mask, float footHeight,
            float searchUp, float searchDown, float maxSlope, int budget,
            float middleLift, float uphillLateLift)
        {
            Valid = false;
            Points.Clear();
            Points.Add(start);
            Points.Add(end);
            int inserted = 0;
            int segment = 0;
            while (segment < Points.Count - 1)
            {
                Vector3 a = Points[segment];
                Vector3 b = Points[segment + 1];
                Vector3 delta = b - a;
                float length = delta.magnitude;
                if (length < 0.005f) { segment++; continue; }
                Vector3 direction = delta / length;
                // 仅缩短很小的端点区间，避免射线把自身端点当成新障碍。
                const float skin = 0.002f;
                bool forwardHit = Physics.Raycast(a + direction * skin, direction,
                    out RaycastHit forward, Mathf.Max(0f, length - skin * 2f), mask,
                    QueryTriggerInteraction.Ignore);
                bool backwardHit = Physics.Raycast(b - direction * skin, -direction,
                    out RaycastHit backward, Mathf.Max(0f, length - skin * 2f), mask,
                    QueryTriggerInteraction.Ignore);
                if (!forwardHit && !backwardHit) { segment++; continue; }
                if (inserted >= budget) return false;

                // 先从当前段的前端处理最近障碍；插入后下一轮仍会双向检测子区间。
                RaycastHit obstruction = forwardHit ? forward : backward;
                Vector3 inward = Vector3.ProjectOnPlane(forwardHit ? direction : -direction, Vector3.up);
                if (inward.sqrMagnitude < 0.000001f) return false;
                inward.Normalize();
                Vector3 probe = obstruction.point + inward * 0.01f;
                float referenceY = Mathf.Max(a.y, b.y);
                probe.y = referenceY + searchUp;
                if (!Physics.Raycast(probe, Vector3.down, out RaycastHit top,
                    searchUp + searchDown, mask, QueryTriggerInteraction.Ignore)) return false;
                if (Vector3.Angle(top.normal, Vector3.up) > maxSlope) return false;

                Vector3 point = top.point + Vector3.up * (footHeight + skin);
                float t = HorizontalParameter(point, start, end);
                float ta = HorizontalParameter(a, start, end);
                float tb = HorizontalParameter(b, start, end);
                // 垂直墙、重复碰撞、越过区间等不伪造一个有效的中间点。
                if (t <= ta + 0.0001f || t >= tb - 0.0001f) return false;
                // [S08][S14] 越靠近中间额外抬高；上坡后半段可单独抬高。
                // 具体 sin 曲线和幅度是移植参数，作者没有公布数学表达式。
                float arch = Mathf.Sin(Mathf.PI * t);
                float late = end.y > start.y ? Mathf.Clamp01((t - 0.5f) * 2f) : 0f;
                point.y += middleLift * arch + uphillLateLift * late * arch;
                Points.Insert(segment + 1, point);
                inserted++;
            }
            Valid = true;
            return true;
        }

        /// <summary>
        /// [S09] 以原动画脚的水平位置取样，按路径的水平进度插值得到高度。
        /// 不是按 FootDownTime 均匀移动脚，也不是将脚完全替换成折线路径。
        /// </summary>
        public float SampleHeight(Vector3 animatedFoot)
        {
            if (!Valid || Points.Count < 2) return animatedFoot.y;
            Vector3 start = Points[0];
            Vector3 end = Points[Points.Count - 1];
            float t = HorizontalParameter(animatedFoot, start, end);
            for (int i = 1; i < Points.Count; i++)
            {
                float tb = HorizontalParameter(Points[i], start, end);
                if (t > tb && i < Points.Count - 1) continue;
                float ta = HorizontalParameter(Points[i - 1], start, end);
                float u = tb - ta > 0.00001f ? Mathf.Clamp01((t - ta) / (tb - ta)) : 0f;
                return Mathf.Lerp(Points[i - 1].y, Points[i].y, u);
            }
            return end.y;
        }

        public void Clear() { Valid = false; Points.Clear(); }

        /// <summary>沿起终点连线在 XZ 平面上的投影比例；退化区间返回 0。</summary>
        public static float HorizontalParameter(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 span = Vector3.ProjectOnPlane(end - start, Vector3.up);
            if (span.sqrMagnitude < 0.000001f) return 0f;
            return Mathf.Clamp01(Vector3.Dot(Vector3.ProjectOnPlane(point - start, Vector3.up), span)
                                 / span.sqrMagnitude);
        }
    }
}

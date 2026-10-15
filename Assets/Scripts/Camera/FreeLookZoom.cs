using UnityEngine;
using Cinemachine;
using System;

public class FreeLookZoom : MonoBehaviour
{
    [Header("缩放设置")]
    [SerializeField]public float zoomSpeed;       // 滚轮灵敏度
    [SerializeField] public float minScale;       // 最近能缩放到初始大小的多少倍 (0.5 = 一半)
    [SerializeField] public float maxScale;       // 最远能放大到初始大小的多少倍 (2.0 = 两倍)
    [SerializeField] private float smoothTime;     //运镜流畅度,数值越大，过渡越慢

    private CinemachineFreeLook freeLook;
    private float[] originalRadii;      // 用来记录三个环的初始半径
    [SerializeField]private float currentScale = 1f;    // 当前缩放倍率
    private float targetScale = 1f;     //目标缩放倍率
    private float velocity = 0f;
    

    void Start()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        //Debug.Log("获取到freeLook");
        // 1. 记录由于你在编辑器里调好的三个环的原始半径
        // 这样缩放时能保持三个环的比例关系不变
        originalRadii = new float[3];
        originalRadii[0] = freeLook.m_Orbits[0].m_Radius; // Top
        originalRadii[1] = freeLook.m_Orbits[1].m_Radius; // Middle
        originalRadii[2] = freeLook.m_Orbits[2].m_Radius; // Bottom

    }

    void Update()
    {
        // 2. 获取滚轮输入
        // 这里的负号取决于你的习惯，通常滚轮向后(负)是拉远(Radius变大)
        // 所以：scale += -(-0.1) -> scale 变大 -> 变远
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            targetScale -= scroll * zoomSpeed;

            // 【修正点 1】限制的是 targetScale 自身！不是 currentScale
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
        }

        // 【修正点 2】平滑计算要放在 if 外面！
        // 即使手指松开了，相机也要继续平滑地飞向目标值
        currentScale = Mathf.SmoothDamp(currentScale, targetScale, ref velocity, smoothTime);
        // 注意：smoothTime 通常填 0.1f ~ 0.3f 这种小浮点数即可，不需要乘 Time.deltaTime，除非你的 smoothTime 定义得很大

        // 3. 应用缩放 (也要放在 if 外面，保证一直同步)
        for (int i = 0; i < 3; i++)
        {
            freeLook.m_Orbits[i].m_Radius = originalRadii[i] * currentScale;
        }
    }
}
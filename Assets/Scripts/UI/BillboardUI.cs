using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // 缓存主摄像机的 Transform，提升性能
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    // 使用 LateUpdate 确保在摄像机移动之后再更新 UI 朝向，防止抖动
    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 让 UI 的旋转与摄像机的旋转保持一致（最平滑正对的方式）
            transform.rotation = mainCameraTransform.rotation;
        }
    }
}
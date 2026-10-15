using UnityEngine;

public class CameraCollider : MonoBehaviour
{
    [SerializeField, Header("检测层级")]
    public LayerMask layerMask;

    [SerializeField, Header("碰撞平滑时间")]
    public float colliderSmoothTime = 10f;

    [SerializeField, Header("墙壁缓冲距离(防止相机看穿墙)")]
    public float wallBuffer = 0.2f;

    private TP_CameraController rigController; // 引用父物体的控制器

    private Vector3 targetLocalPosition;
    private float currentDistance;

    private void Awake()
    {

        // 获取挂在 TP_Camera 上的核心控制器
        rigController = transform.GetComponent<TP_CameraController>();
        
    }

    private void LateUpdate()
    {
        if (rigController == null) return;
        UpdateCameraCollider();
    }

    void UpdateCameraCollider()
    {
        // 从控制器获取当前相机可退后的最大距离 (滚轮控制的距离，或者第一人称的0)
        float desiredMaxDistance = rigController.currentTargetDistance;


        //将相机往后射出一个射线，当射线打在一个墙壁上，说明相机穿墙了，那么就要把相机往前移动一个距离，这个距离就是在hit.distance的基础上，减去一个buffer
        Vector3 rayStart = rigController.tpvTarget.position;
        Vector3 rayDir = -transform.forward;

        if (Physics.Raycast(rayStart, rayDir,out RaycastHit hit,desiredMaxDistance,layerMask,QueryTriggerInteraction.Ignore))
        {
            float targetDistance = Mathf.Clamp(hit.distance - wallBuffer, 0f, desiredMaxDistance);
            rigController.currentDistance = Mathf.Lerp(rigController.currentDistance, targetDistance, Time.deltaTime*colliderSmoothTime);
        }
        else
        {
            rigController.currentDistance = Mathf.Lerp(rigController.currentDistance, desiredMaxDistance,Time.deltaTime*colliderSmoothTime);
        }

        
            
    }
}
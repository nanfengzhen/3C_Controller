using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TP_CameraController : MonoBehaviour
{
    [SerializeField, Header("相机移动速度")] private float CameraSpeed;
    [SerializeField] private Vector2 CameraMaxAngle;
    [SerializeField] private float smoothRotationSpeed;

    [Header("目标绑定 (Targets)")]
    [Tooltip("第三人称时的焦点(原来的CameraTarget)")]
    public Transform tpvTarget;

    [Header("距离与滚轮控制")]
    [SerializeField] public float currentTargetDistance = 2f;
    [SerializeField] public float minDistance = 1f;
    [SerializeField] public float maxDistance = 5f;
    [SerializeField] public float scrollSensitivity = 0.02f;
    public float currentDistance = 0f;


    private Vector3 cameraVelocity;
    private Vector2 input;
    private Vector3 cameraRotation;


    private void Awake()
    {

        // 自动容错：如果你忘了拖 tpvTarget，尝试通过标签寻找
        if (tpvTarget == null)
        {
            GameObject targetObj = GameObject.FindWithTag("CameraTarget");
            if (targetObj != null) tpvTarget = targetObj.transform;
        }
        currentDistance = currentTargetDistance;
    }

    private void Update()
    {
        CameraInput();
    }

    private void LateUpdate()
    {
        CameraRotation();
        CameraPosition();
    }



    //相机缩放功能
    void CameraInput()
    {
        input.x -= PlayerInputManager.MainInstance._look.y * CameraSpeed;
        input.y += PlayerInputManager.MainInstance._look.x * CameraSpeed;
        input.x = Mathf.Clamp(input.x, CameraMaxAngle.x, CameraMaxAngle.y);

        currentTargetDistance -= PlayerInputManager.MainInstance._scoll.y * scrollSensitivity;
        currentTargetDistance = Mathf.Clamp(currentTargetDistance, minDistance, maxDistance);
        
    }

    //第一人称下相机旋转
    void CameraRotation()
    {
        cameraRotation = Vector3.SmoothDamp(cameraRotation, new Vector3(input.x, input.y, 0f), ref cameraVelocity, smoothRotationSpeed);
        transform.eulerAngles = cameraRotation;
    }

    void CameraPosition()
    {
        transform.position = tpvTarget.position - transform.forward * currentDistance;
    }

}
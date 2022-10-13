using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCameraController : MonoBehaviour
{
    [Header("跟随的目标对象")]
    public Transform followTarget = null;

    [Header("旋转角度参数")]
    public Vector2 rotate;

    [Header("相机旋转速度")]
    public float rotateSpeed = 2;

    [Header("相机跟随移动速度")]
    public float moveSpeed = 10;

    [Header("仰角Y最大值")]
    public float maxY = 80;

    [Header("仰角Y最小值")]
    public float minY = -80;

    [Header("视口大小")]
    public float viewSize = 60;

    [Header("默认角度")]
    public float defaultAngle = -135;

    [Header("离目标对象的距离")]
    public float radius = 3;

    [Header("离目标对象的高度")]
    public float height = 1.5f;

    [Header("鼠标是否可见")]
    public bool visiable = false;
    [Header("鼠标限制范围")]
    public CursorLockMode lockMode = CursorLockMode.Confined;


    private Camera controlCamara;// 控制的相机


    void Start()
    {
        controlCamara = this.GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        float inputX = Mouse.current.delta.ReadValue().x;
        float inputY = Mouse.current.delta.ReadValue().y;
        rotate.x += inputX * rotateSpeed;
        rotate.y += inputY * rotateSpeed;

        // 让这个x值不是很大
        if (rotate.x >= 360 || rotate.x <= -360)
        {
            rotate.x = 0;
        }

        // 仰角和俯角的角度限制
        if (rotate.y < minY)
        {
            rotate.y = minY;
        }
        else if (rotate.y > maxY)
        {
            rotate.y = maxY;
        }
        controlCamara.fieldOfView = viewSize;
        Cursor.visible = visiable;
        Cursor.lockState = lockMode;
    }

    void LateUpdate()
    {
        Transform self = controlCamara.transform;
        Vector3 startPosition = self.position;// 相机开始位置
        Vector3 endPosition;// 相机最终位置

        Vector3 targetPos = followTarget.position;
        targetPos.y += height;

        //旋转y轴，左右滑动
        Vector2 v1 = CalcAbsolutePoint(rotate.x, radius);
        endPosition = targetPos + new Vector3(v1.x, 0, v1.y);

        //相机的观察点
        Vector2 v2 = CalcAbsolutePoint(rotate.x + defaultAngle, 1);
        Vector3 viewPoint = new Vector3(v2.x, 0, v2.y) + targetPos;
        //计算2点之间的距离
        float dist = Vector3.Distance(endPosition, viewPoint);
        Vector2 v3 = CalcAbsolutePoint(rotate.y, dist);
        endPosition += new Vector3(0, v3.y, 0);

        // 防相机穿墙检测
        // 定义一条射线
        RaycastHit hit;
        if (Physics.Linecast(targetPos, endPosition, out hit))
        {
            string name = hit.collider.gameObject.tag;
            if (name != "MainCamera" || name != "Player")
            {
                //如果射线碰撞的不是相机，那么就取得射线碰撞点到玩家的距离
                endPosition = hit.point - (endPosition - hit.point).normalized * 0.2f;
            }
        }
        //self.position = endPosition;
        self.position = Vector3.Lerp(startPosition, endPosition, Time.deltaTime * moveSpeed);

        Quaternion rotateQ = Quaternion.LookRotation(viewPoint - endPosition);
        self.rotation = Quaternion.Slerp(transform.rotation, rotateQ, Time.deltaTime * moveSpeed);
        //self.rotation = rotateQ;
    }

    // 用角度计算圆的XY
    public static Vector2 CalcAbsolutePoint(float angle, float dist)
    {
        // 弧度 等于 角度*(PI/180)
        float radian = -angle * (Mathf.PI / 180);
        float x = dist * Mathf.Cos(radian);
        float y = dist * Mathf.Sin(radian);
        return new Vector2(x, y);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 角色IK和运动匹配控制器
/// </summary>
[RequireComponent(typeof(Animator))]
public class ActorIKMotionControl : MonoBehaviour
{
    /*
    使用说明:
    角色使用动画状态机(Animator)

    ====================IK=============================
    移动层需要开启层的Ik
    角色移动和Idle动画使用融合树(blendTree),混合模式采用2DFreeformDirectional.-->后面称此状态为Move2d状态(角色具有前后及左右移动动画)
    建立动画参数 VelocityX和VelocityZ(float类型)代表角色在此状态的移动速度(局部坐标系的X和z轴上的移动速度)
    然后将站立动画 ,左右前后移动动画导入blendTree,并根据VectorXZ自动计算Posx和Posy的值
    对于移动动画,脚抬起的时候不需要进行IK,脚落地的时候需要进行IK匹配,可以在动画中设置2个曲线Curves,需要进行Ik的时候设置为-1,其他根据脚高度设置1到-1的值
    曲线需要设置2个,名称分别为:RightFootCurves和LeftFootCurves,故动画参数也需要设置这2个float参数

    ===========运动匹配功能======================
    注意:如果要使用mationMotion功能,需要移动动画都使用rootMotion即根骨骼移动
    建立动画参数 MathMotionSpeed(float类型),将Move2d状态的Speed的Multip参数打钩,选定此参数,即Move2D的动画播放速度与MathMotionSpeed相关
    
    */


    //=============动画参数的hashId============================================================
    private int AnimId_MathMotionSpeed = Animator.StringToHash("MathMotionSpeed");
    private int AnimId_VelocityX = Animator.StringToHash("VelocityX");
    private int AnimId_VelocityZ = Animator.StringToHash("VelocityZ");
    private int AnimId_RightFootCurves = Animator.StringToHash("RightFootCurves");
    private int AnimId_LeftFootCurves = Animator.StringToHash("LeftFootCurves");

    [Header("========使用说明请看文件内说明=======")]
    [Header("是否使用IK系统")] public bool BeIK = true;
    [Header("是否使用Body IK系统")] public bool BeUseBodyIK = true;
    [Header("是否要精准拼配运动"), SerializeField] bool BeMathMotion = true;
    [Header("Ik向下发射射线的长度"), SerializeField, Range(0, 10)] float rayDistance = 0.2f;
    [Header("交互环境的LayaMask"), SerializeField] LayerMask evnLayerMask;
    [Header("左脚的偏移"), SerializeField] Vector3 leftFootIKOffset;
    [Header("右脚的偏移"), SerializeField] Vector3 rightFootIKOffset;
    [Header("动画机_运动速度参数名_float参数"), SerializeField] string AnimP_MathMotionSpeed_Name = "MathMotionSpeed";
    [Header("动画机_X轴移动速度参数名_float参数"), SerializeField] string AnimP_VelocityX_Name = "VelocityX";
    [Header("动画机_X轴移动速度参数名_"), SerializeField] string AnimP_VelocityZ_Name = "VelocityZ";
    [Header("动画机_右脚高度 曲线及参数名_float参数"), SerializeField] string AnimP_RightFootCurves_Name = "RightFootCurves";
    [Header("动画机_左脚高度 曲线及参数名_float参数"), SerializeField] string AnimP_LeftFootCurves_Name = "LeftFootCurves";


    [Space, Header("========Debug信息=======")]
    [Header("站立的地面的法线")] public Vector3 FootGroundNormal;

    private Animator m_Anim;
    Vector3 leftFootIK, rightFootIK, leftFootIKPos, rightFootIKPos, bodyIKPos;
    float lastBodyOffsetY; Vector3 lastLeftOffsetV3; Vector3 lastRightOffsetV3;
    Quaternion letfFootIKRot, rightFootIKRot;


    private float AnimP_MathMotionSpeed
    {
        get
        {
            return this.m_Anim.GetFloat(this.AnimId_MathMotionSpeed);
        }
        set
        {
            this.m_Anim.SetFloat(this.AnimId_MathMotionSpeed, value);
        }
    }
    private Vector2 AnimP_Velocity
    {
        get
        {
            return new Vector2(this.m_Anim.GetFloat(this.AnimId_VelocityX), this.m_Anim.GetFloat(this.AnimId_VelocityZ));
        }
        set
        {
            this.m_Anim.SetFloat(this.AnimId_VelocityX, value.x);
            this.m_Anim.SetFloat(this.AnimId_VelocityZ, value.y);

        }
    }

    void Start()
    {

    }


    void Awake()
    {
        this.m_Anim = this.transform.GetComponent<Animator>();
        this.AnimId_MathMotionSpeed = Animator.StringToHash(this.AnimP_MathMotionSpeed_Name);
        this.AnimId_VelocityX = Animator.StringToHash(this.AnimP_VelocityX_Name);
        this.AnimId_VelocityZ = Animator.StringToHash(this.AnimP_VelocityZ_Name);
        this.AnimId_RightFootCurves = Animator.StringToHash(this.AnimP_RightFootCurves_Name);
        this.AnimId_LeftFootCurves = Animator.StringToHash(this.AnimP_LeftFootCurves_Name);
    }

    void OnAnimatorIK(int layerIndex)
    {
        this.leftFootIK = this.m_Anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        this.rightFootIK = this.m_Anim.GetIKPosition(AvatarIKGoal.RightFoot);
        this.letfFootIKRot = this.m_Anim.GetIKRotation(AvatarIKGoal.LeftFoot);
        this.rightFootIKRot = this.m_Anim.GetIKRotation(AvatarIKGoal.RightFoot);
        this.bodyIKPos = this.m_Anim.bodyPosition;



        if (!this.BeIK) return;
        var stateInfo = this.m_Anim.GetCurrentAnimatorStateInfo(layerIndex);
        var beCanIk = stateInfo.IsTag("CanIK");
        if (!beCanIk) return;

        this.CheckIK();
        this.m_Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        this.m_Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        this.m_Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        this.m_Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);


        bool rightBeIk = false;
        Vector3 rightFootPos = this.rightFootIK;
        if (this.rightFootIK != this.rightFootIKPos)
        {

            rightBeIk = true;
            Vector3 temp = this.transform.InverseTransformVector(this.rightFootIKOffset * this.transform.lossyScale.y) * this.transform.lossyScale.y;

            this.m_Anim.SetIKPosition(AvatarIKGoal.RightFoot, this.rightFootIKPos - temp);
            this.m_Anim.SetIKRotation(AvatarIKGoal.RightFoot, this.rightFootIKRot);

            rightFootPos = this.rightFootIKPos - temp;
        }

        bool leftBeIk = false;
        Vector3 leftFootPos = this.leftFootIK;
        if (this.leftFootIK != this.leftFootIKPos)
        {
            leftBeIk = true;
            Vector3 temp = this.transform.InverseTransformVector(this.leftFootIKOffset * this.transform.lossyScale.y) * this.transform.lossyScale.y;
            this.m_Anim.SetIKPosition(AvatarIKGoal.LeftFoot, this.leftFootIKPos - temp);
            this.m_Anim.SetIKRotation(AvatarIKGoal.LeftFoot, this.letfFootIKRot);
            leftFootPos = this.leftFootIKPos - temp;
        }


        if (this.BeUseBodyIK)
        {

            //如果双脚都满足了IK,那么裆部的坐标跟着下降较多的走
            if (leftBeIk && rightBeIk)
            {
                if (leftFootPos.y < rightFootPos.y)
                {
                    float dis = (this.leftFootIK.y - leftFootPos.y);

                    this.OffsetBody(dis);
                }
                else
                {
                    float dis = (this.rightFootIK.y - rightFootPos.y);
                    this.OffsetBody(dis);
                }
            }
            else if (leftBeIk)
            {
                float dis = (this.leftFootIK.y - leftFootPos.y);
                this.OffsetBody(dis);
            }
            else if (rightBeIk)
            {
                float dis = (this.rightFootIK.y - rightFootPos.y);
                this.OffsetBody(dis);
            }
            else
            {
                this.lastBodyOffsetY = 0;
            }
        }
    }

    private void OffsetBody(float currentDis)
    {
        currentDis = Mathf.Lerp(this.lastBodyOffsetY, currentDis, 0.1f);
        var v3 = this.m_Anim.bodyPosition;
        v3.y -= currentDis;
        this.m_Anim.bodyPosition = v3;
        this.lastBodyOffsetY = currentDis;
    }

    private void CheckIK()
    {

        bool leftOnGround = false;
        bool rightOnGround = false;

        RaycastHit hitInfo = new RaycastHit();
        float rayDistance = this.rayDistance * this.transform.lossyScale.y;
        bool needCheckLeft = this.m_Anim.GetFloat(this.AnimId_LeftFootCurves) <= -0.9f;
        if (needCheckLeft && Physics.Raycast(this.leftFootIK + new Vector3(0, rayDistance, 0), Vector3.down, out hitInfo, rayDistance * 2, this.evnLayerMask))
        {
            //this.leftFootIKPos = hitInfo.point;
            this.letfFootIKRot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * this.letfFootIKRot;
            leftOnGround = true;
            //Debug.Log(hitInfo.point);
            FootGroundNormal = hitInfo.normal;
            //Debug.DrawRay(this.leftFootIK + new Vector3(0, rayDistance, 0), Vector3.down * rayDistance * 2, Color.red, 0.1f);

            //计算插值,获得本帧IK的位置(通过插值变换得到的,直接得到的 某些时候变化太大)
            var chaV3 = hitInfo.point - this.leftFootIK;
            chaV3 = Vector3.Lerp(this.lastLeftOffsetV3, chaV3, 0.1f);
            this.leftFootIKPos = this.leftFootIK + chaV3;
            this.lastLeftOffsetV3 = chaV3;

        }
        else
        {
            this.leftFootIKPos = this.leftFootIK;
            this.lastLeftOffsetV3 = Vector3.zero;
            // Debug.DrawRay(this.leftFootIK + new Vector3(0, rayDistance, 0), Vector3.down * rayDistance * 2, Color.green, 0.1f);
        }

        RaycastHit hitInfoRight = new RaycastHit();
        bool needCheckRight = this.m_Anim.GetFloat(this.AnimId_RightFootCurves) <= -0.9f;
        if (needCheckRight && Physics.Raycast(this.rightFootIK + new Vector3(0, rayDistance, 0), Vector3.down, out hitInfoRight, rayDistance * 2, this.evnLayerMask))
        {
            //this.rightFootIKPos = hitInfoRight.point;
            this.rightFootIKRot = Quaternion.FromToRotation(Vector3.up, hitInfoRight.normal) * this.rightFootIKRot;
            rightOnGround = true;
            //Debug.Log(hitInfoRight.point);
            FootGroundNormal = hitInfoRight.normal;
            //Debug.DrawRay(this.rightFootIK + new Vector3(0, rayDistance, 0), Vector3.down * rayDistance * 2, Color.red, 0.1f);

            //计算插值,获得本帧IK的位置(通过插值变换得到的,直接得到的 某些时候变化太大)
            var chaV3 = hitInfoRight.point - this.rightFootIK;
            chaV3 = Vector3.Lerp(this.lastRightOffsetV3, chaV3, 0.04f / (this.lastRightOffsetV3 - chaV3).sqrMagnitude);

            this.rightFootIKPos = this.rightFootIK + chaV3;
            this.lastRightOffsetV3 = chaV3;

        }
        else
        {
            this.rightFootIKPos = this.rightFootIK;
            this.lastRightOffsetV3 = Vector3.zero;
            //Debug.DrawRay(this.rightFootIK + new Vector3(0, rayDistance, 0), Vector3.down * rayDistance * 2, Color.green, 0.1f);
        }
        if (needCheckRight && needCheckLeft)
        {
            if (leftOnGround && !rightOnGround)
            {
                this.rightFootIKPos = new Vector3(this.m_Anim.bodyPosition.x, this.rightFootIK.y, this.m_Anim.bodyPosition.z) + this.rightFootIKOffset;

            }
            else if (!leftOnGround && rightOnGround)
            {
                this.leftFootIKPos = new Vector3(this.m_Anim.bodyPosition.x, this.leftFootIK.y, this.m_Anim.bodyPosition.z) + this.leftFootIKOffset;

            }

        }



    }

    void OnAnimatorMove()
    {


        if (this.BeMathMotion)
        {
            this.MathMotion();
        }
        else
        {
            this.AnimP_MathMotionSpeed = 1;
        }


    }

    private void MathMotion()
    {
        Vector2 AnimP_VelocityTemp = this.AnimP_Velocity;
        Vector3 deltaPosition = this.transform.TransformVector(this.m_Anim.deltaPosition);
        deltaPosition.y = 0;
        if (AnimP_VelocityTemp.sqrMagnitude > 1f && deltaPosition.sqrMagnitude > Mathf.Pow(1 * Time.unscaledDeltaTime, 2))
        {

            float deltaTime = Time.deltaTime;
            if (this.m_Anim.updateMode == AnimatorUpdateMode.UnscaledTime)
            {
                deltaTime = Time.unscaledDeltaTime;
            }
            else if (this.m_Anim.updateMode == AnimatorUpdateMode.AnimatePhysics)
            {
                deltaTime = Time.fixedDeltaTime;
            }
            float needSpeed = AnimP_VelocityTemp.magnitude;
            float nowSpeed = deltaPosition.magnitude / deltaTime / this.transform.lossyScale.y;
            float AnimP_MathMotionSpeedTemp = this.AnimP_MathMotionSpeed;

            this.AnimP_MathMotionSpeed = Mathf.Lerp(AnimP_MathMotionSpeedTemp, needSpeed / nowSpeed, Time.unscaledDeltaTime);


        }
        else
        {
            this.AnimP_MathMotionSpeed = 1;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 角色控制器相匹配的动画控制器,主要是用来做角色动画参数来控制动画播放和状态的
/// </summary>
[RequireComponent(typeof(Animator))]
public class ActorAnimControl : MonoBehaviour
{
    //=============动画参数的hashId============================================================
    private int AnimId_OnGround = Animator.StringToHash("OnGround");
    private int AnimId_VelocityX = Animator.StringToHash("VelocityX");
    private int AnimId_VelocityZ = Animator.StringToHash("VelocityZ");
    private int AnimId_Jump = Animator.StringToHash("Jump");

    [Header("========设定动画参数名,无特殊请使用默认的=======")]
    [Header("动画机_是否在地上参数名_bool参数"), SerializeField] string AnimP_OnGround_Name = "OnGround";
    [Header("动画机_X轴移动速度参数名_float参数"), SerializeField] string AnimP_VelocityX_Name = "VelocityX";
    [Header("动画机_X轴移动速度参数名_"), SerializeField] string AnimP_VelocityZ_Name = "VelocityZ";
    [Header("动画机_跳跃参数名_trigger参数"), SerializeField] string AnimP_Jump_Name = "Jump";




    [HideInInspector] public Animator m_Anim;

    void Awake()
    {
        this.m_Anim = this.transform.GetComponent<Animator>();
        this.AnimId_OnGround = Animator.StringToHash(this.AnimP_OnGround_Name);
        this.AnimId_VelocityX = Animator.StringToHash(this.AnimP_VelocityX_Name);
        this.AnimId_VelocityZ = Animator.StringToHash(this.AnimP_VelocityZ_Name);
        this.AnimId_Jump = Animator.StringToHash(this.AnimP_Jump_Name);
    }

    //=============动画机参数============================================================

    public bool AnimP_OnGround
    {
        get
        {
            return this.m_Anim.GetBool(this.AnimId_OnGround);
        }
        set
        {
            this.m_Anim.SetBool(this.AnimId_OnGround, value);
        }

    }

    public Vector2 AnimP_Velocity
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

    public void AnimP_Trigger_Jump()
    {
        this.m_Anim.SetTrigger(this.AnimId_Jump);
    }






}

using UnityEngine;
/// <summary>
/// 角色移动控制器-具有2种移动方式,刚体的速度控制和坐标控制
/// </summary>
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Animator))]
public class ActorMoveControl : MonoBehaviour
{
    /*
    角色移动控制器-具有2种移动方式,
    刚体的速度控制和坐标控制:PlayerMoveType.Rigidbody_TransformMove 和 PlayerMoveType.Rigidbody_VelocityMove
    可以进行输入系统进行主角控制,或者去掉bePlayerControl,使用导航系统作为AI来控制(给出导航点作为目标)
    主角控制时,依赖于摄像机的视角进行移动,还可以选择主角面向摄像机前进(传统越肩视角),主角面向行走方向(非0度顶视图),主角面向指定坐标;
    以上3种主角面向的方式不影响角色的移动,角色移动依然取决于摄像机方向(w向前s向后A左D右),不管角色面对的方向

    角色移动可以选择时间更新模式,主要为AnimatorUpdateMode.UnscaledTime和非AnimatorUpdateMode.UnscaledTime
    可以修改时间系统对角色的移动速度,可以实现 其他角色全部减慢,而主角保持正常速度.即多钟角色的时间差

    设定动画参数名,需要在同物体的animator中设置相同名称的动画参数,用来控制动画的播放

    */

    /*
    新增加失控操作:
    1:纯物理失控,调用LostControlByPhysics(),用来描述被击飞的状态,动画不在操控,角色可被击飞,角色移动速度低于一定值时恢复控制
    2:击退,冲锋等失控,调用LostControlByMoveDir(),根据时间来恢复控制;
    3:强制技能,跳跃等失控,调用LostControlByCurvs(),根据时间来恢复,根据3个曲线3个曲线的倍率,以及时间来控制角色的位置,受物理限制
    
    */

    [Header("========角色移动控制器=======")]
    [Header("========使用说明请看文件内说明=======")]
    [Header("角色的摄像机"), SerializeField] public Camera actorCamera;
    [Header("角色面对方向的类型"), SerializeField] public ActorLookType M_ActorLookType = ActorLookType.LT_LookCameraForward;
    [Header("角色看向的坐标")] public Vector3 ActorLookPos;
    [Header("加速度"), Range(0, 100), SerializeField] public float acceleration = 100;
    [Header("最大速度"), Range(0, 100), SerializeField] public float maxMoveSpeed = 20;
    [Header("空中时重力倍数"), Range(0, 100), SerializeField] float graviteRate = 3;
    [Header("跳跃高度"), SerializeField, Range(0.1f, 5)] public float JumpHeight = 1;
    [Header("角色的Update模式,当选择unscale且TimeScale!=1时")]
    [Header("角色强制切换成TransformMove方式"), SerializeField] AnimatorUpdateMode actorUpdateMode = AnimatorUpdateMode.Normal;
    [Header("角色的移动模式"), SerializeField] PlayerMoveType M_PlayerMoveType = PlayerMoveType.Rigidbody_TransformMove;
    [Header("角色地面球形碰撞体的半径,用来判定是否在地面"), Range(0.1f, 3f), SerializeField] float colliderRadius = 0.35f;
    [Header("交互环境(地面)的LayerMask"), SerializeField] public LayerMask evnLayerMask;
    [Header("跳跃等行为计划悬空的时间"), SerializeField, Range(0.0f, 5f)] float floatingInThePlanTimeLength = 0.1f;

    [Space, Header("========AI设置=======+")]
    [Header("是否被玩家操控")] public bool bePlayerControl = true;
    [Header("直接用InputManager操控?否就根据InputAix字段操控"), SerializeField] bool BeInputManagerControl = true;
    [Header("接收到的玩家方向输入"), SerializeField] public Vector2 InputAix;
    [Header("如果是AI的跳跃指令_实施后自动设置false")] public bool AI_JumpCmd = false;//跳跃命令--主要是给AI用的实施后设置false
    [Header("如果是AI的移动目标坐标")] public Vector3 AI_TargetPos;

    [Space, Header("========设定动画参数名,无特殊请使用默认的=======")]
    [Header("动画机_是否在地上参数名_bool参数"), SerializeField] string AnimP_OnGround_Name = "OnGround";
    [Header("动画机_X轴移动速度参数名_float参数"), SerializeField] string AnimP_VelocityX_Name = "VelocityX";
    [Header("动画机_X轴移动速度参数名_"), SerializeField] string AnimP_VelocityZ_Name = "VelocityZ";
    [Header("动画机_跳跃参数名_trigger参数"), SerializeField] string AnimP_Jump_Name = "Jump";

    [Space, Header("========Debug信息=======")]
    [Header("是否在地面上")] public bool BeOnGround;
    [Header("是否失去控制"), SerializeField] public bool beLostControl;
    [Header("失控的移动模式"), SerializeField] LostControlType lostControlType;
    [Header("角色所处位置的法线方向"), SerializeField] Vector3 groundNormal;
    [Header("计划内的悬空_如跳跃_且此时会受到其他物体的撞击力")] public bool BeFloatingInThePlan = false;
    [Header("目标移动速度_XZ平面(局部坐标)"), SerializeField] Vector2 targetLocalVelocity;
    [Header("移动方向_XZ平面(局部坐标)"), SerializeField] Vector2 Axis;
    [Header("角色的速度(基于Update计算)"), SerializeField] Vector3 velocity_Transform;


    #region  =========================私有变量============================

    Vector3 lastPos; PlayerMoveType lastType; Collider[] evnColliders = new Collider[30]; float floatingInThePlanStartTime; float maxDepenetrationVelocity_Defult;
    Rigidbody m_rigidbody; AnimatorUpdateMode lastActorUpdateMode = AnimatorUpdateMode.Normal; float startNoGroundTime = -1; float startVelocityNoGround = 0;
    Vector2 lastAnimPVelocity; Animator m_Anim;
    int AnimId_OnGround = Animator.StringToHash("OnGround");
    int AnimId_VelocityX = Animator.StringToHash("VelocityX");
    int AnimId_VelocityZ = Animator.StringToHash("VelocityZ");
    int AnimId_Jump = Animator.StringToHash("Jump");

    #endregion

    #region  =========================私有属性============================

    float velocityRate
    {
        get
        {
            return this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime ? 1 / Time.timeScale : 1;
        }
    }

    float ActorUseDeltaTime
    {
        get
        {

            if (this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime)
            {
                return Time.unscaledDeltaTime;
            }
            else
            {
                return Time.deltaTime;
            }
        }
    }

    float ActorUseFixedDeltaTime
    {
        get
        {

            if (this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime)
            {
                return Time.fixedUnscaledDeltaTime;
            }
            else
            {
                return Time.fixedDeltaTime;
            }
        }
    }

    float ActorUseTime
    {
        get
        {
            if (this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime)
            {
                return Time.unscaledTime;
            }
            else
            {
                return Time.time;
            }
        }
    }


    void AnimP_Trigger_Jump()
    {
        this.m_Anim.SetTrigger(this.AnimId_Jump);
    }

    #endregion

    #region   =========================公共属性============================

    /// <summary>
    /// 是否受输入系统控制
    /// </summary>
    /// <value></value>
    public bool m_BeCanByInputControl
    {
        get
        {
            return this.actorCamera != null && this.bePlayerControl;
        }
    }

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

    #endregion

    #region  =======================Unity生命周期函数===========================

    void Awake()
    {
        this.m_Anim = this.transform.GetComponent<Animator>();
        this.AnimId_OnGround = Animator.StringToHash(this.AnimP_OnGround_Name);
        this.AnimId_VelocityX = Animator.StringToHash(this.AnimP_VelocityX_Name);
        this.AnimId_VelocityZ = Animator.StringToHash(this.AnimP_VelocityZ_Name);
        this.AnimId_Jump = Animator.StringToHash(this.AnimP_Jump_Name);
    }

    void Start()
    {
        //Physics.autoSyncTransforms = true;
        this.m_Anim = this.transform.GetComponent<Animator>();
        this.m_rigidbody = this.transform.GetComponent<Rigidbody>();
        this.maxDepenetrationVelocity_Defult = this.m_rigidbody.maxDepenetrationVelocity;
        this.lastPos = this.transform.position;
    }

    void Update()
    {


        this.AutoRecoverFloatingInThePlan();
        this.RotationControl();
        this.AutoChangeMoveTypeByAnimatorUpdateMode();
        this.EditorLastVelocityWhenChangerUpdateMode();
    }

    void FixedUpdate()
    {
        this.m_rigidbody.maxDepenetrationVelocity = this.maxDepenetrationVelocity_Defult / Time.timeScale;

        this.MoveControl();
        this.JumpControl();
        //因为设置了跳跃的时候强制不在地面,所以,判断是否在地面放到后面进行,这样可以保证连续跳跃的时候,一定会有一点时间是落地状态,可以让动画播放落地动画
        this.CheckOnGround();
        this.LostControlFixedUpdate();
    }

    #endregion

    #region ==========================公共函数============================================


    /// <summary>
    /// 物理失控,角色会被力撞飞,直到速度小于输入的参数才恢复控制
    /// </summary>
    /// <param name="resumeVelocity"></param>
    /// <param name="force"></param>
    public void LostControlByPhysics(float resumeVelocity, Vector3 force)
    {
        this.lostControlType = LostControlType.PhysicsLostControl;
        this.lostControlResumeVelocity = resumeVelocity;
        this.beLostControl = true;
        if (force != Vector3.zero)
        {
            this.m_rigidbody.AddForce(force);
        }
    }

    /// <summary>
    /// 移动失控,角色会被按照参数进行移动,非强制,会被阻碍,重力等力都是正常,主要用来做击退
    /// </summary>
    /// <param name="totalLostTime"></param>
    /// <param name="worldVector"></param>
    public void LostControlByMoveDir(float totalLostTime, Vector3 worldVector)
    {
        this.lostControlType = LostControlType.TransformMoveDirLostControl;
        this.lostControlMoveVector = worldVector / totalLostTime;

        this.beLostControl = true;
        this.lostControlRemainTime = totalLostTime;
        this.lostControlTotalTime = totalLostTime;

    }

    /// <summary>
    /// 根据曲线强制移动,重力失去影响,可以按照编辑的曲线进行位移,曲线为局部坐标控制,结束后恢复重力,例如跳跃重劈,Z字快速冲锋等
    /// </summary>
    /// <param name="totalLostTime"></param>
    /// <param name="curve"></param>
    public void LostControlByCurvs(float totalLostTime, AnimationCurve localMoveX, AnimationCurve localMoveY, AnimationCurve localMoveZ,
    float xRate = 1, float yRate = 1, float zRate = 1)
    {
        this.lostControlType = LostControlType.TransformCurvsLostCOntrol;
        this.beLostControl = true;
        this.lostControlRemainTime = totalLostTime;
        this.lostControlTotalTime = totalLostTime;
        this.lostControlCurveX = localMoveX;
        this.lostControlCurveY = localMoveY;
        this.lostControlCurveZ = localMoveZ;
        this.lostControlCurveRateX = xRate;
        this.lostControlCurveRateY = yRate;
        this.lostControlCurveRateZ = zRate;

    }




    #endregion

    #region  ==========================私有函数============================================


    /// <summary>
    /// 自动恢复计划浮空
    /// </summary>
    void AutoRecoverFloatingInThePlan()
    {
        if (this.BeFloatingInThePlan && this.ActorUseTime - this.floatingInThePlanStartTime >= this.floatingInThePlanTimeLength)
        {
            this.BeFloatingInThePlan = false;
        }
    }

    /// <summary>
    /// 设置计划浮空状态-计划浮空的时间内,不会被控制刚体的速度
    /// </summary>
    /// <param name="bePlan"></param>
    void SetFloatingInThePlan(bool bePlan = true)
    {
        if (bePlan)
        {
            this.BeFloatingInThePlan = true;
            this.floatingInThePlanStartTime = this.ActorUseTime;

        }
        else
        {
            this.BeFloatingInThePlan = false;
        }
    }

    /// <summary>
    /// 当角色不受时间影响的时候,角色的移动方式修改成Transform的方式,因为刚体全部用物理,无法实现慢速度下保持看起的正常速度
    /// </summary>
    void AutoChangeMoveTypeByAnimatorUpdateMode()
    {
        if (this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime && Time.timeScale != 1)
        {
            this.lastType = this.M_PlayerMoveType;
            this.M_PlayerMoveType = PlayerMoveType.Rigidbody_TransformMove;
        }
        else if (this.actorUpdateMode == AnimatorUpdateMode.Normal && Time.timeScale > 1)
        {
            this.M_PlayerMoveType = PlayerMoveType.Rigidbody_VelocityMove;
        }

    }

    /// <summary>
    /// 当运行状态中突然改变模式需要进行的一些转换操作
    /// </summary>
    void EditorLastVelocityWhenChangerUpdateMode()
    {

        //如果突然改模式
        if (this.actorUpdateMode == AnimatorUpdateMode.UnscaledTime && this.lastActorUpdateMode != this.actorUpdateMode)
        {
            //所有速度需要放大
            this.m_rigidbody.velocity *= 1 / Time.timeScale;
            this.m_Anim.updateMode = this.actorUpdateMode;
        }
        //突然改成普通模式
        else if (this.actorUpdateMode != AnimatorUpdateMode.UnscaledTime && this.lastActorUpdateMode == AnimatorUpdateMode.UnscaledTime)
        {
            //所有速度需要放大
            this.m_rigidbody.velocity /= (1 / Time.timeScale);
            this.m_Anim.updateMode = this.actorUpdateMode;

        }



        this.lastActorUpdateMode = this.actorUpdateMode;
    }

    /// <summary>
    /// 旋转角色,使用的是tranform的旋转(即时的)
    /// </summary>
    void RotationControl()
    {
        if (this.beLostControl) return;
        Vector3 lookPos = Vector3.zero;


        if (this.M_ActorLookType == ActorLookType.LT_LookCameraForward)
        {
            if (this.m_BeCanByInputControl)
            {
                var moveDir = this.BeInputManagerControl ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : this.InputAix;
                if (moveDir.sqrMagnitude <= 0.1f) return;
            }
            //  ai的话,如果没有新的移动位置,那么也不旋转
            else if (!this.m_BeCanByInputControl && BeArrivedTarget)
            {
                return;
            }
            var cameraForwardXZ = this.actorCamera.transform.forward;
            cameraForwardXZ.y = 0;
            lookPos = this.transform.position + cameraForwardXZ;
        }
        else if (this.M_ActorLookType == ActorLookType.LT_LookMoveForward)
        {
            if (this.m_BeCanByInputControl)
            {
                var moveDir = this.BeInputManagerControl ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : this.InputAix;
                if (moveDir.sqrMagnitude <= 0.01f) return;
                //角色的移动方向
                var pos = new Vector3(moveDir.x, 0, moveDir.y).normalized;
                pos = this.actorCamera.transform.TransformDirection(pos);
                pos.y = 0;
                lookPos = this.transform.position + pos.normalized;

            }
            //  ai的话,如果没有新的移动位置,那么也不旋转
            else if (!this.m_BeCanByInputControl)
            {
                if (BeArrivedTarget)
                {
                    return;
                }

                lookPos = this.AI_TargetPos;
                lookPos.y = this.transform.position.y;

            }

        }
        //this.M_ActorLookType == ActorLookType.LT_LookPos
        else
        {
            var dis = this.transform.position - this.ActorLookPos;
            dis.y = 0;
            if (dis.sqrMagnitude <= 0.1f) return;

            lookPos = this.ActorLookPos;
            lookPos.y = this.transform.position.y;
        }
        var worldDir = lookPos - this.transform.position;
        worldDir.y = 0;
        var targetQuan = Quaternion.FromToRotation(Vector3.forward, worldDir);
        var angle = Quaternion.Angle(this.transform.rotation, targetQuan);
        if (angle != 0)
        {
            //Mathf.Min(1f, 270f / angle * Time.deltaTime)
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetQuan, 0.3f);
        }


        //this.transform.LookAt(lookPos);
    }

    /// <summary>
    /// 跳跃控制
    /// </summary>
    void JumpControl()
    {
        if (this.beLostControl)
        {
            AI_JumpCmd = false;
            return;
        }
        bool jumpCmd = false;
        if (!this.m_BeCanByInputControl)
        {
            jumpCmd = AI_JumpCmd;
            AI_JumpCmd = false;
        }
        else
        {
            jumpCmd = this.BeInputManagerControl ? Input.GetButton("Jump") : AI_JumpCmd;
            AI_JumpCmd = false;
        }

        if (this.BeOnGround && jumpCmd)
        {
            this.SetFloatingInThePlan();
            this.BeOnGround = false;
            this.AnimP_OnGround = this.BeOnGround;
            this.AnimP_Trigger_Jump();
            var v = this.m_rigidbody.velocity;
            v.y = 0;
            this.m_rigidbody.velocity = v;
            if (this.M_PlayerMoveType == PlayerMoveType.Rigidbody_VelocityMove)
            {

                var speedY = Mathf.Sqrt(this.JumpHeight * -2 * Physics.gravity.y);

                this.m_rigidbody.AddForce(new Vector3(0, speedY, 0), ForceMode.VelocityChange);
                // Debug.Log("起跳时间" + Time.time);
            }
            else
            {
                //起跳初速度
                var v0 = Mathf.Sqrt(this.JumpHeight * 2 * -Physics.gravity.y / Time.timeScale);
                startVelocityNoGround = v0;
                this.startNoGroundTime = this.ActorUseTime;

            }



        }

    }

    /// <summary>
    /// 检查是否在地面上--计划浮空状态不检查
    /// </summary>
    void CheckOnGround()
    {
        if (this.BeFloatingInThePlan) return;
        bool beOnGround = false;
        RaycastHit hitInfo = new RaycastHit();
        beOnGround = Physics.Raycast(this.transform.position + new Vector3(0, (this.colliderRadius - colliderRadius / 2f) * this.transform.lossyScale.y, 0), Vector3.down, out hitInfo, this.colliderRadius, this.evnLayerMask);


        if (!beOnGround)
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(this.transform.position + new Vector3(0, (this.colliderRadius - 0.2f) * this.transform.lossyScale.y, 0), this.colliderRadius * this.transform.lossyScale.y, this.evnColliders, this.evnLayerMask);
            if (colliderCount > 0)
            {
                this.startNoGroundTime = -1;
                startVelocityNoGround = 0;
                beOnGround = true;

            }
            groundNormal = new Vector3(0, 1, 0);
        }
        else
        {
            groundNormal = hitInfo.normal;
        }
        //记录离开地面的时间
        if (this.BeOnGround && !beOnGround)
        {
            this.startNoGroundTime = this.ActorUseTime;

        }
        if (!this.BeOnGround && beOnGround)
        {
            // Debug.Log("落地时间" + Time.time);
        }

        this.BeOnGround = beOnGround;
        this.AnimP_OnGround = this.BeOnGround;
    }

    /// <summary>
    /// 根据是否AI来计算角色下一步要去的位置的行走方向
    /// </summary>
    void ComputeAxis()
    {
        if (this.m_BeCanByInputControl)
        {

            this.Axis = this.BeInputManagerControl ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : this.InputAix;
            var dir = new Vector3(this.Axis.x, 0, this.Axis.y);
            dir = this.actorCamera.transform.TransformDirection(dir);
            dir = this.transform.InverseTransformDirection(dir);
            this.Axis.x = dir.x;
            this.Axis.y = dir.z;


        }
        else
        {

            if (!this.BeArrivedTarget)
            {
                var localDir = this.transform.InverseTransformPoint(this.AI_TargetPos);
                this.Axis = new Vector2(localDir.x, localDir.z);
            }
            else
            {
                this.Axis = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// 时候达到目的地(AI_TargetPos)
    /// </summary>
    /// <value></value>
    bool BeArrivedTarget
    {
        get
        {
            var disV3 = this.transform.position - this.AI_TargetPos;
            disV3.y = 0;
            return disV3.sqrMagnitude <= 0.01f;
        }
    }

    /// <summary>
    /// 移动控制
    /// </summary>
    void MoveControl()
    {
        if (this.beLostControl) return;
        this.ComputeAxis();

        if (this.BeOnGround && Axis.sqrMagnitude > 0.01f)
        {

            this.Axis = this.Axis.normalized;
            var delta = (this.Axis * (this.acceleration * this.ActorUseFixedDeltaTime));
            this.targetLocalVelocity += delta;
            if (this.targetLocalVelocity.sqrMagnitude > maxMoveSpeed * maxMoveSpeed)
            {
                this.targetLocalVelocity = this.targetLocalVelocity.normalized * maxMoveSpeed;
            }

            this.Move(new Vector3(this.targetLocalVelocity.x, 0, this.targetLocalVelocity.y));
        }
        else if (this.BeOnGround)
        {

            if (this.targetLocalVelocity.sqrMagnitude > 0)
            {

                var speed = Mathf.Abs(this.targetLocalVelocity.magnitude);
                speed -= (acceleration * this.ActorUseDeltaTime);
                if (speed <= 0)
                {
                    this.targetLocalVelocity = Vector2.zero;
                }
                else
                {
                    this.targetLocalVelocity = this.targetLocalVelocity.normalized * (speed - (acceleration * this.ActorUseFixedDeltaTime));
                    this.Move(new Vector3(this.targetLocalVelocity.x, 0, this.targetLocalVelocity.y));
                }

            }

        }
        //在空中的处理
        else if (!this.BeOnGround)
        {
            //刚体的移动方式不需要处理,transformMove需要用Transform自行计算处理
            if (this.M_PlayerMoveType == PlayerMoveType.Rigidbody_TransformMove)
            {
                var xzVelocity = Vector3.ProjectOnPlane(this.velocity_Transform, Vector3.up);

                //起跳
                if (this.startNoGroundTime > 0)
                {
                    var t = this.ActorUseTime - this.startNoGroundTime;

                    if (t < startVelocityNoGround / -Physics.gravity.y)
                    {
                        //起跳用transform模拟的时候,如果重力影响开着,则速度加一个重力影响的速度
                        if (this.m_rigidbody.useGravity)
                        {
                            var ve = this.m_rigidbody.velocity;
                            ve.y += Physics.gravity.y * -Time.fixedDeltaTime;
                            this.m_rigidbody.velocity = ve;
                        }

                        //某一时间的瞬间速度
                        var upSpeed = Physics.gravity.y / Time.timeScale * t + startVelocityNoGround;
                        //Debug.Log(string.Format("t={0},startVelocityNoGround={1},upSpeed={2}", t, startVelocityNoGround, upSpeed));
                        //添加
                        xzVelocity.y += upSpeed;
                    }



                }

                //浮空的时候,当开始下降的时候就添加额外重力,让刚体跟快下降,提高跳跃的感觉
                if (xzVelocity.y <= 0)
                {
                    var velocityTemp = this.velocity_Transform;
                    velocityTemp.y += Physics.gravity.y * this.ActorUseFixedDeltaTime * (this.graviteRate - 1);
                    xzVelocity.y = velocityTemp.y;
                }

                //风阻--暂时关闭吧
                var needWindForce = false;
                if (needWindForce)
                {
                    var temp = xzVelocity;
                    var y = temp.y;
                    temp.y = 0;
                    if (temp.sqrMagnitude != 0)
                    {
                        temp *= (1 - ActorUseFixedDeltaTime);
                        temp.y = y;
                        xzVelocity = temp;
                    }
                }

                //Debug.Log("maxY=" + this.transform.position.y);

                this.m_rigidbody.MovePosition(this.transform.position + xzVelocity * this.ActorUseFixedDeltaTime);



                var a = this.m_rigidbody.velocity;

                a.x *= 0.2f; a.z *= 0.2f;
                a.x = Mathf.Clamp(a.x, Physics.gravity.y, -Physics.gravity.y);
                a.z = Mathf.Clamp(a.z, Physics.gravity.y, -Physics.gravity.y);
                this.m_rigidbody.velocity = a;
            }

            else
            {
                //浮空的时候,当开始下降的时候就添加额外重力,让刚体跟快下降,提高跳跃的感觉
                if (this.m_rigidbody.velocity.y <= 0)
                {
                    var velocityTemp = this.m_rigidbody.velocity;
                    velocityTemp.y += Physics.gravity.y * this.ActorUseFixedDeltaTime * (this.graviteRate - 1);
                    this.m_rigidbody.velocity = velocityTemp;
                }

                //风阻--暂时关闭吧
                var needWindForce = false;
                if (needWindForce)
                {
                    var temp = this.m_rigidbody.velocity;
                    var y = temp.y;
                    temp.y = 0;
                    if (temp.sqrMagnitude != 0)
                    {

                        temp *= (1 - ActorUseFixedDeltaTime);
                        temp.y = y;
                        this.m_rigidbody.velocity = temp;
                    }
                }


            }

        }

        this.SetAnimPVelocity();
    }

    /// <summary>
    /// 移动角色
    /// </summary>
    /// <param name="localVelocity"></param>
    void Move(Vector3 localVelocity)
    {
        if (this.M_PlayerMoveType == PlayerMoveType.Rigidbody_VelocityMove)
        {
            this.RigidbodyMove(localVelocity);
        }
        else
        {
            this.RigidbodyMoveByTransform(localVelocity);
        }

    }

    /// <summary>
    /// 设置动画移动参数
    /// </summary>
    void SetAnimPVelocity()
    {
        this.ComputeVelocity();
        var velocity = this.transform.InverseTransformVector(this.velocity_Transform);
        Vector2 v2 = Vector2.Lerp(this.lastAnimPVelocity, new Vector2(velocity.x, velocity.z), 0.1f);
        this.AnimP_Velocity = new Vector2(velocity.x, velocity.z);
        this.lastAnimPVelocity = v2;
        // var str = string.Format("ground={0} pos:{1},{2} lastPos:{3},{4} globalV:{5},{6} localV:{7},{8}", this.BeOnGround, this.transform.position.x, this.transform.position.z
        // , this.lastPos.x, this.lastPos.z, this.velocity_Transform.x, this.velocity_Transform.z, velocity.x, velocity.z);

        // Debug.Log(str);
    }

    /// <summary>
    /// 计算速度
    /// </summary>
    void ComputeVelocity()
    {
        if (this.M_PlayerMoveType == PlayerMoveType.Rigidbody_VelocityMove)
        {
            this.velocity_Transform = this.m_rigidbody.velocity;
        }
        else
        {
            this.velocity_Transform = (this.transform.position - this.lastPos) / this.ActorUseFixedDeltaTime;
        }
        this.lastPos = this.transform.position;
    }

    /// <summary>
    /// 刚体速度移动方式
    /// </summary>
    /// <param name="localVelocity"></param>
    void RigidbodyMove(Vector3 localVelocity)
    {
        localVelocity = this.transform.rotation * localVelocity;//局部坐标转到世界坐标




        var velocityTemp = Vector3.ProjectOnPlane(localVelocity, this.groundNormal);
        velocityTemp *= this.velocityRate;

        //当速度是向下的时候,添加重力让他更快下落
        if (this.m_rigidbody.velocity.y < -0.1f)
        {
            velocityTemp.y += Physics.gravity.y * this.ActorUseDeltaTime * (this.graviteRate - 1);
        }


        //Debug.Log(velocityTemp.y);
        var currentVelocity = this.m_rigidbody.velocity;
        var currentVelocityTemp = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);



        var v = currentVelocity - currentVelocityTemp + velocityTemp;

        v.y = Mathf.Min(0, v.y);//去掉往上的速度避免冲起来
        this.m_rigidbody.velocity = v;
    }

    /// <summary>
    /// 刚体坐标移动方式(更加稳定,且可以满足时间快慢缩放)
    /// </summary>
    /// <param name="localVelocity"></param>
    void RigidbodyMoveByTransform(Vector3 localVelocity)
    {


        localVelocity = this.transform.rotation * localVelocity;//局部坐标转到世界坐标

        var velocityTemp = Vector3.ProjectOnPlane(localVelocity, this.groundNormal);



        //当速度是向下的时候,添加重力让他更快下落
        if (this.velocity_Transform.y < -0.1f)
        {

            velocityTemp.y += Physics.gravity.y * this.ActorUseDeltaTime * (this.graviteRate - 1);

        }


        this.m_rigidbody.MovePosition(this.transform.position + velocityTemp * this.ActorUseFixedDeltaTime);




    }

    #endregion

    /// <summary>
    /// 角色移动模式
    /// </summary>
    public enum PlayerMoveType
    {
        Rigidbody_VelocityMove,//刚体速度移动方式
        Rigidbody_TransformMove//刚体坐标移动方式
    }

    /// <summary>
    /// 角色观察类型
    /// </summary>
    public enum ActorLookType
    {

        LT_LookCameraForward,//看着摄像机方向
        LT_LookMoveForward,//看着移动的方向
        LT_LookPos,//看着固定的坐标
    }


    public enum LostControlType
    {
        //物理失控-检查最低速度的时候恢复
        PhysicsLostControl,
        //强制移动方向,根据时间恢复
        TransformMoveDirLostControl,
        //强制按照曲线移动,根据时间恢复
        TransformCurvsLostCOntrol
    }

    #region  =================失控或强制移动=========================



    float lostControlResumeVelocity = 0;
    Vector3 lostControlMoveVector;
    float lostControlRemainTime;
    float lostControlTotalTime;
    AnimationCurve lostControlCurveX;
    AnimationCurve lostControlCurveY;
    AnimationCurve lostControlCurveZ;
    float lostControlCurveRateX;
    float lostControlCurveRateY;
    float lostControlCurveRateZ;





    private void LostControlFixedUpdate()
    {
        if (!this.beLostControl) return;
        this.lostControlRemainTime -= Time.fixedDeltaTime;
        if (this.lostControlType == LostControlType.PhysicsLostControl && this.m_rigidbody.velocity.sqrMagnitude < this.lostControlResumeVelocity)
        {
            this.m_rigidbody.useGravity = true;
            this.beLostControl = false;
        }
        else if (this.lostControlType == LostControlType.TransformMoveDirLostControl)
        {
            if (this.lostControlRemainTime > 0)
            {
                //物理移动会受到地形影响,导致方向改变和遇到障碍跳起的问题,还是射线检测修改坐标比较好;
                //1:当前坐标+2米(角色身高)向前方地面发射射线进行检测,如果检测碰撞点低于1米,高于-1米当前高度,那么就代表可以行走
                // var currentPos = this.transform.position;
                // var nextPos = this.transform.position + this.lostControlMoveVector * Time.deltaTime;
                // this.m_Rigidbody.MovePosition(nextPos);
                // Vector3 nextPosOnGround;
                // if (this.RayGroundTest(nextPos, out nextPosOnGround))
                // {
                //     this.m_Rigidbody.MovePosition(nextPosOnGround);
                // }

                //还是物理方式实现吧,不然需要手动检测太多东西
                var nextPos = this.transform.position + this.lostControlMoveVector * Time.fixedDeltaTime;

                this.PhysicsMoveToNextPosOnEndFrame(nextPos, Time.fixedDeltaTime);
            }
            else
            {
                this.m_rigidbody.useGravity = true;
                this.m_rigidbody.velocity = Vector3.zero;
                this.beLostControl = false;
            }
        }
        else if (this.lostControlType == LostControlType.TransformCurvsLostCOntrol)
        {
            if (this.lostControlRemainTime > 0)
            {
                this.m_rigidbody.useGravity = false;
                var currentTime = this.lostControlTotalTime - this.lostControlRemainTime;
                var lastTime = currentTime - Time.fixedDeltaTime;
                if (lastTime < 0)
                {
                    lastTime = 0;
                }

                var nextPos = this.GetVectorMoveFromLastFrameByCurves(lastTime, currentTime);
                this.PhysicsMoveToNextPosOnEndFrame(this.transform.position + nextPos, Time.fixedDeltaTime);
            }
            else
            {
                this.m_rigidbody.useGravity = true;
                this.m_rigidbody.velocity = Vector3.zero;
                this.beLostControl = false;
            }
        }

    }


    /// <summary>
    /// 根据曲线获得当前与上一帧之间的位移差(返回世界坐标)
    /// </summary>
    /// <returns></returns>
    private Vector3 GetVectorMoveFromLastFrameByCurves(float lastTimeRate, float nextTimeRate)
    {

        var lastX = this.lostControlCurveX.Evaluate(lastTimeRate) * this.lostControlCurveRateX;
        var lastY = this.lostControlCurveY.Evaluate(lastTimeRate) * this.lostControlCurveRateY;
        var lastZ = this.lostControlCurveZ.Evaluate(lastTimeRate) * this.lostControlCurveRateZ;

        var nextX = this.lostControlCurveX.Evaluate(nextTimeRate) * this.lostControlCurveRateX;
        var nextY = this.lostControlCurveY.Evaluate(nextTimeRate) * this.lostControlCurveRateY;
        var nextZ = this.lostControlCurveZ.Evaluate(nextTimeRate) * this.lostControlCurveRateZ;


        var pos = new Vector3(nextX - lastX, nextY - lastY, nextZ - lastZ);
        //这是局部坐标,所以需要换成世界坐标
        return this.transform.TransformVector(pos);

    }

    /// <summary>
    /// 本帧结束时 物理方式移动到下一坐标
    /// </summary>
    /// <param name="nextPos"></param>
    private void PhysicsMoveToNextPosOnEndFrame(Vector3 targetPos, float time)
    {
        if (time <= 0)
        {
            this.m_rigidbody.velocity = Vector3.zero;
            return;
        }

        var dis = targetPos - this.transform.position;
        if (dis.sqrMagnitude <= 0.0001f)
        {
            this.m_rigidbody.velocity = Vector3.zero;
            return;
        }
        this.m_rigidbody.velocity = Vector3.zero;
        this.m_rigidbody.velocity = dis / time;
    }



    #endregion
}


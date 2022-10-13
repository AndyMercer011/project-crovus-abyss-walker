using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//因为没时间看新的inputsystem，所以用了旧的。（在player的inp设置里设置了both）
public class shoot_test : MonoBehaviour
{
    #region 变量
    //射击子弹对象池

    public bullet_Generator bullet_Generator;

    //武器射速冷却

    public float cooldown;

    //ik插件tag对象

    public Transform Tag;
    public GameObject _Player;
    private Vector3 _lookTag;

    //瞄准镜头
    public Camera aimCam;
    private bool IsOpen_aimCam;
    public RectTransform littleCam;
    public Canvas canvas;
    private Vector3 _rectPos;
    private Camera rayCam;

   //散射强度
    public float range=0f;
    public float maxrange = 3f;
    public float basedistan = 30f;
    private float _time;
    private Vector3 _tag;

    //射线（建议统一到静态）
    private Ray ray = new Ray();
    private Vector3 _pos;
    private float aimDistance;
    public shootType sType=shootType.unaim;
    #endregion


    #region 流程
    void OnEnable()
    {
        rayCam = Camera.main;
    }
    void Update()
    {
        //获取全局变量
        
        //开关瞄准镜
        if (Input.GetMouseButtonDown(1))
        {
            changAimCam();
        }

        
        getRayCamHit();

        //射击
        if (Input.GetMouseButton(0) )
        {
            shooting();
        }
        //IK_tag对象,设置位置

        Tag.position = pointTO(_tag,range, Input.GetMouseButton(0));
        PlayLooKAt();
        //停止射击，散射强度归零
        //结束射击
        if (Input.GetMouseButtonUp(0))
        {
            range = 0f;
        }

    }
    #endregion




    #region 方法
    /// <summary>
    /// 调用射线碰撞，获取信息
    /// </summary>
    public void getRayCamHit()
    {
        MyGobal.GetInstance().getHitTag(rayCam);
        _pos = Input.mousePosition;
        _lookTag=_tag = MyGobal.GetInstance().HitInfo.point;
        _lookTag.y = _Player.transform.position.y;
    }
    /// <summary>
    /// 射击
    /// </summary>
    public void shooting()
    {
        //冷却判断
       if ( Time.time > _time)
        {
            _time = Time.time + cooldown;
            bullet_Generator.shoot(_tag);
            range += 0.01f;
        }
       
    }
    /// <summary>
    /// 更换枪支
    /// </summary>
    public void changeGun()
    {

    }
    /// <summary>
    /// 更换弹夹
    /// </summary>
    /// <param name="numb"></param>
    /// <returns></returns>
    public int reloadClip(int numb)
    {

        return numb;
    }

    /// <summary>
    /// IKtag赋值
    /// </summary>
    /// <param name="_tag"></param>
    /// <param name="range"></param>
    /// <param name="isran"></param>
    /// <returns></returns>
    public Vector3 pointTO(Vector3 _tag,float range,bool isran)
    {

        if (isran)
        {
           return RandomTagSet(_tag, range, sType);
        }
        return _tag;
    }

    /// <summary>
    /// 后座力的随机方式，根据需求调整
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="range"></param>
    /// <returns></returns>

    public Vector3 RandomTagSet(Vector3 tag, float range, shootType shootType)
    {
        
        if (range > maxrange) { range = maxrange; }
        if (shootType == shootType.unaim)
        {
            range += 2f;
        }
        range*=Vector3.Distance(this.transform.position, tag)/ basedistan;
        tag.x += 0.3f * Random.Range(-range, range);
        tag.y += Random.Range(-range, range);
        tag.z += 0.3f * Random.Range(-range, range);
        return tag;
    }
    public void PlayLooKAt()
    {
        _Player.transform.LookAt(_lookTag);
    }
    /// <summary>
    /// 切换瞄准镜，设置rayCam
    /// </summary>
    public void changAimCam()
    {
        IsOpen_aimCam = !littleCam.gameObject.activeSelf;
        littleCam.gameObject.SetActive(IsOpen_aimCam);
        aimCam.gameObject.SetActive(IsOpen_aimCam);
        if (IsOpen_aimCam)
        {
            rayCam = aimCam;
        }
        else
        {
            rayCam = Camera.main;
        }
        
    }
    /// <summary>
    /// 设置AimCam到鼠标位置
    /// </summary>
    public void placeAimCamUI()
    {
        if (IsOpen_aimCam)
        {
            //瞄准镜坐标换算

            RectTransformUtility.ScreenPointToWorldPointInRectangle(littleCam, _pos, Camera.main, out _rectPos);
            littleCam.position = _rectPos;

        }
    }

   public enum shootType
    {
        aim,
        unaim
    }
    #endregion
}

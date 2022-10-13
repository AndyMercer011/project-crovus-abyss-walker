using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//��Ϊûʱ�俴�µ�inputsystem���������˾ɵġ�����player��inp������������both��
public class shoot_test : MonoBehaviour
{
    #region ����
    //����ӵ������

    public bullet_Generator bullet_Generator;

    //����������ȴ

    public float cooldown;

    //ik���tag����

    public Transform Tag;
    public GameObject _Player;
    private Vector3 _lookTag;

    //��׼��ͷ
    public Camera aimCam;
    private bool IsOpen_aimCam;
    public RectTransform littleCam;
    public Canvas canvas;
    private Vector3 _rectPos;
    private Camera rayCam;

   //ɢ��ǿ��
    public float range=0f;
    public float maxrange = 3f;
    public float basedistan = 30f;
    private float _time;
    private Vector3 _tag;

    //���ߣ�����ͳһ����̬��
    private Ray ray = new Ray();
    private Vector3 _pos;
    private float aimDistance;
    public shootType sType=shootType.unaim;
    #endregion


    #region ����
    void OnEnable()
    {
        rayCam = Camera.main;
    }
    void Update()
    {
        //��ȡȫ�ֱ���
        
        //������׼��
        if (Input.GetMouseButtonDown(1))
        {
            changAimCam();
        }

        
        getRayCamHit();

        //���
        if (Input.GetMouseButton(0) )
        {
            shooting();
        }
        //IK_tag����,����λ��

        Tag.position = pointTO(_tag,range, Input.GetMouseButton(0));
        PlayLooKAt();
        //ֹͣ�����ɢ��ǿ�ȹ���
        //�������
        if (Input.GetMouseButtonUp(0))
        {
            range = 0f;
        }

    }
    #endregion




    #region ����
    /// <summary>
    /// ����������ײ����ȡ��Ϣ
    /// </summary>
    public void getRayCamHit()
    {
        MyGobal.GetInstance().getHitTag(rayCam);
        _pos = Input.mousePosition;
        _lookTag=_tag = MyGobal.GetInstance().HitInfo.point;
        _lookTag.y = _Player.transform.position.y;
    }
    /// <summary>
    /// ���
    /// </summary>
    public void shooting()
    {
        //��ȴ�ж�
       if ( Time.time > _time)
        {
            _time = Time.time + cooldown;
            bullet_Generator.shoot(_tag);
            range += 0.01f;
        }
       
    }
    /// <summary>
    /// ����ǹ֧
    /// </summary>
    public void changeGun()
    {

    }
    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="numb"></param>
    /// <returns></returns>
    public int reloadClip(int numb)
    {

        return numb;
    }

    /// <summary>
    /// IKtag��ֵ
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
    /// �������������ʽ�������������
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
    /// �л���׼��������rayCam
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
    /// ����AimCam�����λ��
    /// </summary>
    public void placeAimCamUI()
    {
        if (IsOpen_aimCam)
        {
            //��׼�����껻��

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

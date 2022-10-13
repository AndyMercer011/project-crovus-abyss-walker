using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GrenadesThrow : MonoBehaviour
{
    public GameObject Grenades;
    public Rigidbody body;
    public CapsuleCollider _collider;
    public LineRenderer line;
    public Vector3 Force;
    public float boomTime=5f;
    public Mesh _mesh;
    public Material _material;
    public float t=0.1f;
    public float power = 100;
    private Vector3 _Tag;
    private Vector3 _hitPoit;
    public float distance;
    public bool isRemovePin=true;
    public bool isThrow=false;
    public bool resetData = false;
    public bool isTrajectory = false;
    public Transform PP2;
    public float maxDistance = 50;
    private Vector3 p2;
    private Vector3 p1;
    private Vector3 TP1;
    private Vector3 TP2;
    private Vector2 tempV = new Vector2();
    private Vector2 tempH = new Vector2();

    private void Start()
    {

        Replace();
    }
    private void OnEnable()
    {
        //throwGrenades();
    }

    private void Replace()
    {
        isTrajectory = false;
        body.Sleep();
        body.useGravity = false;
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation.Set(0f,0f,0f,0f);
        body.velocity = Vector3.zero;
        
    }

    private void Update()
    {
        if (isRemovePin)
        {
            isRemovePin = false;
            isTrajectory = true;
            line.gameObject.SetActive(true);
            
            beginBoom();
        }
        if (isTrajectory)
        {

            Calculate_trajectory();

        }
        if (Mouse.current.leftButton.isPressed&& isThrow)
         {
             line.gameObject.SetActive(false);
              getInfo();
              CalculatePostion(_Tag);
             // body.transform.LookAt(p2);
              isThrow = false;
              isTrajectory = false;
              throwGrenades();
          }
        if (resetData)
        {
            resetData = false;
            reSet();
        }
        
    }

   

    private bool getInfo()
    {
        MyGobal.GetInstance().getHitTag(Camera.main);
        _Tag = MyGobal.GetInstance().HitInfo.point;
        if (_Tag.x==0f&&_Tag.z == 0f)
        {
            return false;
        }
        Debug.Log("成功获取信息 _Tag");
        return true;
    }
    private void reSet()
    {
        Debug.Log("同步数据");
    }
    private Vector3[] postions = new Vector3[10];
    public Transform tagObject;
    private void Calculate_trajectory()
    {
        if (getInfo())
        {

            CalculatePostion(_Tag);
           
            var index = 0;
            for (float i = 0f; i < 1f; i += 0.1f)
            {
                TP1 = Vector3.Lerp(p1, p2, i);
                TP2 = Vector3.Lerp(p2, _hitPoit, i);
                var p= Vector3.Lerp(TP1, TP2, i);
                postions[index] = p;
                index++;
            }
            line.SetPositions(postions);
            //tagObject.transform.position = _Tag;
            isThrow = true;

        }
    }

    

    private void CalculatePostion(Vector3 HitPoint)
    {
        if (HitPoint.x == 0f && HitPoint.y == 0f && HitPoint.z == 0f)
        {
            HitPoint = body.transform.position + new Vector3(1f, 1f, 1f);
        }
        tagObject.position = HitPoint;
        _hitPoit = tagObject.position-body.transform.position;
        tempH.x = _hitPoit.x;
        tempH.y = _hitPoit.z;
        if ((t = power * tempH.magnitude / maxDistance)>1f)
        {
            Force = Force / t;
            tempV = tempH / t;
            Force.y = (_hitPoit.y - 0.5f * Physics.gravity.y * t * t) / t;
            var a = (tempH.magnitude * Force.y - Physics.gravity.y * t * tempH.magnitude - tempV.magnitude * _hitPoit.y) / (-tempV.magnitude * Physics.gravity.y * t);
            p2 = a * Force;
            Force = Grenades.transform.InverseTransformPoint(Force);
            PP2.localPosition = p2;
        }
        else
        {
            Force = _hitPoit;
            p2 = Vector3.Lerp(p1, _hitPoit, 0.5f);
        }
       

    }
    
    public void throwGrenades()
    {
        isTrajectory = false;
        body.useGravity = true;
       // Debug.Log(body.useGravity.ToString()+Physics.gravity);
        body.WakeUp();
        body.velocity = Force;

    }
    public void beginBoom()
    {
        Debug.Log("倒计时开始");
       
        StartCoroutine(WaitBoom(Boom));
    }
    private void Boom()
    {

        isRemovePin = true;
        Replace();
        Debug.Log("BOOM BOOM BOOM");
    }
    IEnumerator WaitBoom(Action OnTimeOut)
    {
        yield return new WaitForSecondsRealtime(boomTime);
        OnTimeOut();
    }
   
}

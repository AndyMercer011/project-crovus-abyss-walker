using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//
public class MainCamControl : MonoBehaviour
{
    public Transform playerModle;
    public Transform player;
    public float AimDistance;
    public CamProperty property;
    public TagProperty tagProperty;
    private Rigidbody _body;
    private Camera _cam;
    private CamState _state;
    private RaycastHit _hitResult=new RaycastHit();
    private Ray ray=new Ray();
    private hitCillderType hitType;
    private bool isRayHit=true;
    private bool isCamMove=true;
    public bool isCamRotate=false;
    private Vector3 _mousePoint;
    private bool isLookAtPoint=true;
    private InputData _inputData;
    private UnityAction<InputData> action;
    private Vector3 _force;
    private Vector3 _moveVect;
    private bool isfirst=true;
    private Vector3 tempEuler;
    private bool isHit = false;
    // Start is called before the first frame update
    void Start()
    {
        
        action =new UnityAction<InputData>(getEvent);
        inputEvent.Instance().inputEvents.AddListener(action);
        _cam = this.GetComponent<Camera>();
        _body = playerModle.gameObject.GetComponent<Rigidbody>();
        _force = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRayHit)
        {
            isHit= getHit();
        }
        if (isLookAtPoint)
        {
            LookAtHitPoint();
        }
       
    }
    public void getEvent(InputData dat)
    {
        _inputData = dat;
        switch (_inputData.EinputType)
        {
            case EinputType.done:
                {
                    playerMove();
                    tagMove();
                    TagRotate();
                    break;
                }
            case EinputType.A:
                {
                    _force += getForce(_inputData.EinputType);
                    break;
                }
            case EinputType.D:
                {
                    _force += getForce(_inputData.EinputType);
                    break;
                }
            case EinputType.W:
                {
                    _force += getForce(_inputData.EinputType);
                    break;
                }
            case EinputType.S:
                {
                    _force += getForce(_inputData.EinputType);
                    break;
                }
            case EinputType.mouse0up:
                {

                    isCamMove = true;
                    isCamRotate = false;
                    isfirst = true;
                    isLookAtPoint = true;
                    break;
                }
            case EinputType.MoveDrag:
                {
                    isCamMove = true;
                    isCamRotate = false;
                    break;
                }
            case EinputType.ScrollWheel:
                {
                    camForward(_inputData.dv);
                    break;
                }
            case EinputType.Click:
                {
                    var hit = getClickHit(_inputData.currentPoint);
                    break;
                }
            case EinputType.DoubleClick:
                {
                    var dhit = getdoubleHit(_inputData.currentPoint);
                    break;
                }
            case EinputType.RotateCam:
                {
                    if (isfirst)
                    {
                        tempEuler = tagProperty._tag.eulerAngles;
                        isfirst = false;
                    }
                    isCamRotate = true;
                   // isCamMove = false;
                    isLookAtPoint = false;
                    _moveVect = _inputData.currentPoint - _inputData.downPoint;
                   // TagRotate();
                    break;
                }
               
        }
    }

    private void camForward(float dv)
    {
        _cam.transform.position += _cam.transform.forward * dv;
    }

    public void LookAtHitPoint()
    {
        if (!isLookAtPoint|| isHit==false)
            return;
        playerModle.LookAt(_hitResult.point);
        playerModle.eulerAngles.Set(0, playerModle.localEulerAngles.y, 0);
    }
    public bool getHit()
    {
        getMousePoint();
        ray = _cam.ScreenPointToRay(_mousePoint);
        return Physics.Raycast(ray, out _hitResult);
    }
    public RaycastHit getClickHit(Vector3 point)
    {
        var Result = new RaycastHit();
        ray = _cam.ScreenPointToRay(point);
        Physics.Raycast(ray, out Result);
        return Result;
    }
    public RaycastHit getdoubleHit(Vector3 point)
    {
        var Result = new RaycastHit();
        ray = _cam.ScreenPointToRay(point);
        Physics.Raycast(ray, out Result);
        return Result;
    }
    
    private void getMousePoint()
    {
        _mousePoint = Input.mousePosition;
    }

    public void TagRotate()
    {
        
        if (!isCamRotate)
        {
            return;
        }
        var e = tempEuler;
        e.x +=_moveVect.y * tagProperty.Mscale;
        e.y +=_moveVect.x * tagProperty.Mscale*4;
        e.x = Mathf.Max(e.x, tagProperty.MinAngle);
        e.x = Mathf.Min(e.x, tagProperty.MaxAngle);
        tagProperty._tag.eulerAngles = e;

    }
    public void playerMove()
    {
        
        if (_force.sqrMagnitude<1)
        {
            return;
        }
        _force =_force.normalized;
        _force *= property.moveSpeed;
        _body.AddForce(_force);
        _force = default;
    }
    Vector3 getForce(EinputType t)
    {
        var forward = _cam.transform.forward;
        
        switch (t)
        {
            case EinputType.W:
              return forward;
            case EinputType.S:
                return forward * -1;
            case EinputType.A:
                return _cam.transform.right * -1;
            case EinputType.D:
                return _cam.transform.right;
        }
        return default;
    }
    public void tagMove()
    {
         if (!isCamMove||isHit==false)
           return;
       // Debug.Log(Vector3.Distance(playerModle.position, _hitResult.point));
        if (Vector3.Distance(playerModle.position, _hitResult.point)>property.distanceFromPlayer)
        {
         //   Debug.Log(Vector3.Distance(playerModle.position, _hitResult.point));
            return;
        }
        tagProperty._tag.position = Vector3.Lerp(playerModle.position, _hitResult.point, tagProperty.CenterPercen);
      
    }
    public void tagReset()
    {

    }
    public void CamReset()
    {

    }
    public void ViewHight()
    {

    }

}

[Serializable]
public class TagProperty
{
    public Transform _tag;
    public float CenterPercen;
    public float MaxAngle;
    public float MinAngle;
    public float Mscale;
}
[Serializable]
public class CamState
{
    public Vector3 postion;
    public Quaternion quaternion;
    public Vector3 eulerAngle;
    public Vector3 tag_Postion;
}
[Serializable]
public class CamProperty
{
    public float moveSpeed;
    public float rotateSpeed;
    public float distanceFromPlayer;
}
public enum hitCillderType
{
    ground,
    item,
    enemy,
    UI3d
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyGobal:MonoBehaviour
{

    private MyGobal() { }
    private static MyGobal instance = null;
    public static MyGobal GetInstance()
    {
        if (instance == null)
        {
            instance = new MyGobal();
            return instance;
        }
        else
        {
            return instance;
        }
    }
    [HideInInspector]
    public Ray HitRay => hitRay;
    [HideInInspector]
    public RaycastHit HitInfo { get => hitInfo; }


    private RaycastHit hitInfo;
    private Ray hitRay;
    private Vector3 _pos;
    void Awake()
    {
        instance = this;
    }

    public void getHitTag(Camera mian)
    {
        var p = Input.mousePosition;
        _pos = new Vector3(p.x, p.y, 0f);
        hitRay = mian.ScreenPointToRay(_pos);
        if (!Physics.Raycast(hitRay, out hitInfo))
        {
            Debug.Log("Åö×²³ö´í");
        }
    }
    public Vector3 getAimTag(float distan)
    {
        _pos.z = distan;
        return Camera.main.ScreenToWorldPoint(_pos);
    }
}




using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class inputEvent : MonoBehaviour
{
    private static inputEvent instance;

    // Start is called before the first frame update
    public UnityEvent<InputData> inputEvents;
    public float doubleTime = 0.1f;
    private InputData data;
    private bool isMouse0Down;
    private float mouse0UpTime;
    private float mouse0downtime;
    private bool isClick;
    public GameObject gm;
    public Transform tr;


    private EinputType testTEMP= EinputType.done;
    public static inputEvent Instance() {
        if (instance==null)
        {
            instance = new inputEvent();
        }
        return instance;
    }
    
    void Awake()
    {
        Application.targetFrameRate = 60;
        instance = this;
        inputEvents = new UnityEvent<InputData>();
        data = new InputData();
    }

    // Update is called once per frame
    void Update()
    {
        data.currentPoint = getMousePoint();
        if (isMouse0Down)
        {
            mouse0downtime += Time.deltaTime;
        }
        if (isClick)
        {
            mouse0UpTime += Time.deltaTime;
            if (mouse0UpTime> doubleTime)
            {
                sendEvent(EinputType.Click);
                isClick = false;
            }
            
        }
        if (Input.GetMouseButtonDown(0))
        {
            // 鼠标左键首次按下，开始计时
            isMouse0Down = true;
            mouse0downtime = 0f;
            data.downPoint = getMousePoint();
        }
        if (isMouse0Down)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
            {
                sendEvent(EinputType.RotateCam);
            }
            else if (mouse0downtime > doubleTime)
            {
                if (Input.GetMouseButton(0))
                {
                    sendEvent(EinputType.MoveDrag);
                }
                else
                {
                    isMouse0Down = false;
                    mouse0UpTime = 0f;
                    sendEvent(EinputType.mouse0up);
                }
            }
        }
       
        if (Input.GetMouseButtonUp(0))
        {
            //鼠标左键弹起，判断按住时间
            isMouse0Down = false;
            mouse0UpTime = 0f;
            sendEvent(EinputType.mouse0up);
            if (mouse0downtime < doubleTime)
            {
               
                //是否已有单击，若是则为双击。
                if (isClick)
                {
                    //发送doublueClik事件
                    sendEvent(EinputType.DoubleClick);
                    isClick = false;
                }
                else
                {
                    isClick = true;
                }
                
            }
            
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            data.dv = Input.GetAxis("Mouse ScrollWheel");
            sendEvent(EinputType.ScrollWheel);
        }

        if (Input.GetKey(KeyCode.LeftControl)&&Input.GetMouseButton(2))
        {
          //  sendEvent(EinputType.RotateCam);
        }
        if (Input.GetKey(KeyCode.W))
        {
            sendEvent(EinputType.W);
        }
        if (Input.GetKey(KeyCode.S))
        {
            sendEvent(EinputType.S);
        }
        if (Input.GetKey(KeyCode.A))
        {
            sendEvent(EinputType.A);
        }
        if (Input.GetKey(KeyCode.D))
        {
            sendEvent(EinputType.D);
        }
        sendEvent(EinputType.done);
    }

    public void sendEvent(EinputType etype)
    {
        if (testTEMP!= etype&& etype!= EinputType.done)
        {
            Debug.Log("TEMP" + testTEMP.ToString()+ "event" + etype.ToString());
           
            testTEMP = etype;
        }
        
        data.EinputType = etype;
        inputEvents.Invoke(data);
    }
    private Vector3 getMousePoint()
    {
        return Input.mousePosition;
    }

}
[Serializable]

public class InputData
{
    public Vector3 downPoint=default;
    public Vector3 currentPoint = default;
    public float dv;
    public EinputType EinputType;
}
public enum EinputType{
    Click=0,
    DoubleClick=1,
    RotateCam = 2,
    MoveDrag = 3,
    ScrollWheel = 4,
    W =5,
    S=6,
    A=7,
    D=8,
    mouse0up = 9,
    done = 10
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//鼠标右键按住，wsadqe 按键移动相机。。。和unity编辑模式的控制一致。
//input模式使用旧版，可以在playersetting里面改成both，或者修改此脚本和inputsystem一致。
public class CameraControl_45 : MonoBehaviour
{
    private GameObject _mainCam;
   
    public float maxSpeed = 50f;
    public bool isStartFreedom=false;
    private Vector3 tempMousePoint = new Vector3();
    private Vector3 tempEuler = new Vector3();
    private float moveSpeed = 0f;
    public Texture2D TEX;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam =gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
        var dt = Time.deltaTime;
       
      
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.SetCursor(null, new Vector2(1, 1), CursorMode.Auto);
            isStartFreedom = false;
        }
            if (Input.GetMouseButton(1))
        {
            if (!isStartFreedom)
            {
                tempMousePoint = Input.mousePosition;
                tempEuler = _mainCam.transform.eulerAngles;
                moveSpeed = 0f;
                Cursor.SetCursor(TEX,new Vector2(0, 0), CursorMode.Auto);
                isStartFreedom = true;
            }
            var v = Input.mousePosition - tempMousePoint;
            v = new Vector3(tempEuler.x-v.y*0.2f, tempEuler.y + v.x * 0.2f, tempEuler.z);
            _mainCam.transform.eulerAngles =v;
            moveSpeed += 0.1f;
            moveSpeed = Mathf.Min(moveSpeed, maxSpeed);

            if (Input.GetKey(KeyCode.W))
            {
                _mainCam.transform.position += _mainCam.transform.forward * dt * moveSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                _mainCam.transform.position -= _mainCam.transform.forward * dt * moveSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                _mainCam.transform.position -= _mainCam.transform.right * dt * moveSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _mainCam.transform.position += _mainCam.transform.right * dt * moveSpeed;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                _mainCam.transform.position += _mainCam.transform.up * dt * moveSpeed;
            }
            if (Input.GetKey(KeyCode.E))
            {
                _mainCam.transform.position -= _mainCam.transform.up * dt * moveSpeed;
            }

        }
        
    }
    
   
}

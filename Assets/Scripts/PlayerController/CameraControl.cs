using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("跟随的目标")] public Transform target;

    [Header("跟随的序列帧方式")]
    public UpdateType m_UpdateType = UpdateType.UT_Update;

    [Header("与目标的距离"), SerializeField, Range(0, 50)] float targetDistance = 10;

    [Header("视线中心点偏移"), SerializeField] Vector3 viewCenterOffset = new Vector3(0, 2, 0);

    float _lookAngelX = 45; float _lookAngelY = 0; Vector3 _offsetWorld; Vector3 _mouseLastPos; bool _BeRecordLastPos = false;


    void Start()
    {

        this.SetMyPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            if (!_BeRecordLastPos)
            {
                this._mouseLastPos = Input.mousePosition;
                _BeRecordLastPos = true;
                return;
            }

            var mouseDetal = Input.mousePosition - this._mouseLastPos;
            this._lookAngelX -= mouseDetal.y;
            this._lookAngelX = Mathf.Clamp(this._lookAngelX, -88, 88);
            this._lookAngelY += mouseDetal.x;

            this._mouseLastPos = Input.mousePosition;
        }
        else
        {
            _BeRecordLastPos = false;
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            this.targetDistance -= Input.mouseScrollDelta.y;

            this.targetDistance = Mathf.Clamp(this.targetDistance, 0.1f, 50f);

        }

        if (m_UpdateType == UpdateType.UT_Update)
        {
            this.SetMyPos();
        }

    }

    void LateUpdate()
    {
        if (m_UpdateType == UpdateType.UT_LateUpdate)
        {
            this.SetMyPos();
        }
    }
    void FixedUpdate()
    {
        if (m_UpdateType == UpdateType.UT_FiexedUpdate)
        {
            this.SetMyPos();
        }
    }

    private void SetMyPos()
    {
        //视线中心
        var viewCenter = target.position + this.target.rotation * viewCenterOffset;

        var targetForwardXZ = this.target.forward;

        targetForwardXZ.y = 0;
        var targetRoXZ = Quaternion.LookRotation(targetForwardXZ, Vector3.up);


        Quaternion ro = Quaternion.Euler(this._lookAngelX, _lookAngelY, 0);
        var dir = ((ro * Vector3.back));


        _offsetWorld = viewCenter + dir.normalized * targetDistance;

        this.transform.position = _offsetWorld;


        this.transform.LookAt(viewCenter);

    }

    public enum UpdateType
    {
        UT_LateUpdate,
        UT_Update,
        UT_FiexedUpdate,
    }
}

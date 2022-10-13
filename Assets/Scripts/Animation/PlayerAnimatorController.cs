using UnityEngine;

namespace Animation
{
    public class PlayerController
    {
        /// <summary>
        /// 我们通过这个类暴露玩家目前的状态
        /// </summary>
        public class InfoAccessor
        {
            private PlayerController _controllerRef;

            public InfoAccessor(PlayerController controllerRef)
            {
                this._controllerRef = controllerRef;
            }

            public (float, float) Speed()
            {
                return (_controllerRef._x_speed, _controllerRef._y_speed);
            }

        }

        [HideInInspector]
        private Animator _animatorController;

        [Header("x轴移动速度")]
        private float _x_speed;

        [Header("y轴移动速度")]
        private float _y_speed;

        [Header("上一次更新坐标的时间")]
        private float _lastUpdateTime;

        [Header("上一次记录的世界坐标")]
        private Vector3 _lastPosition;


#if DEBUG 
        private bool _isInit = false;
#endif

        private InfoAccessor accessor;

        public PlayerController(Animator animatorController)
        {
            this._animatorController = animatorController;
        }

        public void InitAll()
        {
            // Find all the SMBs on the animator that inherit from CustomSMB.
            accessor = new InfoAccessor(this);
            Player.IBaseSMB[] allSMBs = _animatorController.GetBehaviours<Player.IBaseSMB>();
            for (int i = 0; i < allSMBs.Length; i++)
            {
                allSMBs[i].Init(accessor);
            }

            // init the update time
            _lastUpdateTime = Time.time;
        }

        public void UpdateLocation(Vector3 location)
        {
#if DEBUG
           Debug.Assert(!_isInit, "PlayerController is not init");
#endif
            // 通过上一次更新的时间和坐标计算玩家的x轴和y轴移动速度
            float deltaTime = Time.time - _lastUpdateTime;
            _x_speed = (location.x - _lastPosition.x) / deltaTime;
            _y_speed = (location.y - _lastPosition.y) / deltaTime;

            // 更新上一次更新的时间和坐标
            _lastPosition = location;
            _lastUpdateTime = Time.time;
        }
    }
}

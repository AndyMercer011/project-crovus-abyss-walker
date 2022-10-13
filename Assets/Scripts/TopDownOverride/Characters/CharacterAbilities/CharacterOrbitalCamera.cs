using Cinemachine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 玩家环绕摄像机
    /// </summary>
    public class CharacterOrbitalCamera : CharacterAbility
    {
        /// This method is only used to display a helpbox text at the beginning of the ability's inspector
        public override string HelpBoxText() { return "旋转缩放摄像机"; }

        /// <summary>
        /// 水平移动速度
        /// </summary>
        [Header("水平移动速度")]
        [SerializeField] private float OrbitalXSpeed;

        /// <summary>
        /// 垂直移动最大角度
        /// </summary>
        [Range(0, 80)]
        [Header("垂直移动最大角度")]
        [SerializeField] private float OrbitalVerticalMaxAngle;

        /// <summary>
        /// 垂直移动最小角度
        /// </summary>
        [Range(0, 80)]
        [Header("垂直移动最小角度")]
        [SerializeField] private float OrbitalVerticalMinAngle;

        /// <summary>
        /// 垂直移动速度
        /// </summary>
        [Header("垂直移动速度")]
        [SerializeField] private float OrbitalVerticalSpeed;

        /// <summary>
        /// 最大相机距离
        /// </summary>
        [Header("最大相机距离")]
        [SerializeField] private float CameraMaxDistance;

        /// <summary>
        /// 最小相机距离
        /// </summary>
        [Header("最小相机距离")]
        [SerializeField] private float CameraMinDistance;

        /// <summary>
        /// 相机距离速度
        /// </summary>
        [Header("相机距离速度")]
        [SerializeField] private float CameraDistanceSpeed;

        /// <summary>
        /// 摄像机缩放平滑度
        /// </summary>
        [Header("摄像机缩放平滑度")]
        [SerializeField] private float smooth;

        /// <summary>
        /// 初始相机距离
        /// </summary>
        [Header("初始相机距离")]
        [SerializeField] private float initialCameraDistance;

        /// <summary>
        /// 初始相机垂直角度
        /// </summary>
        [Range(0, 80)]
        [Header("初始相机垂直角度")]
        [SerializeField] private float initialCameraVerticalAngle;

        private Vector3 targetOffset;
        private float targetVerticalAngle;
        private float targetDistance;

        [Header("Input Manager")]
        /// if this is false, this ability won't read input
        [Tooltip("if this is false, this ability won't read input")]
        public bool InputAuthorized = true;
        /// whether or not this ability should make changes on the InputManager to set it in camera driven input mode
        [Tooltip("whether or not this ability should make changes on the InputManager to set it in camera driven input mode")]
        public bool AutoSetupInputManager = true;

        protected Camera _mainCamera;
        protected CinemachineBrain _brain;
        protected CinemachineVirtualCamera _virtualCamera;
        protected CinemachineOrbitalTransposer _cinemachineOrbitalTransposer;

        /// <summary>
        /// On init we grab our camera and setup our input manager if needed
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            GetCurrentCamera();

            targetDistance = initialCameraDistance;
            targetVerticalAngle = initialCameraVerticalAngle;

            if (AutoSetupInputManager)
            {
                _inputManager.RotateInputBasedOnCameraDirection = true;
                bool camera3D = (_character.CharacterDimension == Character.CharacterDimensions.Type3D);
                _inputManager.SetCamera(_mainCamera, camera3D);
            }
        }

        /// <summary>
        /// Stores the current camera
        /// </summary>
        protected virtual void GetCurrentCamera()
        {
            _mainCamera = Camera.main;
            _brain = _mainCamera.GetComponent<CinemachineBrain>();
            if (_brain != null)
            {
                _virtualCamera = _brain.ActiveVirtualCamera as CinemachineVirtualCamera;
                _cinemachineOrbitalTransposer = _virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            }
        }

        /// <summary>
        /// Every frame we rotate the camera
        /// </summary>
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (!AbilityAuthorized)
            {
                return;
            }
            if (_virtualCamera != null && _cinemachineOrbitalTransposer != null
                )
            {
                Vector2 look = default;
                float cameraScale = 0;
                if (InputAuthorized && InputManagerOverride.InstanceOverride.CameraInputActiveButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
                {
                    look = InputManagerOverride.InstanceOverride.CameraMoveInput;
                    cameraScale = InputManagerOverride.InstanceOverride.CameraScaleInput;
                }

                OrbitalCameraMove(_cinemachineOrbitalTransposer, look, cameraScale, OrbitalXSpeed, OrbitalVerticalSpeed,
                   OrbitalVerticalMaxAngle, OrbitalVerticalMinAngle,
                   CameraDistanceSpeed, CameraMaxDistance,
                   CameraMinDistance, ref targetOffset, ref targetVerticalAngle, ref targetDistance, smooth);
                _mainCamera.transform.eulerAngles = new Vector3(_mainCamera.transform.eulerAngles.x,
                    _cinemachineOrbitalTransposer.m_XAxis.Value, _mainCamera.transform.eulerAngles.z);
            }
        }


        private static void OrbitalCameraMove(CinemachineOrbitalTransposer cinemachineTransposer, Vector2 look, float cameraScale,
            float Xspeed, float verticalSpeed, float verticalMaxAngle, float verticalMinAngle, float distanceSpeed,
            float distanceMax, float distanceMin,
            ref Vector3 targetOffset, ref float targetVerticalAngle, ref float targetDistance, float smooth)
        {
            cinemachineTransposer.m_XAxis.m_InputAxisValue = look.x * Xspeed;

            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle + look.y * verticalSpeed, verticalMinAngle, verticalMaxAngle);
            targetDistance = Mathf.Clamp(targetDistance + cameraScale * distanceSpeed, distanceMin, distanceMax);

            targetOffset.y = Mathf.Sin(targetVerticalAngle * Mathf.Deg2Rad) * targetDistance;
            targetOffset.z = -Mathf.Cos(targetVerticalAngle * Mathf.Deg2Rad) * targetDistance;

            cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetOffset, smooth);
        }
    }
}

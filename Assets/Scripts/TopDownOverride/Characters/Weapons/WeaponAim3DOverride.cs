using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class WeaponAim3DOverride : WeaponAim3D
    {
        /// <summary>
        /// 是否使用武器准星
        /// </summary>
        [Tooltip("是否使用武器准星")]
        public bool UseWeaponAimReticle;
        /// <summary>
        /// 武器准星是否在使用
        /// </summary>
        [Tooltip("武器准星是否在使用")]
        [MMReadOnly]
        public bool _weaponAimReticleUsing;

        /// <summary>
        /// 武器准星预制体
        /// </summary>
        [Tooltip("武器准星预制体")]
        public GameObject WeaponAimReticlePrefab;

        /// <summary>
        /// 瞄准开启武器准星
        /// </summary>
        [Tooltip("瞄准开启武器准星")]
        public bool AimEnableAimReticle = true;

        protected Vector2 _currentWeaponRecoilAngle;
        protected Vector2 _weapomRecoilRevertSpeed;
        protected GameObject _weaponAimReticle;
        protected bool _weaponAimReticleEnable;
        protected float _lastReticleTime;
        protected float _weapomRecoilRevertTime;
        protected Vector3 _reticleUIPosition;
        protected Vector3 _weaponAimReticlePoint;

        /// <summary>
        /// 当前武器后坐力角度
        /// </summary>
        public Vector2 CurrentWeaponRecoilAngle { get => _currentWeaponRecoilAngle; }
        /// <summary>
        /// 当前武器回复角速度
        /// </summary>
        public Vector2 CurrentweapomRecoilRevertSpeed { get => _weapomRecoilRevertSpeed; }

        public WeaponOverride WeaponOverride { get; protected set; }


        protected override void Initialization()
        {
            if (_initialized)
            {
                return;
            }
            base.Initialization();
            MousePositionAction.Disable();
            WeaponOverride = _weapon as WeaponOverride;
            if (UseWeaponAimReticle && _weaponAimReticle == null)
            {
                _weaponAimReticle = Instantiate(WeaponAimReticlePrefab, transform);
                DisableWeaponAimReticle();
            }
        }

        /// <summary>
        /// Every frame, we compute the aim direction and rotate the weapon accordingly
        /// </summary>
        protected override void Update()
        {
            HideMousePointer();
            HideReticle();
            if (GameManager.HasInstance && GameManager.Instance.Paused)
            {
                return;
            }
            MoveWeaponAimReticle(_weaponAimReticlePoint);
        }

        /// <summary>
        /// At fixed update we move the target and reticle
        /// </summary>
        protected override void FixedUpdate()
        {
            if (GameManager.Instance.Paused)
            {
                return;
            }
            UpdatePlane();

            GetCurrentAim();
            MoveReticle();
            DetermineWeaponRotation();
            GetWeaponAimReticlePoint();

            MoveTarget();
            RotateTarget();
        }

        /// <summary>
        /// Hides or show the mouse pointer based on the settings
        /// </summary>
        protected override void HideMousePointer()
        {
            if (AimControl != AimControls.Mouse)
            {
                return;
            }
            if (GameManager.Instance.Paused || GUIManagerOverride.InstanceOverride.EnableInfoPanel)
            {
                Cursor.visible = true;
                return;
            }
            if (ReplaceMousePointer)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
            }
        }

        public override void GetMouseAim()
        {
#if !ENABLE_INPUT_SYSTEM
			_mousePosition = Input.mousePosition;
#endif
            if (!WeaponOverride.Aiming)
            {
                GetAimPointAtPlane();
            }
            else
            {
                Ray ray = _mainCamera.ScreenPointToRay(InputManagerOverride.InstanceOverride.MousePosition);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, float.PositiveInfinity, ReticleObstacleMask.value))
                {
                    _direction = raycastHit.point;
                    Debug.DrawLine(ray.origin, _direction, Color.green);
                }
                else
                {
                    GetAimPointAtPlane();
                }

            }

            _reticlePosition = _direction;

            if (Vector3.Distance(_direction, transform.position) < MouseDeadZoneRadius)
            {
                _direction = _lastMousePosition;
            }
            else
            {
                _lastMousePosition = _direction;
            }

            _currentAim = _direction - _weapon.Owner.transform.position;
            _weaponAimCurrentAim = _direction - this.transform.position;
        }


        protected void GetAimPointAtPlane()
        {
            Ray ray = _mainCamera.ScreenPointToRay(InputManagerOverride.InstanceOverride.MousePosition);
            float distance;
            if (_playerPlane.Raycast(ray, out distance))
            {
                Vector3 target = ray.GetPoint(distance);
                _direction = target;
                Debug.DrawRay(ray.origin, _direction, Color.green);
            }
        }

        /// <summary>
        /// Determines the weapon rotation based on the current aim direction
        /// </summary>
        protected override void DetermineWeaponRotation()
        {
            if (GameManager.Instance.Paused)
            {
                return;
            }
            AimAt(this.transform.position + _weaponAimCurrentAim);
        }

        protected override void AimAt(Vector3 target)
        {
            base.AimAt(target);
            _aimAtDirection = target - transform.position;
            _aimAtQuaternion = Quaternion.LookRotation(_aimAtDirection, Vector3.up);
            //transform.rotation = Quaternion.Lerp(transform.rotation, _aimAtQuaternion * GetRecoilRotation(), WeaponRotationSpeed * Time.deltaTime);
            transform.rotation = _aimAtQuaternion * GetRecoilRotation();
        }

        /// <summary>
        /// Every frame, moves the reticle if it's been told to follow the pointer
        /// </summary>
        protected override void MoveReticle()
        {
            if (ReticleType == ReticleTypes.None) { return; }
            if (_reticle == null) { return; }
            if (_weapon.Owner.ConditionState.CurrentState == CharacterStates.CharacterConditions.Paused) { return; }

            if (ReticleType == ReticleTypes.Scene)
            {
                // if we're not supposed to rotate the reticle, we force its rotation, otherwise we apply the current look rotation
                if (!RotateReticle)
                {
                    _reticle.transform.rotation = Quaternion.identity;
                }
                else
                {
                    if (ReticleAtMousePosition)
                    {
                        _reticle.transform.rotation = _lookRotation;
                    }
                }

                // if we're in follow mouse mode and the current control scheme is mouse, we move the reticle to the mouse's position
                if (ReticleAtMousePosition && AimControl == AimControls.Mouse)
                {
                    _reticle.transform.position = MMMaths.Lerp(_reticle.transform.position, _reticlePosition, 0.3f, Time.deltaTime);
                }
                _reticlePosition = _reticle.transform.position;
            }

            if (ReticleType == ReticleTypes.UI)
            {
                _reticleUIPosition = _reticle.transform.position;
            }
        }


        protected virtual void RotateTarget()
        {
            _weapon.Owner.CameraTarget.transform.rotation = _lookRotation;
        }

        protected virtual void GetWeaponAimReticlePoint()
        {
            _weaponAimReticle.SetActive(_weaponAimReticleEnable);
            if (!UseWeaponAimReticle || !_weaponAimReticleUsing)
            {
                DisableWeaponAimReticle();
                return;
            }
            if (Physics.Raycast(new Ray(_weapon.WeaponUseTransform.position, _weapon.WeaponUseTransform.forward),
                out RaycastHit raycastHit, float.PositiveInfinity, ReticleObstacleMask.value))
            {
                EnableWeaponAimReticle();
                _weaponAimReticlePoint = raycastHit.point;
            }
            else
            {
                DisableWeaponAimReticle();
            }
        }

        protected virtual void MoveWeaponAimReticle(Vector3 point)
        {
            if (!UseWeaponAimReticle || !_weaponAimReticleEnable)
            {
                return;
            }
            _weaponAimReticle.transform.position = point;
        }

        /// <summary>
        /// 设置准星的缩放值
        /// </summary>
        /// <param name="scale">缩放值</param>
        public virtual void SetReticleScale(Vector3 scale)
        {
            _reticle.transform.localScale = scale;
        }

        /// <summary>
        /// 获取准星的缩放值
        /// </summary>
        /// <returns>缩放值</returns>
        public virtual Vector3 GetReticleScale()
        {
            return _reticle.transform.localScale;
        }

        /// <summary>
        /// 开启武器准星
        /// </summary>
        protected virtual void EnableWeaponAimReticle()
        {
            if (!UseWeaponAimReticle)
            {
                return;
            }
            _weaponAimReticleEnable = true;
        }

        /// <summary>
        /// 关闭武器准星
        /// </summary>
        protected virtual void DisableWeaponAimReticle()
        {
            if (!UseWeaponAimReticle)
            {
                return;
            }
            _weaponAimReticleEnable = false;
        }

        protected virtual Quaternion GetRecoilRotation()
        {
            float deltaTime = Time.deltaTime;
            _lastReticleTime += deltaTime;
            if (_lastReticleTime > _weapomRecoilRevertTime)
            {
                _currentWeaponRecoilAngle = Vector2.zero;
                return Quaternion.identity;
            }


            float revertedX = _weapomRecoilRevertSpeed.x * deltaTime;
            float revertedY = _weapomRecoilRevertSpeed.y * deltaTime;

            if (_currentWeaponRecoilAngle.x > 0)
            {
                revertedX = Mathf.Max(0, _currentWeaponRecoilAngle.x - revertedX);
            }
            else if (_currentWeaponRecoilAngle.x < 0)
            {
                revertedX = Mathf.Min(0, _currentWeaponRecoilAngle.x + revertedX);
            }
            else
            {
                revertedX = 0;
            }
            if (_currentWeaponRecoilAngle.y > 0)
            {
                revertedY = Mathf.Max(0, _currentWeaponRecoilAngle.y - revertedY);
            }
            else if (_currentWeaponRecoilAngle.y < 0)
            {
                revertedY = Mathf.Min(0, _currentWeaponRecoilAngle.y + revertedY);
            }
            else
            {
                revertedY = 0;
            }

            _currentWeaponRecoilAngle = new Vector2(revertedX, revertedY);

            return Quaternion.Euler(_currentWeaponRecoilAngle);
        }

        /// <summary>
        /// 添加后坐力
        /// </summary>
        /// <param name="randomAngle">随机角度</param>
        /// <param name="revertSpeed">回复速度</param>
        /// <param name="speedMultiplyCurve">速度倍率曲线</param>
        /// <param name="recoilRevertTime">后坐力回复时间</param>
        public virtual void AddRecoilRotation(Vector2 randomAngle, Vector2 revertSpeed, AnimationCurve speedMultiplyCurve, float recoilRevertTime)
        {
            _currentWeaponRecoilAngle += randomAngle;
            float multiply = speedMultiplyCurve.Evaluate(_currentWeaponRecoilAngle.magnitude);
            _weapomRecoilRevertSpeed = revertSpeed * multiply;
            _weapomRecoilRevertTime = recoilRevertTime;
            _lastReticleTime = 0;
        }

        /// <summary>
        /// Returns the current mouse position
        /// </summary>
        public override Vector3 GetMousePosition()
        {
            return _mainCamera.ScreenToWorldPoint(InputManagerOverride.InstanceOverride.MousePosition);
        }


        /// <summary>
        /// Hides (or shows) the reticle based on the DisplayReticle setting
        /// </summary>
        protected override void HideReticle()
        {
            if (_reticle != null)
            {
                if (GameManager.Instance.Paused || GUIManagerOverride.InstanceOverride.EnableInfoPanel)
                {
                    _reticle.gameObject.SetActive(false);
                    return;
                }
                _reticle.gameObject.SetActive(DisplayReticle);
            }
        }
    }
}


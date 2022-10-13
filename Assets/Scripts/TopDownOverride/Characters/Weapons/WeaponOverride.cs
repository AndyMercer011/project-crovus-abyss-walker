using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class WeaponOverride : Weapon
    {
        [MMInspectorGroup("ID")]
        /// 武器图标
        [Tooltip("武器图标")]
        public Sprite WeaponIcon;

        [MMInspectorGroup("Use", true)]
        [Header("PumpAction")]
        /// 泵动移动的目标
        [Tooltip("泵动移动的目标")]
        public Transform PumpActionTarget;
        /// 泵动移动的位置
        [Tooltip("泵动移动的位置")]
        public Vector3 PumpActionPosition;
        /// 泵动移动速度曲线
        [Tooltip("泵动移动速度曲线")]
        public AnimationCurve PumpActionDistanceCurve;
        /// 是否在泵动
        [Tooltip("是否在泵动")]
        [MMReadOnly]
        public bool IsPumping;

        [MMInspectorGroup("Aim", true)]
        /// 换弹打断瞄准
        [Tooltip("换弹打断瞄准")]
        public bool ReloadBreakAim = true;
        /// 瞄准切换时间
        [Tooltip("瞄准切换时间")]
        public float AimSwitchingTime;
        /// 瞄准时视野角度倍率
        [Tooltip("瞄准时视野角度倍率")]
        public float AimViewAngleMultiplier;
        /// 瞄准时视野距离倍率
        [Tooltip("瞄准时视野距离倍率")]
        public float AimViewDistanceMultiplier;
        /// 瞄准时视野距离倍率
        [Tooltip("瞄准时视野距离倍率")]
        public float AimCameraTargetOffest;
        /// 瞄准时准星倍率
        [Tooltip("瞄准时准星倍率")]
        public float AimReticleScale;
        /// 是否在瞄准
        [Tooltip("是否在瞄准")]
        [MMReadOnly]
        public bool Aiming;

        /// <summary>
        /// 是否开启单发装填
        /// </summary>
        [MMInspectorGroup("Magazine", true)]
        [Tooltip("是否开启单发装填")]
        public bool SingleProjectilePerReload;
        /// <summary>
        /// 单发装填时间
        /// </summary>
        [Tooltip("单发装填时间")]
        public float SingleReloadTime;
        /// <summary>
        /// 单发装填动画循环曲线
        /// </summary>
        [Tooltip("单发装填动画循环曲线")]
        public AnimationCurve SingleReloadAnimationLoopCurve;

        [MMInspectorGroup("Position")]
        /// an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.
        [Tooltip("an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.")]
        public Transform WeaponAttachmentTransform;


        [MMInspectorGroup("Movement", true)]
        /// 是否应用瞄准时移动速度倍率
        [Tooltip("是否应用瞄准时移动速度倍率")]
        public bool ModifyMovementWhileAiming = false;
        /// 瞄准时移动速度倍率
        [Tooltip("瞄准时移动速度倍率")]
        public float AimingMovementMultiplier;

        [MMInspectorGroup("Animation Parameters Names")]
        /// 换弹时间动画参数
        [Tooltip("换弹时间动画参数")]
        public string ReloadTimeAnimationParameter;

        [MMInspectorGroup("Feedbacks")]
        /// 泵动Feedback
        [Tooltip("泵动Feedback")]
        public MMFeedbacks WeaponPumpActionMMFeedback;

        protected bool _playWeaponPumpActionMMFeedback;
        protected float _reloadingAnimationTime = 0f;
        protected float _singleReloadingCounter = 0f;
        protected float _cameraTargetOffsetStorage = 0f;
        protected float _aimMovementMultiplierStorage = 1;
        protected float _pumpActionTimeCounter;

        protected int _reloadTimeAnimationParameter;
        protected int _singleReloadedProjectileCount = 0;
        protected int _needRelaodTimes;

        protected Vector3 _reticleScaleStorage;
        protected Vector3 _pumpActionPositionStorage;

        protected CharacterView _characterView;
        protected WeaponAim3DOverride weaponAim3DOverride;

        public WeaponAim WeaponAim { get => _weaponAim; }
        public WeaponAim3DOverride WeaponAim3DOverride
        {
            get
            {
                if (weaponAim3DOverride == null)
                {
                    weaponAim3DOverride = _weaponAim as WeaponAim3DOverride;
                }
                return weaponAim3DOverride;
            }
        }

        protected override void InitializeFeedbacks()
        {
            base.InitializeFeedbacks();
            WeaponPumpActionMMFeedback?.Initialization(this.gameObject);
        }

        /// <summary>
        /// Sets the weapon's owner
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public override void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
        {
            base.SetOwner(newOwner, handleWeapon);
            if (Owner != null)
            {
                _characterView = Owner.GetComponent<Character>()?.FindAbility<CharacterView>();
            }
        }

        /// <summary>
        /// 瞄准输入开始
        /// </summary>
        public virtual void AimInputStart()
        {
            if (Aiming)
            {
                return;
            }
            Aiming = true;
            _characterView?.ChangeViewRangeSmooth(_characterView.InitialViewAngle * AimViewAngleMultiplier,
                _characterView.InitialViewDistance * AimViewDistanceMultiplier, AimSwitchingTime);

            if (WeaponAim3DOverride.AimEnableAimReticle)
            {
                WeaponAim3DOverride._weaponAimReticleUsing = true;
            }

            _reticleScaleStorage = WeaponAim3DOverride.GetReticleScale();
            WeaponAim3DOverride.SetReticleScale(_reticleScaleStorage * AimReticleScale);

            if ((_characterMovement != null) && (ModifyMovementWhileAiming))
            {
                _aimMovementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
                _characterMovement.MovementSpeedMultiplier = AimingMovementMultiplier;
            }
            if ((_weaponAim != null))
            {
                _cameraTargetOffsetStorage = _weaponAim.CameraTargetOffset;
                _weaponAim.CameraTargetOffset = AimCameraTargetOffest;
            }
        }

        /// <summary>
        /// 瞄准输入停止
        /// </summary>
        public virtual void AimInputStop()
        {
            if (!Aiming)
            {
                return;
            }
            Aiming = false;
            _characterView?.ChangeViewRangeSmooth(_characterView.InitialViewAngle,
                 _characterView.InitialViewDistance, AimSwitchingTime);

            if (WeaponAim3DOverride.AimEnableAimReticle)
            {
                WeaponAim3DOverride._weaponAimReticleUsing = false;
            }

            WeaponAim3DOverride.SetReticleScale(_reticleScaleStorage);

            if ((_characterMovement != null) && (ModifyMovementWhileAiming))
            {
                _characterMovement.MovementSpeedMultiplier = _aimMovementMultiplierStorage;
                _aimMovementMultiplierStorage = 1;
            }
            if ((_weaponAim != null))
            {
                _weaponAim.CameraTargetOffset = _cameraTargetOffsetStorage;
            }
        }

        /// <summary>
        /// If the weapon is idle, we reset the movement multiplier
        /// </summary>
        public override void CaseWeaponIdle()
        {
            if (_delayBetweenUsesCounter > 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
            }
            if (!Aiming)
            {
                base.CaseWeaponIdle();
            }
        }

        public override void WeaponInputStop()
        {
            if (_reloading)
            {
                return;
            }
            _triggerReleased = true;
        }

        /// <summary>
        /// When in delay between uses, we either turn our weapon off or make a shoot request
        /// </summary>
        public override void CaseWeaponDelayBetweenUses()
        {
            if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
            {
                TurnWeaponOff();
                return;
            }
            _delayBetweenUsesCounter -= Time.deltaTime;

            if (TriggerMode == TriggerModes.PumpAction)
            {
                if (_pumpActionTimeCounter == 0)
                {
                    _pumpActionPositionStorage = PumpActionTarget.localPosition;
                }

                _pumpActionTimeCounter += Time.deltaTime;

                float t = PumpActionDistanceCurve.Evaluate(_pumpActionTimeCounter / TimeBetweenUses);
                if (t != 0 && !_playWeaponPumpActionMMFeedback)
                {
                    IsPumping = true;
                    _playWeaponPumpActionMMFeedback = true;
                    TriggerWeaponPumpActionMMFeedback();
                }
                if (t == 0 && _playWeaponPumpActionMMFeedback)
                {
                    IsPumping = false;
                }

                PumpActionTarget.localPosition = Vector3.Lerp(_pumpActionPositionStorage, _pumpActionPositionStorage + PumpActionPosition, t);
            }

            if (_delayBetweenUsesCounter <= 0)
            {
                _pumpActionTimeCounter = 0;
                _playWeaponPumpActionMMFeedback = false;

                if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
                {
                    StartCoroutine(ShootRequestCo());
                }
                else
                {
                    TurnWeaponOff();
                }
            }
        }

        private void TriggerWeaponPumpActionMMFeedback()
        {
            WeaponPumpActionMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// If a reload is needed, we mention it and switch to idle
        /// </summary>
        public override void CaseWeaponReloadNeeded()
        {
            ReloadNeeded();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        /// <summary>
        /// on reload start, we reload the weapon and switch to reload
        /// </summary>
        public override void CaseWeaponReloadStart()
        {
            ReloadWeapon();
            if (!SingleProjectilePerReload)
            {
                _reloadingCounter = ReloadTime;
            }
            else
            {
                _needRelaodTimes = MagazineSize - CurrentAmmoLoaded + 1;
                _reloadingCounter = SingleReloadTime * (_needRelaodTimes != 0 ? _needRelaodTimes : 1);
                _singleReloadingCounter = 0;
                _singleReloadedProjectileCount = 0;
            }
            WeaponState.ChangeState(WeaponStates.WeaponReload);
            if (ReloadBreakAim)
            {
                AimInputStop();
            }
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        /// <param name="ammo">Ammo.</param>
        protected override void ReloadWeapon()
        {
            if (MagazineBased && !SingleProjectilePerReload)
            {
                TriggerWeaponReloadFeedback();
            }
        }

        /// <summary>
        /// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
        /// </summary>
        public override void CaseWeaponReload()
        {
            float deltaTime = Time.deltaTime;
            _reloadingCounter -= deltaTime;

            if (SingleProjectilePerReload)
            {
                _singleReloadingCounter += deltaTime;
                if (_singleReloadingCounter > SingleReloadTime)
                {
                    _singleReloadingCounter = 0;
                    if (CurrentAmmoLoaded < MagazineSize)
                    {
                        TriggerWeaponReloadFeedback();
                        CurrentAmmoLoaded++;
                    }
                    _singleReloadedProjectileCount++;
                }
                GetMultiReloadAnimationTime();
            }
            else
            {
                SingleReloadingAnimationTime();
            }

            if (_reloadingCounter <= 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
            }
        }

        /// <summary>
        /// on reload stop, we swtich to idle and load our ammo
        /// </summary>
        public override void CaseWeaponReloadStop()
        {
            _reloading = false;
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            if (WeaponAmmo == null)
            {
                CurrentAmmoLoaded = MagazineSize;
            }
        }

        private void SingleReloadingAnimationTime()
        {
            if (!SingleProjectilePerReload)
            {
                _reloadingAnimationTime = 1 - _reloadingCounter / ReloadTime;
            }
            else
            {
                _reloadingAnimationTime = _singleReloadingCounter / SingleReloadTime;
            }
        }

        private void GetMultiReloadAnimationTime()
        {
            if (_singleReloadedProjectileCount != 0)
            {
                if (_singleReloadedProjectileCount == _needRelaodTimes - 1 || _singleReloadedProjectileCount == _needRelaodTimes)
                {
                    MultiReloadEndAnimationTime();
                }
                else
                {
                    MultiReloadingAnimationTime();
                }
            }
            else
            {
                if (_singleReloadedProjectileCount == _needRelaodTimes - 1)
                {
                    MultiReloadEndAnimationTime();
                }
                else
                {
                    MultiReloadStartAnimationTime();
                }
            }
        }

        private void MultiReloadStartAnimationTime()
        {
            _reloadingAnimationTime = _singleReloadingCounter / SingleReloadTime;
            _reloadingAnimationTime = MMMaths.Remap(_reloadingAnimationTime, 0, 1,
                0, _reloadingAnimationTime = SingleReloadAnimationLoopCurve.Evaluate(1));
        }

        private void MultiReloadingAnimationTime()
        {
            _reloadingAnimationTime = _singleReloadingCounter / SingleReloadTime;
            _reloadingAnimationTime = SingleReloadAnimationLoopCurve.Evaluate(_reloadingAnimationTime);
        }

        protected void MultiReloadEndAnimationTime()
        {
            _reloadingAnimationTime = 1 - _reloadingCounter / SingleReloadTime;
            _reloadingAnimationTime = MMMaths.Remap(_reloadingAnimationTime, 0, 1,
              _reloadingAnimationTime = SingleReloadAnimationLoopCurve.Evaluate(0), 1);
        }

        /// <summary>
        /// When the weapon is used, plays the corresponding sound
        /// </summary>
        public override void WeaponUse()
        {
            ApplyRecoil();
            base.WeaponUse();
        }

        protected virtual void ApplyRecoil()
        {
            if ((RecoilForce > 0f) && (_controller != null))
            {
                if (Owner != null)
                {
                    if (!_controllerIs3D)
                    {
                        if (Flipped)
                        {
                            _controller.Impact(this.transform.right, RecoilForce);
                        }
                        else
                        {
                            _controller.Impact(-this.transform.right, RecoilForce);
                        }
                    }
                    else
                    {
                        _controller.Impact(-this.transform.forward, RecoilForce);
                    }
                }
            }
        }

        /// <summary>
        /// Initiates a reload
        /// </summary>
        public override void InitiateReloadWeapon()
        {
            if (CurrentAmmoLoaded == MagazineSize || IsPumping)
            {
                return;
            }
            base.InitiateReloadWeapon();
        }

        /// <summary>
        /// Applies the offset specified in the inspector
        /// </summary>
        public override void ApplyOffset()
        {

            if (!WeaponCurrentlyActive)
            {
                return;
            }

            _weaponAttachmentOffset = WeaponAttachmentTransform != null ? 
                -WeaponAttachmentTransform.localPosition + WeaponAttachmentOffset : WeaponAttachmentOffset;

            if (Owner == null)
            {
                return;
            }

            if (Owner.Orientation2D != null)
            {
                if (Flipped)
                {
                    _weaponAttachmentOffset.x = WeaponAttachmentTransform != null ? 
                        WeaponAttachmentTransform.localPosition.x + WeaponAttachmentOffset.x : WeaponAttachmentOffset.x;
                }

                // we apply the offset
                if (transform.parent != null)
                {
                    _weaponOffset = transform.parent.position + _weaponAttachmentOffset;
                    transform.position = _weaponOffset;
                }
            }
            else
            {
                if (transform.parent != null)
                {
                    _weaponOffset = transform.localRotation * _weaponAttachmentOffset;
                    transform.localPosition = _weaponOffset;
                }
            }
        }

        protected override void AddParametersToAnimator(Animator animator, HashSet<int> list)
        {
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadTimeAnimationParameter, out _reloadTimeAnimationParameter, AnimatorControllerParameterType.Float, list);
            base.AddParametersToAnimator(animator, list);
        }

        protected override void UpdateAnimator(Animator animator, HashSet<int> list)
        {
            base.UpdateAnimator(animator, list);

            if (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload && ReloadTime != 0)
            {
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _reloadTimeAnimationParameter,
                    _reloadingAnimationTime, list, PerformAnimatorSanityChecks);
            }
        }
    }
}


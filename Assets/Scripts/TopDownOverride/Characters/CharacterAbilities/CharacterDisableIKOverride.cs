using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 控制角色IK开关
    /// </summary>
    public class CharacterDisableIKOverride : CharacterAbility
    {
        [Header("IK")]
        [SerializeField] private WeaponIK boundWeaponIK;
        /// <summary>
        /// 是否启用关闭左手IK
        /// </summary>
        [Tooltip("是否启用关闭左手IK")]
        [SerializeField] private bool detachLeftHand = true;
        /// <summary>
        /// 是否启用关闭右手IK
        /// </summary>
        [Tooltip("是否启用关闭右手IK")]
        [SerializeField] private bool detachRightHand = false;
        /// <summary>
        /// 是否在武器换弹时关闭IK
        /// </summary> 
        [Tooltip("是否在武器换弹时关闭IK")]
        [SerializeField] private bool disableIKDuringReload = true;

        [Header("Weapon Models")]
        [SerializeField] private List<WeaponModel> weaponModels;

        protected CharacterHandleWeapon _handleWeapon;

        private bool reloadDisableIK;

        public WeaponIK BoundWeaponIK { get => boundWeaponIK; set => boundWeaponIK = value; }
        public bool DetachLeftHand { get => detachLeftHand; set => detachLeftHand = value; }
        public bool DetachRightHand { get => detachRightHand; set => detachRightHand = value; }
        public bool DisableIKDuringReload { get => disableIKDuringReload; set => disableIKDuringReload = value; }
        public List<WeaponModel> WeaponModels { get => weaponModels; set => weaponModels = value; }

        protected override void Initialization()
        {
            base.Initialization();
            _handleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
        }

        protected virtual void LateUpdate()
        {
            if ((_handleWeapon == null) || (!_handleWeapon.isActiveAndEnabled))
            {
                return;
            }
            if (_handleWeapon.CurrentWeapon == null)
            {
                return;
            }


            if (disableIKDuringReload)
            {
                if (!reloadDisableIK && _handleWeapon.CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
                {
                    reloadDisableIK = true;
                    ForceDisable();
                }
                if (reloadDisableIK && _handleWeapon.CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop)
                {
                    reloadDisableIK = false;
                    ForceEnable();
                }
            }
        }

        /// <summary>
        /// 强制开启IK
        /// </summary>
        public void ForceDisable()
        {
            if (detachLeftHand)
            {
                BoundWeaponIK.AttachLeftHand = false;
            }
            if (detachRightHand)
            {
                BoundWeaponIK.AttachRightHand = false;
            }
        }
        /// <summary>
        /// 强制关闭IK
        /// </summary>
        public void ForceEnable()
        {
            if (detachLeftHand)
            {
                BoundWeaponIK.AttachLeftHand = true;
            }
            if (detachRightHand)
            {
                BoundWeaponIK.AttachRightHand = true;
            }
        }
    }
}
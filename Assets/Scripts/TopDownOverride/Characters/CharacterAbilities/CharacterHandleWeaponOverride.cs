using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class CharacterHandleWeaponOverride : CharacterHandleWeapon
    {
        /// the ID of the WeaponDisplayID this ability should update
        [Tooltip("the ID of the WeaponDisplayID this ability should update")]
        public int WeaponDisplayID = 0;

        protected WeaponOverride weaponOverride;

        /// <summary>
        /// 是否瞄准
        /// </summary>
        public bool IsAiming
        {
            get
            {
                if (WeaponOverride == null)
                    return false;
                else
                    return WeaponOverride.Aiming;
            }
        }
        /// <summary>
        /// 武器状态
        /// </summary>
        public Weapon.WeaponStates WeaponState
        {
            get
            {
                if (CurrentWeapon == null)
                    return Weapon.WeaponStates.WeaponIdle;
                else
                    return CurrentWeapon.WeaponState.CurrentState;
            }
        }

        protected WeaponOverride WeaponOverride
        {
            get
            {
                if (weaponOverride == null)
                {
                    weaponOverride = CurrentWeapon as WeaponOverride;
                }
                return weaponOverride;
            }
        }

        protected CharacterOrientation3D _characterOrientation3D;
        private GUIManagerOverride gUIManagerOverride;
        protected CharacterRun _characterRun;

        /// <summary>
        /// Grabs various components and inits stuff
        /// </summary>
        public override void Setup()
        {
            base.Setup();
            _characterOrientation3D = _character?.FindAbility<CharacterOrientation3D>();
            _characterRun = _character?.FindAbility<CharacterRun>();
        }

        /// <summary>
        /// Gets input and triggers methods based on what's been pressed
        /// </summary>
        protected override void HandleInput()
        {
            if ((InputManagerOverride.InstanceOverride.AimButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed))
            {
                AimStart();
            }
            else
            {
                AimStop();
            }

            if (InputManagerOverride.InstanceOverride.ThrowButton.State.CurrentState == MMInput.ButtonStates.ButtonDown
                || InputManagerOverride.InstanceOverride.PlayerInputPaused)
            {
                ThrowInput();
            }

            base.HandleInput();
        }

        /// <summary>
        /// 使角色瞄准
        /// </summary> 
        public virtual void AimStart()
        {
            if (!AbilityAuthorized
                || (CurrentWeapon == null)
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
                || (_movement.CurrentState == CharacterStates.MovementStates.Running))
            {
                return;
            }
            if (WeaponOverride.ReloadBreakAim && CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
            {
                return;
            }

            WeaponOverride.AimInputStart();
        }

        /// <summary>
        /// 使角色停止瞄准
        /// </summary>
        public virtual void AimStop()
        {
            if (!AbilityAuthorized
                || (CurrentWeapon == null))
            {
                return;
            }
            WeaponOverride.AimInputStop();
        }

        /// <summary>
        /// 使角色进入投掷模式
        /// </summary>
        public virtual void ThrowInput()
        {
            if (!AbilityAuthorized
                || (CurrentWeapon == null)
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
                || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
                || (_movement.CurrentState == CharacterStates.MovementStates.Running))
            {
                return;
            }

            ThrownWeapon thrownWeapon = WeaponOverride as ThrownWeapon;


            if (thrownWeapon != null)
            {
                if (InputManagerOverride.InstanceOverride.PlayerInputPaused)
                {
                    thrownWeapon.ThrowStop();
                }
                else
                {
                    thrownWeapon.ThrowInput();
                }
            }
        }

        /// <summary>
        /// Causes the character to start shooting
        /// </summary>
        public override void ShootStart()
        {
            base.ShootStart();
            if (!AbilityAuthorized
               || (CurrentWeapon == null)
               || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))
            {
                if (!_characterGridMovement.PerfectTile)
                {
                    return;
                }
            }
            _characterRun.RunStop();

        }

        /// <summary>
        /// Instantiates the specified weapon
        /// </summary>
        /// <param name="newWeapon"></param>
        /// <param name="weaponID"></param>
        /// <param name="combo"></param>
        protected override void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            if (!combo)
            {
                CurrentWeapon = (Weapon)Instantiate(newWeapon);
            }

            CurrentWeapon.name = newWeapon.name;
            CurrentWeapon.transform.SetParent(WeaponAttachment.transform, false);
            CurrentWeapon.transform.rotation = WeaponAttachment.rotation;
            CurrentWeapon.WeaponID = weaponID;
            CurrentWeapon.FlipWeapon();
            _weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
            CurrentWeapon.SetOwner(_character, this);

            HandleWeaponAim();

            // we handle (optional) inverse kinematics (IK) 
            HandleWeaponIK();

            // we handle the weapon model
            HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

            // we turn off the gun's emitters.
            CurrentWeapon.Initialization();
            CurrentWeapon.InitializeComboWeapons();
            CurrentWeapon.InitializeAnimatorParameters();
            InitializeAnimatorParameters();
            GUIManagerOverride.InstanceOverride.GameplayPanel.UpdateWeaponIcons(WeaponOverride.WeaponIcon, null, CurrentWeapon.TriggerMode, _character.PlayerID, WeaponDisplayID);
        }

        /// <summary>
        /// Updates the ammo display bar and text.
        /// </summary>
        public override void UpdateAmmoDisplay()
        {
            if ((GUIManagerOverride.HasInstanceOverride) && (_character.CharacterType == Character.CharacterTypes.Player))
            {
                if (CurrentWeapon == null)
                {
                    GUIManagerOverride.InstanceOverride.GameplayPanel.SetWeaponDisplays(false, _character.PlayerID, WeaponDisplayID);
                    return;
                }

                float updateCD;
                if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
                {
                    if (WeaponOverride.SingleProjectilePerReload)
                    {
                        updateCD = WeaponOverride.SingleReloadTime;
                    }
                    else
                    {
                        updateCD = CurrentWeapon.ReloadTime;
                    }
                }
                else
                {
                    updateCD = CurrentWeapon.TimeBetweenUses;
                }

                if (CurrentWeapon.WeaponAmmo == null)
                {
                    GUIManagerOverride.InstanceOverride.GameplayPanel.SetWeaponDisplays(true, _character.PlayerID, WeaponDisplayID);
                    GUIManagerOverride.InstanceOverride.GameplayPanel.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, -1, -1,
                        CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, updateCD,
                        _character.PlayerID, WeaponDisplayID);
                    return;
                }
                else
                {
                    GUIManagerOverride.InstanceOverride.GameplayPanel.SetWeaponDisplays(true, _character.PlayerID, WeaponDisplayID);
                    GUIManagerOverride.InstanceOverride.GameplayPanel.UpdateAmmoDisplays(CurrentWeapon.MagazineBased,
                        CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable,
                        CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize,
                        updateCD, _character.PlayerID, WeaponDisplayID);
                    return;
                }
            }
        }
    }

}
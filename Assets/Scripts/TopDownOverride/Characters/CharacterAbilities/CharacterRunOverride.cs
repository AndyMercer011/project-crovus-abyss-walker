using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class CharacterRunOverride : CharacterRun
    {
        protected CharacterHandleWeaponOverride characterHandleWeaponOverride;

        protected override void Initialization()
        {
            base.Initialization();
            characterHandleWeaponOverride = _character.FindAbility<CharacterHandleWeapon>() as CharacterHandleWeaponOverride;
        }

        /// <summary>
        /// At the beginning of each cycle, we check if we've pressed or released the run button
        /// </summary>
        protected override void HandleInput()
        {
            if (AutoRun)
            {
                if (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold)
                {
                    _inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }
                else
                {
                    _inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
            }

            if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
            {
                if (characterHandleWeaponOverride.WeaponState != Weapon.WeaponStates.WeaponUse &&
               characterHandleWeaponOverride.WeaponState != Weapon.WeaponStates.WeaponDelayBetweenUses)
                {
                    RunStart();
                }
            }
            else
            {
                RunStop();
            }
        }


        /// <summary>
        /// Causes the character to start running.
        /// </summary>
        public override void RunStart()
        {
            if (characterHandleWeaponOverride.IsAiming)
            {
                return;
            }
            base.RunStart();

            if (!AbilityAuthorized // if the ability is not permitted
                 || (!_controller.Grounded) // or if we're not grounded
                 || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) // or if we're not in normal conditions
                 || (_movement.CurrentState != CharacterStates.MovementStates.Walking)) // or if we're not walking
            {
                // we do nothing and exit
                return;
            }
            characterHandleWeaponOverride.ShootStop();
        }

    }
}

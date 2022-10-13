using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class HealthOverride : Health
    {
        /// <summary>
        /// Forces a refresh of the character's health bar
        /// </summary>
        public override void UpdateHealthBar(bool show)
        {
            if (_healthBar != null)
            {
                _healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
            }

            if (MasterHealth == null)
            {
                if (_character != null)
                {
                    if (_character.CharacterType == Character.CharacterTypes.Player)
                    {
                        // We update the health bar
                        if (GUIManagerOverride.HasInstanceOverride)
                        {
                            GUIManagerOverride.InstanceOverride.GameplayPanel.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                            if (GUIManagerOverride.InstanceOverride.EnableInfoPanel)
                            {
                                GUIManagerOverride.InstanceOverride.MainMenuPanel.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                            }
                        }
                    }
                }
            }
        }
    }
}

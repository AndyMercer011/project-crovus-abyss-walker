using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this component to a character and it'll be able to activate/desactivate the information panel
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
    public class CharacterInfoPanel : CharacterAbility, MMEventListener<TopDownEngineEvent>
    {

        private bool infoPanelEnabled;

        public void OnMMEvent(TopDownEngineEvent eventType)
        {
            if (eventType.EventType == TopDownEngineEventTypes.PlayerDeath && infoPanelEnabled)
            {
                ToggleInfoPanel();
            }
        }

        /// <summary>
        /// Every frame, we check the input
        /// </summary>
        protected override void HandleInput()
        {
            if (InputManagerOverride.InstanceOverride.InfoPanelButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                ToggleInfoPanel();
            }
        }

        /// <summary>
        /// If the pause button has been pressed, we change the pause state
        /// </summary>
        protected virtual void ToggleInfoPanel()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            PlayAbilityStartFeedbacks();
            // we trigger a Pause event for the GameManager and other classes that could be listening to it too
            if (GUIManagerOverride.HasInstanceOverride)
            {
                if (GUIManagerOverride.InstanceOverride.ToggleInfoPanel())
                {
                    infoPanelEnabled = true;
                    _character.CharacterHealth.UpdateHealthBar(true);
                    InputManagerOverride.InstanceOverride.EnableUIInput(false);
                    InputManagerOverride.InstanceOverride.DisablePlayerInput(true);
                }
                else
                {
                    infoPanelEnabled = false;
                    InputManagerOverride.InstanceOverride.EnablePlayerInput(true);
                    InputManagerOverride.InstanceOverride.DisableUIInput(false);
                }
            }
        }

        /// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        /// <summary>
        /// OnDisable, we stop listening to events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}

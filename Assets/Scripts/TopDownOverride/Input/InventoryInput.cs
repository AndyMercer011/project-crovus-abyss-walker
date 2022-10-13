/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Integrations.InputSystem
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using System.Collections.Generic;
    using MoreMountains.TopDownEngine;

    /// <summary>
    /// Responds to input using the Unity Input System.
    /// </summary>
    public class InventoryInput : Opsive.Shared.Input.PlayerInput
    {
        private Dictionary<string, InputAction> m_InputActionByName = new Dictionary<string, InputAction>();

        protected override bool CanCheckForController => false;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected override bool GetButtonInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null)
            {
                if (action.activeControl is ButtonControl button && button.isPressed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected override bool GetButtonDownInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null)
            {
                if (action.activeControl is ButtonControl button && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returnstrue if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected override bool GetButtonUpInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null)
            {
                for (int i = 0; i < action.controls.Count; i++)
                {
                    if (action.controls[i] is ButtonControl button && button.wasReleasedThisFrame)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected override float GetAxisInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null)
            {
                return action.ReadValue<float>();
            }
            return 0.0f;
        }

        /// <summary>
        /// Internal method which returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected override float GetAxisRawInternal(string name)
        {
            return GetAxisInternal(name);
        }

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public override Vector2 GetMousePosition() { return Mouse.current.position.ReadValue(); }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected override void EnableGameplayInput(bool enable)
        {
            base.EnableGameplayInput(enable);
        }

        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(hasFocus);
        }

        /// <summary>
        /// Returns the InputAction with the specified name.
        /// </summary>
        /// <param name="name">The name of the InputAction.</param>
        /// <returns>The InputAction with the specified name.</returns>
        private InputAction GetActionByName(string name)
        {
            if (!InputManagerOverride.HasInstanceOverride)
            {
                return null;
            }
            if (!m_InputActionByName.TryGetValue(name, out var inputAction))
            {
                inputAction = InputManagerOverride.InstanceOverride.InputActions?.FindAction(name);
                m_InputActionByName.Add(name, inputAction);
            }

            return inputAction;
        }
    }
}

using MoreMountains.Tools;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This is a replacement InputManager if you prefer using Unity's InputSystem over the legacy one.
    /// Note that it's not the default solution in the engine at the moment, because older versions of Unity don't support it, 
    /// and most people still prefer not using it
    /// You can see an example of how to set it up in the MinimalScene3D_InputSystem demo scene
    /// </summary>
    public class InputManagerOverride : InputManager
    {
        public static InputManagerOverride InstanceOverride
        {
            get
            {
                if (Instance != null)
                {
                    if (instanceOverride == null)
                    {
                        instanceOverride = Instance as InputManagerOverride;
                    }
                    return instanceOverride;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool HasInstanceOverride => InstanceOverride != null;

        protected static InputManagerOverride instanceOverride;

        /// a set of input actions to use to read input on
        public TopDownEngineInputActionsOverride InputActions;

        /// 武器瞄准
        public MMInput.IMButton AimButton { get; protected set; }

        /// 投掷
        public MMInput.IMButton ThrowButton { get; protected set; }

        /// 投掷
        public MMInput.IMButton InfoPanelButton { get; protected set; }

        /// 启用镜头控制
        public MMInput.IMButton CameraInputActiveButton { get; protected set; }

        /// 鼠标输入位置
        public Vector2 MousePosition { get { return _mousePosition; } }

        /// the camera rotation axis input value
        public Vector2 CameraMoveInput { get { return _cameraMoveInput; } }

        /// the camera scale input value
        public float CameraScaleInput { get { return _cameraScale; } }

        public bool PlayerInputPaused { get; protected set; }

        protected float _cameraScale;

        protected Vector2 _cameraMoveInput = Vector2.zero;

        protected Vector2 _mousePosition = Vector2.zero;

        protected override void Awake()
        {
            base.Awake();
            InputActions = new TopDownEngineInputActionsOverride();
            EnablePlayerInput(false);
            DisableUIInput(false);
        }

        public virtual void DisablePlayerInput(bool resetInputs)
        {
            PlayerInputPaused = true;
            InputActions.PlayerControls.Disable();
            if (resetInputs)
            {
                ResetInputs();
            }
        }

        public virtual void EnablePlayerInput(bool resetInputs)
        {
            PlayerInputPaused = false;
            InputActions.PlayerControls.Enable();
            if (resetInputs)
            {
                ResetInputs();
            }
        }

        public virtual void DisableUIInput(bool resetInputs)
        {
            InputActions.UI.Disable();
            InputActions.UI.Pause.Enable();
            InputActions.UI.ToggleMenuPanel.Enable();
            if (resetInputs)
            {
                ResetInputs();
            }
        }

        public virtual void EnableUIInput(bool resetInputs)
        {
            InputActions.UI.Enable();
            if (resetInputs)
            {
                ResetInputs();
            }
        }

        protected virtual void ResetInputs()
        {
            _primaryMovement = Vector2.zero;
            _secondaryMovement = Vector2.zero;
            _cameraMoveInput = Vector2.zero;
            _mousePosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            _cameraScale = 0;
            JumpButton.State.ChangeState(MMInput.ButtonStates.Off);
            RunButton.State.ChangeState(MMInput.ButtonStates.Off);
            DashButton.State.ChangeState(MMInput.ButtonStates.Off);
            CrouchButton.State.ChangeState(MMInput.ButtonStates.Off);
            ShootButton.State.ChangeState(MMInput.ButtonStates.Off);
            AimButton.State.ChangeState(MMInput.ButtonStates.Off);
            ThrowButton.State.ChangeState(MMInput.ButtonStates.Off);
            InfoPanelButton.State.ChangeState(MMInput.ButtonStates.Off);
            SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.Off);
            InteractButton.State.ChangeState(MMInput.ButtonStates.Off);
            ReloadButton.State.ChangeState(MMInput.ButtonStates.Off);
            PauseButton.State.ChangeState(MMInput.ButtonStates.Off);
            SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.Off);
            SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.Off);
            TimeControlButton.State.ChangeState(MMInput.ButtonStates.Off);
            CameraInputActiveButton.State.ChangeState(MMInput.ButtonStates.Off);
        }

        /// <summary>
        /// On init we register to all our actions
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();

            ButtonList.Add(AimButton = new MMInput.IMButton(PlayerID, "Aim", AimButtonDown, AimButtonPressed, AimButtonUp));
            ButtonList.Add(ThrowButton = new MMInput.IMButton(PlayerID, "Throw", ThrowButtonDown, ThrowButtonPressed, ThrowButtonUp));
            ButtonList.Add(InfoPanelButton = new MMInput.IMButton(PlayerID, "InfoPanel", ThrowButtonDown, ThrowButtonPressed, ThrowButtonUp));
            ButtonList.Add(CameraInputActiveButton = new MMInput.IMButton(PlayerID, "CameraInputActive", CameraInputActiveButtonDown, CameraInputActiveButtonPressed, CameraInputActiveButtonUp));

            InputActions.PlayerControls.PrimaryMovement.performed += context => _primaryMovement = ApplyCameraRotation(context.ReadValue<Vector2>());
            InputActions.PlayerControls.SecondaryMovement.performed += context => _secondaryMovement = ApplyCameraRotation(context.ReadValue<Vector2>());

            InputActions.PlayerControls.CameraMove.performed += context => _cameraMoveInput = context.ReadValue<Vector2>();

            InputActions.PlayerControls.MousePosition.performed += context => _mousePosition = context.ReadValue<Vector2>();

            InputActions.PlayerControls.CameraScale.performed += context => _cameraScale = -context.ReadValue<Vector2>().y;
            InputActions.PlayerControls.CameraScale.canceled += context => _cameraScale = 0;

            InputActions.PlayerControls.Jump.performed += context => { BindButton(context, JumpButton); };
            InputActions.PlayerControls.Run.performed += context => { BindButton(context, RunButton); };
            InputActions.PlayerControls.Dash.performed += context => { BindButton(context, DashButton); };
            InputActions.PlayerControls.Crouch.performed += context => { BindButton(context, CrouchButton); };
            InputActions.PlayerControls.Shoot.performed += context => { BindButton(context, ShootButton); };
            InputActions.PlayerControls.Aim.performed += context => { BindButton(context, AimButton); };
            InputActions.PlayerControls.Throw.performed += context => { BindButton(context, ThrowButton); };
            InputActions.UI.ToggleMenuPanel.performed += context => { BindButton(context, InfoPanelButton); };
            InputActions.PlayerControls.SecondaryShoot.performed += context => { BindButton(context, SecondaryShootButton); };
            InputActions.PlayerControls.Interact.performed += context => { BindButton(context, InteractButton); };
            InputActions.PlayerControls.Reload.performed += context => { BindButton(context, ReloadButton); };
            InputActions.UI.Pause.performed += context => { BindButton(context, PauseButton); };
            InputActions.PlayerControls.SwitchWeapon.performed += context => { BindButton(context, SwitchWeaponButton); };
            InputActions.PlayerControls.SwitchCharacter.performed += context => { BindButton(context, SwitchCharacterButton); };
            InputActions.PlayerControls.TimeControl.performed += context => { BindButton(context, TimeControlButton); };
            InputActions.PlayerControls.CameraInputActive.performed += context => { BindButton(context, CameraInputActiveButton); };
        }

        /// <summary>
        /// Changes the state of our button based on the input value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imButton"></param>
        protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
        {
            var control = context.control;

            if (control is ButtonControl button)
            {
                if (button.wasPressedThisFrame)
                {
                    imButton.State.ChangeState(MMInput.ButtonStates.ButtonDown);
                }
                if (button.wasReleasedThisFrame)
                {
                    imButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
            }
        }

        public virtual void AimButtonDown() { AimButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void AimButtonPressed() { AimButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void AimButtonUp() { AimButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ThrowButtonDown() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ThrowButtonPressed() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ThrowButtonUp() { ThrowButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void InfoPanelButtonDown() { InfoPanelButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void InfoPanelButtonPressed() { InfoPanelButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void InfoPanelButtonUp() { InfoPanelButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void CameraInputActiveButtonDown() { CameraInputActiveButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void CameraInputActiveButtonPressed() { CameraInputActiveButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void CameraInputActiveButtonUp() { CameraInputActiveButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        protected override void GetInputButtons()
        {
            // useless now
        }

        public override void SetMovement()
        {
            // useless now
        }

        public override void SetSecondaryMovement()
        {
            //do nothing
        }

        protected override void SetShootAxis()
        {
            //do nothing
        }

        protected override void SetCameraRotationAxis()
        {
            // do nothing
        }

        protected virtual void SetCameraScaleAxis()
        {
            // do nothing
        }


        /// <summary>
        /// On enable we enable our input actions
        /// </summary>
        protected virtual void OnEnable()
        {
            InputActions.Enable();
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable()
        {
            InputActions.Disable();
        }

    }
}

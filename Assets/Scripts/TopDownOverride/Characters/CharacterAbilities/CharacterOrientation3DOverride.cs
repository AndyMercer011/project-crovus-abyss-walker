using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.TopDownEngine
{
    public class CharacterOrientation3DOverride : CharacterOrientation3D
    {
        /// <summary>
        /// 人物方向的速度的平滑度
        /// </summary>
        [Tooltip("人物方向的速度的平滑度")]
        public float RemappedSpeedSmooth;

        protected Vector3 _remappedSpeedTarget;

        /// <summary>
        /// Computes the relative speeds
        /// </summary>
        protected override void ComputeRelativeSpeeds()
        {
            if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
            {
                return;
            }

            if (Time.deltaTime != 0f)
            {
                _newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            }

            // relative speed
            if ((_characterHandleWeapon == null) || (_characterHandleWeapon.CurrentWeapon == null))
            {
                _relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
            }
            else
            {
                _relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
            }

            // remapped speed

            float maxSpeed = 0f;
            if (_characterMovement != null)
            {
                maxSpeed = _characterMovement.WalkSpeed;
            }
            if (_characterRun != null)
            {
                maxSpeed = _characterRun.RunSpeed;
            }

            _relativeMaximum = _model.transform.TransformVector(Vector3.one);
            _remappedSpeedTarget.x = MMMaths.Remap(_relativeSpeed.x, 0f, maxSpeed, 0f, _relativeMaximum.x);
            _remappedSpeedTarget.y = MMMaths.Remap(_relativeSpeed.y, 0f, maxSpeed, 0f, _relativeMaximum.y);
            _remappedSpeedTarget.z = MMMaths.Remap(_relativeSpeed.z, 0f, maxSpeed, 0f, _relativeMaximum.z);

            _remappedSpeed = Vector3.Lerp(_remappedSpeed, _remappedSpeedTarget, RemappedSpeedSmooth);

            // relative speed normalized
            _relativeSpeedNormalized = _relativeSpeed.normalized;
            _yRotationOffset = _modelAnglesYLastFrame - ModelAngles.y;

            _yRotationOffsetSmoothed = Mathf.Lerp(_yRotationOffsetSmoothed, _yRotationOffset, RotationOffsetSmoothSpeed * Time.deltaTime);

            // RotationSpeed
            if (Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y) > 1f)
            {
                _rotationSpeed = Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y);
            }
            else
            {
                _rotationSpeed -= Time.time * RotationSpeedResetSpeed;
            }
            if (_rotationSpeed <= 0f)
            {
                _rotationSpeed = 0f;
            }

            _modelAnglesYLastFrame = ModelAngles.y;
            _positionLastFrame = this.transform.position;
        }
    }
}

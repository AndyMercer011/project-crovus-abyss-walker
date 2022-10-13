using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class WeaponLaserSightOverride : WeaponLaserSight
    {
        /// <summary>
        /// 武器激光原点变换
        /// </summary>
        public Transform LaserOriginTransform;

        protected WeaponOverride WeaponOverride;

        protected override void Initialization()
        {
            base.Initialization();
            WeaponOverride = _weapon as WeaponOverride;
        }

        /// <summary>
        /// Every frame we draw our laser  放在FixedUpdate里激光比较流畅
        /// </summary>
     	protected void FixedUpdate()
        {
            if (WeaponOverride.Aiming)
            {
                LaserActive(true);
                ShootLaser();
            }
            else
            {
                LaserActive(false);
            }
        }

        protected override void LateUpdate()
        {

        }

        /// <summary>
        /// Draws the actual laser
        /// </summary>
        public override void ShootLaser()
        {
            if (!PerformRaycast)
            {
                return;
            }

            _thisForward = LaserOriginTransform.forward;

            // our laser will be shot from the weapon's laser origin
            _origin = LaserOriginTransform.position;

            // we cast a ray in front of the weapon to detect an obstacle
            _hit = MMDebug.Raycast3D(_origin, _thisForward, LaserMaxDistance, LaserCollisionMask, Color.red, true);

            // if we've hit something, our destination is the raycast hit
            if (_hit.transform != null)
            {
                _destination = _hit.point;
            }
            // otherwise we just draw our laser in front of our weapon 
            else
            {
                _destination = _origin + _thisForward * LaserMaxDistance;
            }

            if (Time.frameCount <= _initFrame + 1)
            {
                return;
            }

            // we set our laser's line's start and end coordinates
            if (DrawLaser)
            {
                _line.SetPosition(0, _origin);
                _line.SetPosition(1, _destination);
            }
        }
    }
}


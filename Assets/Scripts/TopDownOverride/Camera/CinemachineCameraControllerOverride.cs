using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
    public class CinemachineCameraControllerOverride : CinemachineCameraController
    {
        /// <summary>
        /// Starts following the LevelManager's main player
        /// </summary>
        public override void StartFollowing()
        {
            if (!FollowsAPlayer) { return; }
            FollowsPlayer = true;
#if MM_CINEMACHINE
            _virtualCamera.Follow = TargetCharacter.CameraTarget.transform;
            _virtualCamera.LookAt = TargetCharacter.CameraTarget.transform;
            _virtualCamera.enabled = true;
#endif
        }

        /// <summary>
        /// Stops following any target
        /// </summary>
        public override void StopFollowing()
        {
            if (!FollowsAPlayer) { return; }
            FollowsPlayer = false;
#if MM_CINEMACHINE
            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;
            _virtualCamera.enabled = false;
#endif
        }
    }
}

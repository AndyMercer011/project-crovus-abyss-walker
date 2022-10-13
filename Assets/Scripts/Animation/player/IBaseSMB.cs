using UnityEngine;
using Animation;

namespace Animation.Player
{
    public abstract class IBaseSMB : StateMachineBehaviour
    {
        [HideInInspector]
        protected PlayerController.InfoAccessor _playerInfo;

        public void Init(PlayerController.InfoAccessor infoAccessor)
        {
#if DEBUG
            if (Object.ReferenceEquals(infoAccessor, null))
            {
                Debug.LogError("infoAccessor is null");
            }
#endif
            _playerInfo = infoAccessor;
        }
    }
}
using UnityEngine;

namespace Animation.Player
{
    public class Locomotion : IBaseSMB
    {

        private static float kSMB_MAX_X_SPEED = 0.8f;
        private static float kSMB_MAX_Y_SPEED = 1.0f;

        private static float kSCALE_FACTOR_X = kSMB_MAX_X_SPEED / GameConstant.kMAX_SPEED;

        private static float kSCALE_FACTOR_Y = kSMB_MAX_Y_SPEED / GameConstant.kMAX_SPEED;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            // 我们要初始化在陆地上
            animator.SetBool("Grounded", true);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
#if DEBUG
            if (Object.ReferenceEquals(_playerInfo, null)){
                Debug.LogError("_playerInfo is null");
            }
#endif
            // 我们通过设置_playerInfo 来更新我们走动的动画
            (float vol_x, float vol_y) = _playerInfo.Speed();
            animator.SetFloat("VelX", vol_x * kSCALE_FACTOR_X);
            animator.SetFloat("VelY", vol_y * kSCALE_FACTOR_Y);
        }
    }
}
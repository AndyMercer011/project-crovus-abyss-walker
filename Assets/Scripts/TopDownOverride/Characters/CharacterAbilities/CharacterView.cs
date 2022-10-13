using FoW;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 通过FoG和灯光控制玩家的视野
    /// </summary>
    public class CharacterView : CharacterAbility
    {
        public override string HelpBoxText() { return "控制玩家视野"; }
        /// <summary>
        /// 战争迷雾单位脚本
        /// </summary>
        public FogOfWarUnit fogOfWarUnit;
        /// <summary>
        /// 照向前方的灯光
        /// </summary>
        public Light viewLight;

        [Range(0, 180)]
        [Tooltip("初始视野角度")]
        [SerializeField] private float initialViewAngle;

        [Tooltip("初始视野范围")]
        [SerializeField] private float initialViewDistance;

        [MMReadOnly]
        [SerializeField] private float viewAngle;
        [MMReadOnly]
        [SerializeField] private float viewDistance;
        [SerializeField] private AnimationCurve LightAngleCurve;

        /// <summary>
        /// 视野距离
        /// </summary>
        public float ViewDistance { get => viewDistance; }
        /// <summary>
        /// 视野角度
        /// </summary>
        public float ViewAngle { get => viewAngle; }

        /// <summary>
        /// 初始视野角度
        /// </summary>
        public float InitialViewAngle { get => initialViewAngle; set => initialViewAngle = value; }
        /// <summary>
        /// 初始视野范围
        /// </summary>
        public float InitialViewDistance { get => initialViewDistance; set => initialViewDistance = value; }

        private Coroutine coroutine;

        private void OnValidate()
        {
            if (viewLight != null & fogOfWarUnit != null)
            {
                ChangeViewRange(initialViewAngle, initialViewDistance);
            }
        }

        protected override void Initialization()
        {
            base.Initialization();
            viewAngle = initialViewAngle;
            viewDistance = initialViewDistance;
        }

        /// <summary>
        /// 平滑的改变视野
        /// </summary>
        /// <param name="viewAngle">视野角度</param>
        /// <param name="viewDistance">视野距离</param>
        /// <param name="changeTime">过渡时间</param>
        public void ChangeViewRangeSmooth(float viewAngle, float viewDistance, float changeTime)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(ChangeViewRangeIE(viewAngle, viewDistance, changeTime));
        }

        private IEnumerator ChangeViewRangeIE(float viewAngle, float viewDistance, float changeTime)
        {
            if (changeTime <= 0)
            {
                ChangeViewRange(viewAngle, viewDistance);
                yield break;
            }
            float time = 0;
            float startViewAngle = this.viewAngle;
            float startViewDistance = this.viewDistance;

            while (time <= changeTime)
            {
                time += Time.deltaTime;
                ChangeViewRange(Mathf.Lerp(startViewAngle, viewAngle, time / changeTime),
                    Mathf.Lerp(startViewDistance, viewDistance, time / changeTime));
                yield return null;
            }
            ChangeViewRange(viewAngle, viewDistance);
        }

        /// <summary>
        /// 改变视野
        /// </summary>
        /// <param name="viewAngle">视野角度</param>
        /// <param name="viewDistance">视野距离</param>
        public void ChangeViewRange(float viewAngle, float viewDistance)
        {
            this.viewAngle = viewAngle;
            this.viewDistance = viewDistance;
            if (viewAngle != fogOfWarUnit.angle)
            {
                fogOfWarUnit.angle = Mathf.Clamp(viewAngle, 0, 180);
                viewLight.spotAngle = Mathf.Clamp(LightAngleCurve.Evaluate(viewAngle), 0, 180);
            }

            if (fogOfWarUnit.circleRadius != viewDistance)
            {
                fogOfWarUnit.circleRadius = viewDistance;
                viewLight.range = viewDistance;
            }
        }
    }
}

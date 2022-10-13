using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 爆炸范围伤害
    /// </summary>
    public class ExplosionDamage : DamageOnTouch
    {
        [MMInspectorGroup("Feedbacks")]
        /// 爆炸Feedbacks
        [Tooltip("爆炸Feedbacks")]
        public MMFeedbacks ExplosionFeedback;

        protected Collider[] results = new Collider[20];

        /// <summary>
        /// 爆炸
        /// </summary>
        /// <param name="explosionRadius">爆炸半径</param>
        public virtual void Explosion(float explosionRadius)
        {
            if (explosionRadius > 0)
            {
                int num = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, results,
                    TargetLayerMask.value, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < num; i++)
                {
                    GameObject obj = results[i].gameObject;
                    Vector3 objCenter = results[i].bounds.center;
                    if (Physics.Raycast(transform.position, (objCenter - transform.position).normalized, out RaycastHit raycast,
                        explosionRadius, TargetLayerMask.value, QueryTriggerInteraction.Ignore))
                    {
                        if (obj.name == "Player")
                        {

                        }
                        if (raycast.collider.gameObject == obj)
                        {
                            Colliding(obj);
                        }
                    }
                }
            }
            ExplosionFeedback.transform.rotation = Quaternion.identity;
            ExplosionFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Initializes feedbacks
        /// </summary>
        public override void InitializeFeedbacks()
        {
            base.InitializeFeedbacks();
            if (_initializedFeedbacks) return;
            ExplosionFeedback?.Initialization(this.gameObject);
        }
    }
}


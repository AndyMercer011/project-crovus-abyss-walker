using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMProgressBarOverride : MMProgressBar
    {
        protected Coroutine coroutine;

        /// <summary>
        /// Triggers a camera bump  有新的调用就开启新的协程
        /// </summary>
        public override void Bump()
        {
            bool shouldBump = false;

            if (!_initialized)
            {
                return;
            }

            DetermineDirection();

            if (BumpOnIncrease && (_direction > 0))
            {
                shouldBump = true;
            }

            if (BumpOnDecrease && (_direction < 0))
            {
                shouldBump = true;
            }

            if (BumpScaleOnChange)
            {
                shouldBump = true;
            }

            if (!shouldBump)
            {
                return;
            }

            if (this.gameObject.activeInHierarchy)
            {
                if (Bumping && coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                coroutine = StartCoroutine(BumpCoroutine());
            }

            OnBump?.Invoke();
        }
    }
}

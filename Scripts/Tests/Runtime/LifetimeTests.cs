using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Aikom.FunctionalAnimation.Tests
{
    public class LifetimeTests
    {
        [UnityTest]
        public IEnumerator LifeTime_Test()
        {
            var handle1 = EF.Create(0, 1, 2, Function.Linear);
            var handle2 = EF.Create(0, 1, 2, Function.Linear, TimeControl.Loop)
                .SetLoopLimit(2);
            var handle3 = EF.Create(0, 1, 2, Function.Linear, TimeControl.PingPong)
                .SetLoopLimit(3);
            var nonallocHandle1 = EF.CreateNonAlloc(0, 1, 2, Function.Linear, TimeControl.Loop, 2);
            var infiniteHandle = EF.CreateNonAlloc(0, 1, 2, Function.Linear, TimeControl.Loop, -1);

            yield return null;
            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive);
            Assert.IsTrue(handle2.IsAlive);
            Assert.IsTrue(handle3.IsAlive);
            Assert.IsTrue(EFAnimator.TryGetProcessor<float, FloatInterpolator>(nonallocHandle1, out _));
            Assert.IsTrue(EFAnimator.TryGetProcessor<float, FloatInterpolator>(infiniteHandle, out _));

            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive);
            Assert.IsTrue(!handle2.IsAlive);
            Assert.IsTrue(handle3.IsAlive);
            Assert.IsTrue(!EFAnimator.TryGetProcessor<float, FloatInterpolator>(nonallocHandle1, out _));
            Assert.IsTrue(EFAnimator.TryGetProcessor<float, FloatInterpolator>(infiniteHandle, out _));

            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive);
            Assert.IsTrue(!handle2.IsAlive);
            Assert.IsTrue(!handle3.IsAlive);
            Assert.IsTrue(!EFAnimator.TryGetProcessor<float, FloatInterpolator>(nonallocHandle1, out _));
            Assert.IsTrue(EFAnimator.TryGetProcessor<float, FloatInterpolator>(infiniteHandle, out _));
        }
    }
}


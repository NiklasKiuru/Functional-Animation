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
            var handle1 = EF.Create(0, 1f, new FloatInterpolator(), 2, Function.Linear);
            var handle2 = EF.Create(0, 1f, new FloatInterpolator(), 2, Function.Linear, TimeControl.Loop)
                .SetLoopLimit(2);
            var handle3 = EF.Create(0, 1f, new FloatInterpolator(), 2, Function.Linear, TimeControl.PingPong)
                .SetLoopLimit(3);
            var infiniteHandle = EF.Create(0f, 1f, new FloatInterpolator(), 2, Function.Linear, TimeControl.Loop);

            yield return null;
            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive());
            Assert.IsTrue(handle2.IsAlive());
            Assert.IsTrue(handle3.IsAlive());
            Assert.IsTrue(infiniteHandle.IsAlive());

            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive());
            Assert.IsTrue(!handle2.IsAlive());
            Assert.IsTrue(handle3.IsAlive());
            Assert.IsTrue(infiniteHandle.IsAlive());

            yield return new WaitForSeconds(2);

            Assert.IsTrue(!handle1.IsAlive());
            Assert.IsTrue(!handle2.IsAlive());
            Assert.IsTrue(!handle3.IsAlive());
            Assert.IsTrue(infiniteHandle.IsAlive());
        }
    }
}


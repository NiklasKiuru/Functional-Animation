using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Aikom.FunctionalAnimation.Tests
{
    public class CallbackTests
    {
        [UnityTest]
        public IEnumerator OnCommands_Test()
        {   
            var val = 0f;
            var hasStarted = false;
            var isPaused = false;
            var hasResumed = false;
            var hasCompleted = false;
            var isKilled = false;
            var handle1 = EF.Create(0, 1, 2, Function.Linear)
                .OnUpdate(this, (v) => val = v)
                .OnStart(this, (v) => hasStarted = true)
                .OnPause(this, (v) => isPaused = true)
                .OnResume(this, (v) => hasResumed = true)
                .OnComplete(this, (v) => hasCompleted = true)
                .OnKill(this, (v) => isKilled = true);

            yield return new WaitForSeconds(1);
            // Cant really easily test the start since it will be called once the process has actually started
            Assert.IsTrue(hasStarted);
            handle1.Pause();
            Assert.IsTrue(isPaused);
            // Cannot be equal due to float inaccuracy. Should propably use Time.deltaTime/2 here but 0.001 is good enough
            Assert.IsTrue(val - 0.5f < 0.001f || val - 0.5f > 0.001f);

            yield return new WaitForSeconds(1);
            handle1.Resume();
            Assert.IsTrue(hasResumed);

            yield return new WaitForSeconds(1);
            Assert.IsTrue(hasCompleted);
            Assert.IsTrue(isKilled);
            Assert.That(1 == val);
        }

        [UnityTest]
        public IEnumerator Delay_Test()
        {
            var hasStarted = false;
            var handle1 = EF.Create(0, 1, 2, Function.Linear)
                .Hibernate(1)
                .OnStart(this, (v) => hasStarted = true);
                
            yield return new WaitForSeconds(1);
            // Since start call is execution order dependent there has to be one frame delay to guarantee it
            yield return null;
            Assert.IsTrue(hasStarted);
            handle1.Hibernate(1);

            yield return new WaitForSeconds(1);
            Assert.IsTrue(handle1.IsAlive);
            EFAnimator.TryGetProcessor<float, FloatInterpolator>(handle1.GetIdentifier(), out var process);
            Assert.That(process.Status == ExecutionStatus.Running);
        }

        [UnityTest]
        public IEnumerator Kill_Test()
        {   
            var hasCompleted = false;
            var handle = EF.Create(0, 1, 2, Function.Linear)
                .OnComplete(this, (V) => hasCompleted = true);

            yield return new WaitForSeconds(1);
            handle.Kill();
            Assert.IsFalse(hasCompleted);

        }

    }
}
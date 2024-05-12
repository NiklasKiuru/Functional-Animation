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
                .OnUpdate((v) => val = v)
                .OnStart((v) => hasStarted = true)
                .OnPause((v) => isPaused = true)
                .OnResume((v) => hasResumed = true)
                .OnComplete((v) => hasCompleted = true)
                .OnKill((v) => isKilled = true);

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
                .OnStart((v) => hasStarted = true);
                
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
                .OnComplete((V) => hasCompleted = true);
            var go = new GameObject();
            var handle1 = EF.Create(0,1,0.5f, Function.Linear).OnComplete(go, (v) => go.transform.position = new Vector3(v,v,v));
            UnityEngine.Object.Destroy(go);
            yield return new WaitForSeconds(1);
            handle.Kill();
            Assert.IsFalse(hasCompleted);
        }

        [UnityTest]
        public IEnumerator Inversion_Test()
        {
            var val = 0f;
            var handle = EF.Create(0, 2, 2, Function.Linear)
                .OnUpdate((v) => val = v);
            yield return new WaitForSeconds(1);
            Assert.IsTrue(val > 0.5f);
            handle.Invert();
            yield return new WaitForSeconds(1.1f);
            Assert.AreEqual(0f, val);
        }

        [Test]
        public void FlipValues_Test()
        {
            var from = 0;
            var to = 1;
            var handle = EF.Create(from, to, 1, Function.Linear)
                .FlipValues();
            EFAnimator.TryGetProcessor<float, FloatInterpolator>(handle.GetIdentifier(), out var proc);
            Assert.AreEqual(from, proc.To);
            Assert.AreEqual(to, proc.From);
        }

        [UnityTest]
        public IEnumerator Loop_Test()
        {
            var loopCompleted = false;
            var handle = EF.Create(0, 1, 1, Function.Linear, TimeControl.Loop)
                .OnLoopCompleted((v) => loopCompleted = true);
            yield return new WaitForSeconds(1.1f);
            EFAnimator.TryGetProcessor<float, FloatInterpolator>(handle.GetIdentifier(), out var process);
            Assert.IsTrue((process.PassiveFlags & EventFlags.OnLoopCompleted) == EventFlags.OnLoopCompleted);
            Assert.IsTrue(loopCompleted);
            handle.Kill();
        }

    }
}
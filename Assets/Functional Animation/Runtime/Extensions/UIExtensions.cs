using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation.Extensions
{
    public static class UIExtensions
    {
        #region VisualElement Extensions

        /// <summary>
        /// Cretes a visual element with defined class names
        /// </summary>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static VisualElement CreateElement(params string[] classNames)
        {
            return CreateElement<VisualElement>(classNames);
        }

        /// <summary>
        /// Creates a visual element and adds it to the parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static VisualElement CreateElement(VisualElement parent, params string[] classNames)
        {
            return CreateElement<VisualElement>(parent, classNames);
        }

        /// <summary>
        /// Creates an element of type T with defined class names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static T CreateElement<T>(params string[] classNames) where T : VisualElement, new()
        {
            var element = new T();
            foreach (var className in classNames)
            {
                element.AddToClassList(className);
            }
            return element;
        }

        /// <summary>
        /// Creates an element of type T with defined class names and adds it to the parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static T CreateElement<T>(VisualElement parent, params string[] classNames) where T : VisualElement, new()
        {
            var element = CreateElement<T>(classNames);
            parent.Add(element);
            return element;
        }

        /// <summary>
        /// Gets the root element in the hierarchy by climbing it as far up as possible
        /// This might return the panel settings object in some cases
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static VisualElement GetRoot(this VisualElement element)
        {
            var parent = element.hierarchy.parent;
            while (parent != null)
            {
                var newParent = parent.hierarchy.parent;
                if (newParent == null)
                    return parent;
                parent = newParent;
            }
            return parent;
        }

        /// <summary>
        /// Single use position transition animation
        /// </summary>
        /// <param name="element"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ProcessId AnimateOnce(this VisualElement element, Vector3 offset, float duration, Function func)
        {
            var currentPos = element.transform.matrix.GetPosition();
            var proc = EF.CreateNonAlloc(currentPos, currentPos + offset, duration, func, TimeControl.PlayOnce, 1, 
                (v) => element.transform.position = v);
            return proc;
        }

        /// <summary>
        /// Animates the elements position with mouse events
        /// </summary>
        /// <typeparam name="T">Activation callback type</typeparam>
        /// <typeparam name="D">Reset callback type</typeparam>
        /// <param name="element"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> AnimatePosition<T, D>(this VisualElement element, Vector3 offset, float duration, Function func)
            where T : MouseEventBase<T>, new()
            where D : MouseEventBase<D>, new()
        {
            var currentPos = element.transform.matrix.GetPosition();
            var handle = EF.Create(currentPos, currentPos + offset, duration, func)
                .OnUpdate(SetPosition)
                .Pause();
            // Entry
            element.RegisterCallback<T>((evt) =>
            {
                if (handle.IsAlive)
                    handle.Resume();
                else
                    handle.Restart().OnUpdate(SetPosition);
            });
            // Exit
            element.RegisterCallback<D>((evt) =>
            {
                handle.Kill();
                element.transform.position = currentPos;
            });

            void SetPosition(float3 pos) => element.transform.position = pos;

            return handle;
        }

        #endregion

        #region Struct shortcuts

        /// <summary>
        /// Quicker way of creating an axis uniform style scale
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static StyleScale ScaleFromFloat(float f)
        {
            return new StyleScale(new Scale(new Vector3(f, f)));
        }

        #endregion
    }
}


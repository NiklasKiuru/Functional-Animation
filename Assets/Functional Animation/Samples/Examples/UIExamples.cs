using System.Collections;
using UnityEngine;
using Aikom.FunctionalAnimation;
using UnityEngine.UIElements;
using Aikom.FunctionalAnimation.Extensions;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// An example of how to animate UI elements
/// </summary>
public class UIExamples : MonoBehaviour
{
    [SerializeField] private UIDocument _mainDoc;
    [SerializeField] private List<PanelEaseSetttings> _settings = new List<PanelEaseSetttings>();
    [SerializeField] private float _fizzleAmplitude;
    [SerializeField] private GraphData _test;
    [SerializeField] private FunctionAlias _alias;

    private void Start()
    {
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        yield return null;

        var root = _mainDoc.rootVisualElement;
        root.style.alignItems = Align.Center;
        var mainContainer = UIExtensions.CreateElement(root);
        mainContainer.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
        mainContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        mainContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        mainContainer.style.alignItems = new StyleEnum<Align>(Align.Center);
        mainContainer.style.justifyContent = Justify.SpaceAround;

        for(int i = 0; i < _settings.Count; i++)
        {   
            // Create the sub panel element
            var setting = _settings[i];
            //var subPanel = CreateSubContainer("Element: " + i);

            // Create an animation handle for the element
            // This handle handles the restart, resume and kill automatically
            //setting.Handle = subPanel.AnimatePosition<MouseEnterEvent, MouseLeaveEvent>(setting.Offset, setting.Duration, setting.Ease);
            _settings[i] = setting;
        }

        //var shizzleElement = CreateSubContainer("Fizzle element:");
        //var pos = shizzleElement.transform.matrix.GetPosition();
        //var animHandle = EF.Create(pos.x, pos.x + _fizzleAmplitude,  1, _test)
        //    .OnUpdate(this, (v) => shizzleElement.transform.position = new Vector3(v, pos.y, 0))
        //    .Pause();
        //shizzleElement.RegisterCallback<MouseDownEvent>((evt) => 
        //{
        //    if (animHandle.IsAlive())
        //        animHandle.Resume();
        //    else
        //    {   
        //        animHandle
        //        .Restart()
        //        .OnUpdate(this, (v) => shizzleElement.transform.position = new Vector3(v, pos.y, 0));
        //    }
                
        //});

        //VisualElement CreateSubContainer(string header)
        //{
        //    var headerElement = UIExtensions.CreateElement<Label>(mainContainer);
        //    headerElement.text = header;
        //    headerElement.style.width = new StyleLength(new Length(360, LengthUnit.Pixel));
        //    headerElement.style.height = new StyleLength(new Length(120, LengthUnit.Pixel));
        //    headerElement.style.color = Color.white;
        //    headerElement.style.fontSize = 40;
        //    headerElement.style.unityTextAlign = TextAnchor.MiddleCenter;

        //    var subPanel = UIExtensions.CreateElement(mainContainer);
        //    subPanel.style.width = new StyleLength(new Length(360, LengthUnit.Pixel));
        //    subPanel.style.height = new StyleLength(new Length(120, LengthUnit.Pixel));
        //    subPanel.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        //    return subPanel;
        //}
    }

    [Serializable]
    private struct PanelEaseSetttings
    {
        public Function Ease;
        public float Duration;
        public Vector3 Offset;

        [HideInInspector] public IInterpolatorHandle<float3, Vector3Interpolator> Handle;
    }
}

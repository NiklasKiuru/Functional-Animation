using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class LegendEntryElement : VisualElement
    {
        private VisualElement _color;
        private Label _labelElement;

        public string Name { get => _labelElement.text; set => _labelElement.text = value; }
        public Color Color { get => _color.style.backgroundColor.value; set => _color.style.backgroundColor = value; }

        public LegendEntryElement(string name, Color color)
        { 
            style.flexDirection = FlexDirection.Row;
            style.marginBottom = new StyleLength(new Length(2f, LengthUnit.Pixel));
            style.marginTop = new StyleLength(new Length(2f, LengthUnit.Pixel));
            style.alignItems = Align.Center;
            style.width = new StyleLength(new Length(100, LengthUnit.Percent));

            var colorContainer = new VisualElement();
            colorContainer.style.backgroundColor = new StyleColor(color);
            colorContainer.style.width = new StyleLength(new Length(10f, LengthUnit.Pixel));
            colorContainer.style.height = new StyleLength(new Length(10f, LengthUnit.Pixel));

            var label = new Label(name);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.marginLeft = new StyleLength(new Length(3f, LengthUnit.Pixel));

            Add(colorContainer);
            Add(label);
        }
    }
}


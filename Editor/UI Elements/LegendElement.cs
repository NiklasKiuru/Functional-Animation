using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class LegendElement : VisualElement
    {
        private Label _label;
        private List<LegendEntryElement> _entries = new List<LegendEntryElement>();

        public string Header
        {
            get => _label.text;
            set => _label.text = value;
        }

        public LegendElement(string legendName)
        {
            style.marginTop = new StyleLength(new Length(30f, LengthUnit.Pixel));
            style.marginLeft = new StyleLength(new Length(5f, LengthUnit.Pixel));
            style.maxWidth = new StyleLength(new Length(140f, LengthUnit.Pixel));

            _label = new Label(legendName);
            Add(_label);
        }

        public void RemoveLegend()
        {

        }

        public void AddLegend()
        {

        }

        public void ChangeLegend()
        {

        }

        public void SetAll(string[] names, Func<int, Color> getColor)
        {   
            // Remove old
            for(int i = 0; i < _entries.Count; i++)
            {
                Remove(_entries[i]);
                _entries.RemoveAt(i);
            }

            for(int i = 0; i < names.Length; i++)
            {
                var entry = new LegendEntryElement(names[i], getColor(i));
                _entries.Add(entry);
                Add(entry);
            }
        }
    }
}

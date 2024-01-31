using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class LegendElement<T> : VisualElement where T : System.Enum
    {
        private Label _label;
        private List<LegendEntryElement<T>> _entries = new List<LegendEntryElement<T>>();

        public string LegendName
        {
            get => _label.text;
            set => _label.text = value;
        }

        public LegendElement(string legendName)
        {
            _label = new Label(legendName);
            Add(_label);
        }
    }
}

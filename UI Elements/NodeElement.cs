using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.UI
{
    public class NodeElement : VisualElement
    {   
        private Color _cachedColor;
        public int Index { get; private set; }
        public Axis Axis { get; private set; }

        public NodeElement()
        {
            style.backgroundColor = new StyleColor(Color.white);
            style.width = new StyleLength(new Length(7f, LengthUnit.Pixel));
            style.height = new StyleLength(new Length(7f, LengthUnit.Pixel));
            style.flexGrow = 0;
            style.position = Position.Absolute;
            style.visibility = Visibility.Hidden;
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        ~NodeElement()
        {
            UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.backgroundColor = _cachedColor;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _cachedColor = style.backgroundColor.value;
            style.backgroundColor = new StyleColor(Color.yellow);
        }

        public void Activate(int index, Color color, Axis axis)
        {
            Index = index;
            style.visibility = Visibility.Visible;
            style.backgroundColor = new StyleColor(color);
            Axis = axis;
        }
    }
}


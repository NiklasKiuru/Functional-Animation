using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Aikom.FunctionalAnimation.Editor
{
    public class NodeElement : VisualElement
    {   
        private Color _cachedColor;
        private Vector2 _cachedData;
        private Vector2Field _dataDisplay;

        public int Index { get; private set; }
        public int GraphIndex { get; private set; }

        public NodeElement()
        {
            style.backgroundColor = new StyleColor(Color.white);
            style.width = new StyleLength(new Length(8f, LengthUnit.Pixel));
            style.height = new StyleLength(new Length(8f, LengthUnit.Pixel));
            style.flexGrow = 0;
            style.position = Position.Absolute;
            style.visibility = Visibility.Hidden;
            style.borderBottomLeftRadius = new StyleLength(4f);
            style.borderBottomRightRadius = new StyleLength(4f);
            style.borderTopLeftRadius = new StyleLength(4f);
            style.borderTopRightRadius = new StyleLength(4f);
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            _dataDisplay = new Vector2Field();
            var child = _dataDisplay.Children().First();
            child.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            child.Query<Label>().ForEach(x => x.style.marginLeft = new StyleLength(0f));
            _dataDisplay.labelElement.style.minWidth = new StyleLength(0f);
            _dataDisplay.style.paddingLeft = new StyleLength(15f);
            _dataDisplay.style.width = new StyleLength(new Length(120f, LengthUnit.Pixel));
            _dataDisplay.style.height = new StyleLength(new Length(40f, LengthUnit.Pixel));
            _dataDisplay.pickingMode = PickingMode.Ignore;
            _dataDisplay.style.visibility = Visibility.Hidden;

            Add(_dataDisplay);
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
            _dataDisplay.style.visibility = Visibility.Hidden;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _cachedColor = style.backgroundColor.value;
            style.backgroundColor = new StyleColor(Color.yellow);
            _dataDisplay.style.visibility = Visibility.Visible;
            _dataDisplay.SetValueWithoutNotify(_cachedData);
        }

        public void Activate(int index, Color color, int graphIndex, Vector2 values)
        {
            Index = index;
            style.visibility = Visibility.Visible;
            style.backgroundColor = new StyleColor(color);
            GraphIndex = graphIndex;
            _cachedData = values;
            _dataDisplay.SetValueWithoutNotify(values);
        }

        public void ShowData(Vector2 data, bool show)
        {
            if (!show)
            {
                _dataDisplay.style.visibility = Visibility.Hidden;
                _dataDisplay.SetValueWithoutNotify(_cachedData);
            }

            else
            {
                _dataDisplay.style.visibility = Visibility.Visible;
                _cachedData = data;
                _dataDisplay.SetValueWithoutNotify(data);
            }
        }
    }
}


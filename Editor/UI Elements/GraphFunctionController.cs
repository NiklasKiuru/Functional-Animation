using Aikom.FunctionalAnimation.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class GraphFunctionController<T> : VisualElement
    {
        /// <summary>
        /// Selection field for the GraphFunctionController
        /// </summary>
        public class FunctionSelectionField : PopupField<FunctionAlias> 
        {   
            /// <summary>
            /// Represents the position of this field in the source container
            /// </summary>
            public int Index { get; private set; }

            /// <summary>
            /// Removes this index from the source container once clicked
            /// </summary>
            public Button RemoveButton { get; private set; }

            /// <summary>
            /// Parent controller
            /// </summary>
            public IGraphController Parent { get; set; }

            /// <summary>
            /// Fired when the remove button is clicked
            /// </summary>
            public event Action OnFunctionRemovedInUI;

            /// <summary>
            /// Creates a new FunctionSelectionField
            /// </summary>
            /// <param name="label"></param>
            /// <param name="defaultValue"></param>
            public FunctionSelectionField(string label, int index, FunctionAlias defaultValue)
            {
                SetValueWithoutNotify(defaultValue);
                choices = BurstFunctionCache.GetDefinitions().ToList();
                formatListItemCallback = (a) => a.Value;
                formatSelectedValueCallback = (a) => a.Value;
                Index = index;
                labelElement.text = label;

                style.flexDirection = FlexDirection.Row;
                labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                labelElement.style.minWidth = new StyleLength(new Length(20f, LengthUnit.Pixel));
                RemoveButton = new Button(OnRemoveButtonClicked);
                RemoveButton.text = "Remove";
                Add(RemoveButton);
            }

            /// <summary>
            /// Removes a function in this field's index from the source container
            /// </summary>
            public void OnRemoveButtonClicked()
            {   
                var source = Parent.GetSource();
                if(source.RemoveFunction(Index))
                    OnFunctionRemovedInUI?.Invoke();

            }
        }
    }
}


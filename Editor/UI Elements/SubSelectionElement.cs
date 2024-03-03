using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SubSelectionElement<T> : VisualElement
{   
    private Label _labelElement;
    public T Value { get; private set; }
    public Label LabelElement { get => _labelElement; }

    public SubSelectionElement(T value, string label)
    {
        Value = value;
        style.flexGrow = 1;
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.justifyContent = Justify.Center;
        style.marginBottom = 5;
        style.marginTop = 5;
        style.marginLeft = 5;
        style.marginRight = 5;

        style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
        style.borderBottomWidth = 1;
        style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
        style.borderTopWidth = 1;
        style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
        style.borderLeftWidth = 1;
        style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);
        style.borderRightWidth = 1;

        RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

        // Label
        var labelElement = new Label(label);
        labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
        labelElement.style.flexGrow = 1;
        Add(labelElement);
        _labelElement = labelElement;
    }

    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        _labelElement.style.color = Color.white;
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        _labelElement.style.color = Color.yellow;        
    }
}

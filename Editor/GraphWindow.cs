using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.UI;
using System;
using UnityEngine.Profiling;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// This script shows the animator graph in the editor
    /// The UI building here is messy at best since I did not want to spend too much time on it
    /// </summary>
    internal class GraphWindow : EditorWindow
    {
        private TransformAnimator _animator;
        private RenderElement _renderElement;

        [MenuItem("Examples/My Editor Window")]
        public static void Init()
        {
            var window = GetWindow<GraphWindow>();
            window.titleContent = new GUIContent("Graph window");
        }

        private void OnEnable()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GraphMaterial.mat");
            _renderElement = new RenderElement(material);
            _renderElement.style.width = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.height = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            _renderElement.style.borderLeftWidth = new StyleFloat(2);
            _renderElement.style.borderBottomWidth = new StyleFloat(2);
            _renderElement.style.borderLeftColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _renderElement.style.borderBottomColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));

            var sideBar = new VisualElement();
            sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

            var header = new Label("Properties");
            header.style.fontSize = 15;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            sideBar.Add(header);

            root.Add(sideBar);
            root.Add(_renderElement);
            Selection.selectionChanged += SetAnimatior;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= SetAnimatior;
        }

        private void SetAnimatior()
        {
            var obj = Selection.activeGameObject;
            if (obj != null && obj.TryGetComponent<TransformAnimator>(out var anim))
            {
                _renderElement.Animator = anim;
                //_renderElement.DrawGraph();
            }
                
            else
                _renderElement.Animator = null;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Visual element that draws the animation graph
        /// </summary>
        private class RenderElement : ImmediateModeElement
        {   
            private Material _mat;
            private int _sampleAmount = 200;
            private int _gridLines = 10;

            public TransformAnimator Animator { get; set; }

            public RenderElement(Material lineMaterial)
            {
                _mat = lineMaterial;
            }

            protected override void ImmediateRepaint()
            {
                DrawGraph();
            }

            public void DrawGraph()
            {
                if (Animator == null)
                    return;
                var heightInc = resolvedStyle.height / _sampleAmount;
                var widthInc = resolvedStyle.width / _sampleAmount;

                GL.PushMatrix();
                _mat.SetPass(0);
                GL.LoadOrtho();

                GL.Begin(GL.LINES);
                GL.Color(new Color(1,1,1,0.2f));
                for(int j = 1; j <= _gridLines; j++)
                {
                    float x = j * (1f / _gridLines);
                    DrawVertex(x, 0);
                    DrawVertex(x, 1);
                    DrawVertex(0, x);
                    DrawVertex(1, x);
                }

                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 0:
                            GL.Color(Color.red);
                            break;
                        case 1:
                            GL.Color(Color.blue);
                            break;
                        case 2:
                            GL.Color(Color.green);
                            break;
                    }
                    var container = Animator.Container[i];
                    if (container == null || !container.Animate)
                        continue;
                    var func = container.FunctionConstructor.Generate();
                    float sampleInc = 1f / _sampleAmount;

                    if (!Animator.SyncAll)
                    {
                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {
                            DrawVertexSingle(sample);
                            DrawVertexSingle(sample + 1);
                        }
                    }
                    else
                    {
                        float duration = container.TrimBack - container.TrimFront;
                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {   
                            DrawVertexTrimmed(sample);
                            DrawVertexTrimmed(sample + 1);
                        }

                        void DrawVertexTrimmed(int index)
                        {
                            float x = index * sampleInc;
                            float y;
                            if (x <= container.TrimFront)
                                y = func.Invoke(0);
                            else if (x >= container.TrimBack)
                                y = func.Invoke(1);
                            else
                                y = func.Invoke((x - container.TrimFront) / duration);
                            DrawVertex(x, y);
                        }
                    }

                    void DrawVertexSingle(int index)
                    {
                        float x = index * sampleInc;
                        DrawVertex(x, func(x));
                    }
                }

                if (Application.isPlaying)
                {
                    GL.Color(Color.white);
                    DrawVertex(Animator.Timer.Time, 0);
                    DrawVertex(Animator.Timer.Time, 1);
                }

                GL.End();
                GL.PopMatrix();
            }



            public void DrawVertex(float x, float y) => GL.Vertex3((x * 0.75f) + 0.2f, (y * 0.7f) + 0.2f, 0);
        }

    }
}


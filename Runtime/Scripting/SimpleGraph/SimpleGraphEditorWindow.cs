using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace SimpleGraph
{
    public class SimpleGraphEditorWindow : EditorWindow
    {
        public static SimpleGraphEditorWindow ShowWindow(string name, SimpleGraphModel model)
        {
            if (model == null)
            {
                return null;
            }

            var window = Resources.FindObjectsOfTypeAll<SimpleGraphEditorWindow>().FirstOrDefault(window => window.IsShowingGraph(model));
            if (window != null)
            {
                window.Focus();
            }
            else
            {
                window = CreateWindow<SimpleGraphEditorWindow>(name, typeof(SceneView));
                window.ShowGraph(model);
            }
            return window;
        }

        private SimpleGraphModel model;
        public SimpleGraphModel Model => model;

        private SimpleGraphView view;
        public SimpleGraphView View => view;

        private void OnEnable()
        {
            if (model != null)
            {
                ShowGraph(model);
            }
        }

        public void ShowGraph(SimpleGraphModel model)
        {
            this.model = model;

            if (view == null)
            {
                view = new SimpleGraphView();
                rootVisualElement.Add(view);

                var toolbar = new Toolbar();
                rootVisualElement.Add(toolbar);

                var saveButton = new Button(() => model = view.Save()) { text = "Save" };
                toolbar.Add(saveButton);
            }

            view.Load(model);
        }

        public bool IsShowingGraph(SimpleGraphModel model)
        {
            return this.model == model;
        }
    }
}

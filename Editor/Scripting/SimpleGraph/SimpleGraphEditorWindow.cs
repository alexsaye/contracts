using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleGraph.Editor
{
    public class SimpleGraphEditorWindow : EditorWindow
    {
        public static SimpleGraphEditorWindow ShowWindow(SimpleGraphBehaviour graph)
        {
            if (graph == null)
            {
                return null;
            }

            var window = Resources.FindObjectsOfTypeAll<SimpleGraphEditorWindow>().FirstOrDefault(window => window.graph == graph);
            if (window != null)
            {
                window.Focus();
            }
            else
            {
                window = CreateWindow<SimpleGraphEditorWindow>($"{graph.GetType().Name} ({graph.name})", typeof(SceneView));
                window.ShowGraph(graph);
            }
            return window;
        }

        private SimpleGraphBehaviour graph;

        private void OnEnable()
        {
            if (graph != null)
            {
                ShowGraph(graph);
            }
        }

        private void ShowGraph(SimpleGraphBehaviour graph)
        {
            rootVisualElement.Clear();
            this.graph = graph;
            var serializedGraph = new SerializedObject(graph);
            var serializedModel = serializedGraph.FindProperty(nameof(SimpleGraphBehaviour.Model));
            var view = new SimpleGraphView(serializedModel);
            rootVisualElement.Add(view);
        }
    }
}

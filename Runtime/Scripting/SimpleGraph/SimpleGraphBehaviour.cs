using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraphBehaviour : MonoBehaviour
    {
        [SerializeReference]
        private SimpleGraphModel model;

        /// <summary>
        /// The nodes stored in the model.
        /// </summary>
        public IEnumerable<SimpleGraphNodeModel> Nodes => model.Nodes;

        /// <summary>
        /// The edges stored in the model.
        /// </summary>
        public IEnumerable<SimpleGraphEdgeModel> Edges => model.Edges;

        /// <summary>
        /// Create a default model for when the behaviour is initialised or reset.
        /// </summary>
        protected abstract SimpleGraphModel CreateDefaultModel();

        protected virtual void OnValidate()
        {
            if (model == null)
            {
                Debug.Log("Validate: Creating default model...");
                model = CreateDefaultModel();
            }
        }

        protected virtual void Reset()
        {
            Debug.Log("Reset: Creating default model...");
            model = CreateDefaultModel();
        }
    }

    class SimpleGraphScriptableObjectPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is not SimpleGraphBehaviour graph)
            {
                return false;
            }

            var window = SimpleGraphEditorWindow.ShowWindow(graph);
            if (window == null)
            {
                return false;
            }

            return true;
        }
    }

    [CustomEditor(typeof(SimpleGraphBehaviour), true)]
    class SimpleGraphEditor : Editor
    {
        private SimpleGraphBehaviour graph => (SimpleGraphBehaviour)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Graph"))
            {
                SimpleGraphEditorWindow.ShowWindow(graph);
            }
        }
    }

    class SimpleGraphEditorWindow : EditorWindow
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
        private SimpleGraphView view;

        private void OnEnable()
        {
            if (graph != null)
            {
                ShowGraph(graph);
            }
        }

        private void ShowGraph(SimpleGraphBehaviour graph)
        {
            this.graph = graph;

            if (view != null)
            {
                rootVisualElement.Remove(view);
            }

            var serializedGraph = new SerializedObject(graph);
            view = new SimpleGraphView(serializedGraph);
            rootVisualElement.Add(view);
        }
    }
}
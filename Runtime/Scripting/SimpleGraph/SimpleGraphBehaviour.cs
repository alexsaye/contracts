using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraphBehaviour : MonoBehaviour
    {
        [SerializeReference]
        protected SimpleGraphModel model;

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

        [CustomEditor(typeof(SimpleGraphBehaviour), true)]
        class SimpleGraphBehaviourEditor : Editor
        {
            private SimpleGraphBehaviour graph => (SimpleGraphBehaviour)target;

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                if (GUILayout.Button("Open Graph"))
                {
                    var serializedGraph = new SerializedObject(graph);
                    var serializedModel = serializedGraph.FindProperty(nameof(model));
                    SimpleGraphEditorWindow.ShowWindow(serializedModel, graph.GetType());
                }
            }
        }
    }

    class SimpleGraphEditorWindow : EditorWindow
    {
        public static SimpleGraphEditorWindow ShowWindow(SerializedProperty serializedGraphModel, Type contextualType)
        {
            if (serializedGraphModel == null)
            {
                return null;
            }

            var window = Resources.FindObjectsOfTypeAll<SimpleGraphEditorWindow>().FirstOrDefault(window => window.view.IsShowingModel(serializedGraphModel));
            if (window != null)
            {
                window.Focus();
            }
            else
            {
                window = CreateWindow<SimpleGraphEditorWindow>($"{contextualType} ({serializedGraphModel.serializedObject.targetObject.name})", typeof(SceneView));
                window.ShowGraph(serializedGraphModel, contextualType);
            }
            return window;
        }

        private SimpleGraphView view;

        private void ShowGraph(SerializedProperty serializedGraphModel, Type contextualType)
        {
            if (view != null)
            {
                rootVisualElement.Remove(view);
            }
            view = new SimpleGraphView(serializedGraphModel, contextualType);
            rootVisualElement.Add(view);
        }
    }
}
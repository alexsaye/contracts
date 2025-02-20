using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;

namespace Contracts.Scripting.Graph
{
    public class ScriptableGraphEditorWindow : EditorWindow
    {
        public ScriptableGraph Graph { get; private set; }
        
        public ScriptableGraphView View { get; private set; }

        private Toolbar Toolbar;
    
        [MenuItem("Window/Contract Graph Editor")]
        public static void OpenWindow()
        {
            var inspectorType = typeof(SceneView);
            EditorWindow window = GetWindow<GraphViewEditorWindow>(new Type[] { inspectorType });
        }
        
        public void ShowGraph(ScriptableGraph graph)
        {
            Graph = graph;
            if (View != null)
            {
                rootVisualElement.Remove(View);
            }
            View = new ScriptableGraphView(Graph);
            rootVisualElement.Add(View);

            if (Toolbar != null)
            {
                rootVisualElement.Remove(Toolbar);
            }

            Toolbar = new Toolbar();
            var saveButton = new Button(() => View.SaveGraph()) { text = "Save" };
            Toolbar.Add(saveButton);
            rootVisualElement.Add(Toolbar);
        }
        
        private void OnEnable()
        {
            if (Graph != null)
            {
                ShowGraph(Graph);
            }
        }

        private void OnDisable()
        {
            if (View != null)
            {
                rootVisualElement.Remove(View);
                rootVisualElement.Remove(Toolbar);
            }
        }

        private void AddToolbar()
        {

        }

        private void RemoveToolbar()
        {

        }
    }

    public class ContractGraphAssetProcessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ScriptableGraph graph)
            {
                var window = Resources.FindObjectsOfTypeAll<ScriptableGraphEditorWindow>().FirstOrDefault(window => window.Graph == graph);
                if (window != null)
                {
                    window.Focus();
                    return true;
                }

                window = EditorWindow.CreateWindow<ScriptableGraphEditorWindow>(graph.name, typeof(SceneView));
                window.ShowGraph(graph);
                return true;
            }
            return false;
        }
    }
}

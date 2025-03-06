using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace Contracts.Scripting.Graph
{
    public class ScriptableGraphEditorWindow : EditorWindow
    {
        private ScriptableGraph graph;
        private ScriptableGraphView view;

        private void OnEnable()
        {
            if (graph != null)
            {
                ShowGraph(graph);
            }
        }

        public void ShowGraph(ScriptableGraph graph)
        {
            this.graph = graph;

            if (view == null)
            {
                view = new ScriptableGraphView();
                rootVisualElement.Add(view);

                var toolbar = new Toolbar();
                rootVisualElement.Add(toolbar);

                // Allow the graph to be saved from the view (the graph will be assigned when the window is opened).
                var saveButton = new Button(() => graph.Model = view.Save()) { text = "Save" };
                toolbar.Add(saveButton);
            }

            view.Load(graph.Model, graph.GetType());
        }

        public bool IsShowingGraph(ScriptableGraph graph)
        {
            return this.graph == graph;
        }
    }

    public class ContractGraphAssetProcessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)//
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ScriptableGraph graph)
            {
                var window = Resources.FindObjectsOfTypeAll<ScriptableGraphEditorWindow>().FirstOrDefault(window => window.IsShowingGraph(graph));
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

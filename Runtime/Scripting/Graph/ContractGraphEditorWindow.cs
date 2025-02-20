using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace ContractGraph
{
    public class ContractGraphEditorWindow : EditorWindow
    {
        public ContractBaseGraph Graph { get; private set; }
        
        public ContractGraphView View { get; private set; }

        private Toolbar Toolbar;
    
        [MenuItem("Window/Contract Graph Editor")]
        public static void OpenWindow()
        {
            var inspectorType = typeof(SceneView);
            EditorWindow window = EditorWindow.GetWindow<GraphViewEditorWindow>(new Type[] { inspectorType });
        }
        
        public void ShowGraph(ContractBaseGraph graph)
        {
            Debug.Log("Showign graph");
            Graph = graph;
            if (View != null)
            {
                rootVisualElement.Remove(View);
            }
            View = new ContractGraphView(Graph);
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
            if (obj is ContractBaseGraph graph)
            {
                ContractGraphEditorWindow window = EditorWindow.GetWindow<ContractGraphEditorWindow>();
                window.ShowGraph(graph);
                return true;
            }
            return false;
        }
    }
}

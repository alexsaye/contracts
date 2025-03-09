using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraphScriptableObject : ScriptableObject
    {
        public SimpleGraphModel Model { get; protected set; }
    }

    class SimpleGraphScriptableObjectPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is not SimpleGraphScriptableObject graph)
            {
                return false;
            }

            var window = SimpleGraphEditorWindow.ShowWindow(graph.name, graph.Model);
            if (window == null)
            {
                return false;
            }

            window.View.Contextualise(graph.GetType());
            return true;
        }
    }

    [CustomEditor(typeof(SimpleGraphScriptableObject), true)]
    public class SimpleGraphScriptableObjectEditor : Editor
    {
        private SimpleGraphScriptableObject graph => (SimpleGraphScriptableObject)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Graph"))
            {
                var window = SimpleGraphEditorWindow.ShowWindow(graph.name, graph.Model);
                window.View.Contextualise(graph.GetType());
            }
        }
    }
}
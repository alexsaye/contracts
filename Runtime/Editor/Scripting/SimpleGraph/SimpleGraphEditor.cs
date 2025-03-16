using UnityEditor;
using UnityEngine;

namespace SimpleGraph.Editor
{
    [CustomEditor(typeof(SimpleGraph), true)]
    public class SimpleGraphEditor : UnityEditor.Editor
    {
        private SimpleGraph graph => (SimpleGraph)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Graph"))
            {
                SimpleGraphEditorWindow.ShowWindow(graph);
            }
        }
    }
}

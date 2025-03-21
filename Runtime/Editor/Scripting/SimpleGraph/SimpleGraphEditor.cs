using UnityEditor;
using UnityEngine;

namespace SimpleGraph.Editor
{
    [CustomEditor(typeof(SimpleGraphBehaviour), true)]
    public class SimpleGraphEditor : UnityEditor.Editor
    {
        private SimpleGraphBehaviour graph => (SimpleGraphBehaviour)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Graph"))
            {
                SimpleGraphEditorWindow.ShowWindow(graph);
            }
        }
    }
}

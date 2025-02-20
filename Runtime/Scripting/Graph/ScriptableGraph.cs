using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraph : ScriptableObject
    {
        [HideInInspector]
        public List<NodeSaveData> Nodes = new List<NodeSaveData>();

        [HideInInspector]
        public List<EdgeSaveData> Edges = new List<EdgeSaveData>();
        
        public void Save(IEnumerable<GraphElement> elements)
        {
            Nodes.Clear();
            Edges.Clear();

            foreach (var element in elements)
            {
                if (element is ScriptableGraphNode node)
                {
                    Nodes.Add(new NodeSaveData(node));;
                }
                else if (element is Edge edge)
                {
                    Edges.Add(new EdgeSaveData(edge));
                }
            }
        }
    }
}

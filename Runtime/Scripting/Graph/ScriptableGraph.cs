using UnityEngine;
using System.Collections.Generic;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraph : ScriptableObject
    {
        [HideInInspector]
        public List<NodeSaveData> Nodes = new();

        [HideInInspector]
        public List<EdgeSaveData> Edges = new();
    }
}

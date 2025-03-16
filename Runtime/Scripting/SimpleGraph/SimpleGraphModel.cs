using System;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class SimpleGraphModel
    {
        [SerializeReference]
        public SimpleGraphNodeModel[] Nodes;

        [SerializeField]
        public SimpleGraphEdgeModel[] Edges;
    }
}

using System;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class SimpleGraphModel
    {
        [SerializeReference]
        public SimpleGraphNode[] Nodes;

        [SerializeField]
        public SimpleGraphEdge[] Edges;
    }
}

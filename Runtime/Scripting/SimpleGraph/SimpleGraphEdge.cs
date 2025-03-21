using System;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class SimpleGraphEdge
    {
        [SerializeField]
        private string outputNodeGuid;
        public string OutputNodeGuid => outputNodeGuid;

        [SerializeField]
        private string inputNodeGuid;
        public string InputNodeGuid => inputNodeGuid;

        [SerializeField]
        private string outputPortName;
        public string OutputPortName => outputPortName;

        [SerializeField]
        private string inputPortName;
        public string InputPortName => inputPortName;

        public SimpleGraphEdge(SimpleGraphNode outputNode, SimpleGraphNode inputNode, string outputPortName, string inputPortName)
            : this(outputNode.Guid, inputNode.Guid, outputPortName, inputPortName)
        {
        }

        public SimpleGraphEdge(string outputNodeGuid, string inputNodeGuid, string outputPortName, string inputPortName)
        {
            this.outputNodeGuid = outputNodeGuid;
            this.inputNodeGuid = inputNodeGuid;
            this.outputPortName = outputPortName;
            this.inputPortName = inputPortName;
        }
    }
}

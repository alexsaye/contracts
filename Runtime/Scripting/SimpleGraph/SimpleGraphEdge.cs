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

        public SimpleGraphEdge(SimpleGraphNode outputNode, string outputPortName, SimpleGraphNode inputNode, string inputPortName)
            : this(outputNode.Guid, outputPortName, inputNode.Guid, inputPortName)
        {
        }

        public SimpleGraphEdge(string outputNodeGuid, string outputPortName, string inputNodeGuid, string inputPortName)
        {
            this.outputNodeGuid = outputNodeGuid;
            this.outputPortName = outputPortName;
            this.inputNodeGuid = inputNodeGuid;
            this.inputPortName = inputPortName;
        }
    }
}

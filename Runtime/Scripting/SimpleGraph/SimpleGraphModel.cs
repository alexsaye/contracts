using System;
using System.Collections.Generic;
using System.Linq;
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

    [Serializable]
    public abstract class SimpleGraphNodeModel
    {
        [SerializeField]
        public string Guid;

        [SerializeField]
        public Rect Position;

        public SimpleGraphNodeModel()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }

    [Serializable]
    public class SimpleGraphEdgeModel
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

        public SimpleGraphEdgeModel(SimpleGraphNodeModel outputNodeModel, SimpleGraphNodeModel inputNodeModel, string outputPortName, string inputPortName)
            : this(outputNodeModel.Guid, inputNodeModel.Guid, outputPortName, inputPortName)
        {
        }

        public SimpleGraphEdgeModel(string outputNodeGuid, string inputNodeGuid, string outputPortName, string inputPortName)
        {
            this.outputNodeGuid = outputNodeGuid;
            this.inputNodeGuid = inputNodeGuid;
            this.outputPortName = outputPortName;
            this.inputPortName = inputPortName;
        }
    }
}

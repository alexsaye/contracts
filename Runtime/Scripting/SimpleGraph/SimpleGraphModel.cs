using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class SimpleGraphModel
    {
        public static SimpleGraphModel Create(IEnumerable<SimpleGraphNodeModel> nodes, IEnumerable<SimpleGraphEdgeModel> edges)
        {
            return new SimpleGraphModel(nodes, edges);
        }

        [SerializeField]
        private SimpleGraphNodeModel[] nodes;
        public IEnumerable<SimpleGraphNodeModel> Nodes => nodes;        

        [SerializeField]
        private SimpleGraphEdgeModel[] edges;
        public IEnumerable<SimpleGraphEdgeModel> Edges => edges;

        internal SimpleGraphModel(IEnumerable<SimpleGraphNodeModel> nodes, IEnumerable<SimpleGraphEdgeModel> edges)
        {
            this.nodes = nodes.ToArray();
            this.edges = edges.ToArray();
        }
    }

    [Serializable]
    public class SimpleGraphNodeModel
    {
        public static SimpleGraphNodeModel Create<T>(Rect position, object value = null) where T : SimpleGraphNode
        {
            return new SimpleGraphNodeModel(typeof(T), position, value);
        }

        [SerializeField]
        private string guid;
        public string Guid => guid;

        [SerializeField]
        private string type;
        public string Type => type;

        [SerializeField]
        private Rect position;
        public Rect Position => position;

        [SerializeReference]
        private object value;
        public object Value => value;

        internal SimpleGraphNodeModel(Type type, Rect position, object value = null)
        {
            guid = System.Guid.NewGuid().ToString();
            this.type = type.AssemblyQualifiedName;
            this.position = position;
            this.value = value;
        }
    }

    [Serializable]
    public class SimpleGraphEdgeModel
    {
        public static SimpleGraphEdgeModel Create(SimpleGraphNodeModel outputNodeModel, SimpleGraphNodeModel inputNodeModel, string outputPortName, string inputPortName)
        {
            return new SimpleGraphEdgeModel(outputNodeModel.Guid, inputNodeModel.Guid, outputPortName, inputPortName);
        }

        [SerializeField]
        private string outputNodeGuid;
        /// <summary>
        /// The id of the node on the output side of the port.
        /// </summary>
        public string OutputNodeGuid => outputNodeGuid;

        [SerializeField]
        private string inputNodeGuid;
        /// <summary>
        /// The id of the node on the input side of the port.
        /// </summary>
        public string InputNodeGuid => inputNodeGuid;

        [SerializeField]
        private string outputPortName;
        /// <summary>
        /// The name of the output port on the output node.
        /// </summary>
        public string OutputPortName => outputPortName;

        [SerializeField]
        private string inputPortName;
        /// <summary>
        /// The name of the input port on the input node.
        /// </summary> 
        public string InputPortName => inputPortName;

        internal SimpleGraphEdgeModel(string outputNodeGuid, string inputNodeGuid, string outputPortName, string inputPortName)
        {
            this.outputNodeGuid = outputNodeGuid;
            this.inputNodeGuid = inputNodeGuid;
            this.outputPortName = outputPortName;
            this.inputPortName = inputPortName;
        }
    }
}

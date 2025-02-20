using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerGraph))]
    [NodePresentOnCreation(x: -200, y: 0)]
    public class CareerStartNode : ScriptableGraphNode
    {
        [NodeOutput("To", Port.Capacity.Multi)]
        public ScriptableGraphNode To;

        public CareerStartNode() : base("Start")
        {

        }
    }
}

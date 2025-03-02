using System;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [CreateAssetMenu(fileName = "New Career Progression Graph", menuName = "Contracts/Career Progression Graph")]
    public class CareerProgressionGraph : ScriptableGraph
    {
        private void Awake()
        {
            // Add a default start node.
            var careerStartNode = new NodeSaveData(typeof(CareerStartNode), new Rect(-300, 0, 0, 0));
            Nodes.Add(careerStartNode);

            // Add a default progression node.
            var careerProgressionNode = new NodeSaveData(typeof(CareerProgressionNode), new Rect(0, 0, 0, 0));
            Nodes.Add(careerProgressionNode);

            // Add a default end node.
            var careerEndNode = new NodeSaveData(typeof(CareerEndNode), new Rect(300, 0, 0, 0));
            Nodes.Add(careerEndNode);
        }
    }
}

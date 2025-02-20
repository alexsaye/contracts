using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContractGraph
{
    public class ContractGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        public ContractBaseGraph Graph { get; private set; }
        private static List<(Type type, NodeMenuAttribute attribute)> cachedNodeTypes;

        public ContractGraphView(ContractBaseGraph graph)
        {
            this.StretchToParentSize();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Contracts-New/Editor/USS/ContractGraphView.uss"));
            var grid = new GridBackground();
            grid.name = "Grid";
            Add(grid);
            grid.SendToBack();

            Graph = graph;
            
            foreach (var node in Graph.Nodes)
            {
                CreateNode(node);
            }
            
            foreach (var edge in Graph.Edges)
            {
                CreateEdge(edge);
            }
        }

        private void EnsureCachedNodeTypes()
        {
            if (cachedNodeTypes != null)
                return;

            cachedNodeTypes = new List<(Type type, NodeMenuAttribute attribute)>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<NodeMenuAttribute>();
                    if (attribute != null)
                    {
                        cachedNodeTypes.Add((type, attribute));
                    }
                }
            }
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent populateEvent)
        {
            EnsureCachedNodeTypes();

            Vector2 localMousePosition = this.contentViewContainer.WorldToLocal(populateEvent.mousePosition);

            foreach (var (type, attribute) in cachedNodeTypes)
            {
                populateEvent.menu.AppendAction(attribute.MenuName, (action) =>
                {
                    CreateNode(type, new Rect(localMousePosition, attribute.Size), Guid.NewGuid().ToString());
                });
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port || startPort.node == port.node)
                    return;

                if (startPort.portType == port.portType)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private void CreateNode(NodeSaveData nodeSave)
        {
            Debug.Log(nodeSave.Type);
            CreateNode(Type.GetType(nodeSave.Type), nodeSave.Position, nodeSave.Guid);
        }

        private void CreateNode(Type type, Rect position, string guid)
        {
            var node = (ContractGraphNode)Activator.CreateInstance(type);
            node.SetPosition(position);
            node.Guid = guid;
            AddElement(node);
        }

        private void CreateEdge(EdgeSaveData edgeSave)
        {
            var outputNode = graphElements.Where((element) => element is ContractGraphNode).Select((element) => element as ContractGraphNode).First((node) => node.Guid == edgeSave.OutputNodeGuid);
            var outputPort = outputNode.GetOutputPort(edgeSave.OutputPortName);
        
            var inputNode = graphElements.Where((element) => element is ContractGraphNode).Select((element) => element as ContractGraphNode).First((node) => node.Guid == edgeSave.InputNodeGuid);
            var inputPort = inputNode.GetInputPort(edgeSave.InputPortName);

            var edge = new UnityEditor.Experimental.GraphView.Edge
            {
                input = inputPort,
                output = outputPort
            };

            inputPort.Connect(edge);
            outputPort.Connect(edge);

            AddElement(edge);
        }

        public void SaveGraph()
        {
            Graph.Save(graphElements);
        }
    }
}

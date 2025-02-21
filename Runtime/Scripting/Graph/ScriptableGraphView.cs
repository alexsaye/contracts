using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    public class ScriptableGraphView : GraphView
    {
        public ScriptableGraph Graph { get; private set; }
        private static Dictionary<Type, List<NodeAttributeComposition>> cachedNodeTypes = new();

        public ScriptableGraphView(ScriptableGraph graph)
        {
            this.StretchToParentSize();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            var grid = new GridBackground();
            grid.name = "Grid";
            Add(grid);
            grid.SendToBack();

            Graph = graph;
            
            foreach (var node in Graph.Nodes)
            {
                LoadNode(node);
            }
            
            foreach (var edge in Graph.Edges)
            {
                LoadEdge(edge);
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            contentViewContainer.transform.position = new Vector3(layout.width / 2, layout.height / 2, 0);
        }

        private void EnsureCachedNodeTypes()
        {
            var graphType = Graph.GetType();
            if (cachedNodeTypes.ContainsKey(graphType))
                return;

            var nodeTypes = new List<NodeAttributeComposition>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var menuAttribute = type.GetCustomAttribute<NodeMenuAttribute>();
                    if (menuAttribute == null)
                    {
                        continue;
                    }

                    var contextAttribute = type.GetCustomAttribute<NodeContextAttribute>();
                    if (contextAttribute != null && !contextAttribute.Contexts.Contains(Graph.GetType()))
                    {
                        continue;
                    }

                    nodeTypes.Add(new NodeAttributeComposition()
                    {
                        Type = type,
                        Menu = menuAttribute,
                        Context = contextAttribute,
                    });
                }
            }
            cachedNodeTypes.Add(graphType, nodeTypes);
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent populateEvent)
        {
            EnsureCachedNodeTypes();

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(populateEvent.mousePosition);

            foreach (var composition in cachedNodeTypes[Graph.GetType()])
            {
                populateEvent.menu.AppendAction(composition.Menu.MenuName, (action) =>
                {
                    CreateNode(composition.Type, new Rect(localMousePosition, Vector2.zero), Guid.NewGuid().ToString());
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

        private ScriptableGraphNode CreateNode(Type type, Rect position, string guid)
        {
            var node = (ScriptableGraphNode)Activator.CreateInstance(type);
            node.Guid = guid;
            node.SetPosition(position);
            AddElement(node);
            return node;
        }

        private ScriptableGraphNode LoadNode(NodeSaveData nodeSave)
        {
            var nodeType = Type.GetType(nodeSave.Type);
            var node = CreateNode(nodeType, nodeSave.Position, nodeSave.Guid);
            var slots = nodeType.GetFields().Where(field => field.GetCustomAttribute<NodeFieldAttribute>() != null);
            foreach (var slot in slots)
            {
                var slotValue = nodeSave.Slots[slot.Name];
                if (slotValue != null)
                {
                    var slotAttribute = slot.GetCustomAttribute<NodeFieldAttribute>();
                    slot.SetValue(node, slotValue);

                    var objectField = node.GetSlot(slot.Name);
                    Debug.Log($"object field: {objectField != null}, name: {slot.Name}, {node.extensionContainer.childCount}");
                    objectField.value = slotValue;

                    Debug.Log($"Loading Slot: {slot.Name} = {slotValue}");
                }
            }
            return node;
        }

        private void LoadEdge(EdgeSaveData edgeSave)
        {
            var outputNode = graphElements.Where((element) => element is ScriptableGraphNode).Select((element) => element as ScriptableGraphNode).First((node) => node.Guid == edgeSave.OutputNodeGuid);
            var outputPort = outputNode.GetOutputPort(edgeSave.OutputPortName);
        
            var inputNode = graphElements.Where((element) => element is ScriptableGraphNode).Select((element) => element as ScriptableGraphNode).First((node) => node.Guid == edgeSave.InputNodeGuid);
            var inputPort = inputNode.GetInputPort(edgeSave.InputPortName);

            var edge = new Edge
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

    class NodeAttributeComposition
    {
        public Type Type;
        public NodeMenuAttribute Menu;
        public NodeContextAttribute Context;
    }
}

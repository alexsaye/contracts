using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleGraph
{
    // unity is wank lol https://discussions.unity.com/t/callback-on-edge-connection-in-graphview/769707/6
    public class ObservablePort : Port
    {
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                this.m_EdgesToCreate = new List<Edge>();
                this.m_EdgesToDelete = new List<GraphElement>();
                this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                this.m_EdgesToCreate.Clear();
                this.m_EdgesToCreate.Add(edge);
                this.m_EdgesToDelete.Clear();
                if (edge.input.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }
                if (edge.output.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.output.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }
                if (this.m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements((IEnumerable<GraphElement>)this.m_EdgesToDelete);
                List<Edge> edgesToCreate = this.m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                    edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
                foreach (Edge edge1 in edgesToCreate)
                {
                    graphView.AddElement((GraphElement)edge1);
                    edge.input.Connect(edge1);
                    edge.output.Connect(edge1);
                }
            }
        }

        public event EventHandler<PortConnectionEventArgs> Connected;
        public event EventHandler<PortConnectionEventArgs> Disconnected;

        protected ObservablePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);

            Connected?.Invoke(this, new PortConnectionEventArgs(edge));
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            Disconnected?.Invoke(this, new PortConnectionEventArgs(edge));
        }

        public override void DisconnectAll()
        {
            var connections = this.connections.ToArray();
            base.DisconnectAll();
            foreach (var edge in connections)
            {
                Disconnected?.Invoke(this, new PortConnectionEventArgs(edge));
            }
        }

        public static ObservablePort Create<TEdge>(
            string name,
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
            where TEdge : Edge, new()
        {
            var listener = new DefaultEdgeConnectorListener();
            var ele = new ObservablePort(orientation, direction, capacity, type)
            {
                m_EdgeConnector = (EdgeConnector)new EdgeConnector<TEdge>((IEdgeConnectorListener)listener)
            };
            ele.AddManipulator((IManipulator)ele.m_EdgeConnector);
            ele.name = name;
            ele.portName = name;
            return ele;
        }
    }

    public class PortConnectionEventArgs: EventArgs
    {
        public Edge Edge { get; }
        public PortConnectionEventArgs(Edge edge)
        {
            Edge = edge;
        }
    }
}
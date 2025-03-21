using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SimpleGraph.Editor
{
    public abstract class SimpleGraphNodeView : Node
    {
        public string Guid { get; private set; }

        public SimpleGraphNodeView()
        {
        }

        public void LoadModel(SerializedProperty serializedNode)
        {
            Guid = serializedNode.FindPropertyRelative(nameof(SimpleGraphNode.Guid)).stringValue;
            SetPosition(serializedNode.FindPropertyRelative(nameof(SimpleGraphNode.Position)).rectValue);
            RenderModel(serializedNode);
            RefreshExpandedState();
        }

        protected virtual void RenderModel(SerializedProperty serializedNode)
        {
        }

        protected static PropertyField CreateField(string name, SerializedProperty serializedParentProperty)
        {
            var field = new PropertyField(serializedParentProperty.FindPropertyRelative(name));
            field.name = name;
            return field;
        }

        protected static Port CreatePort<T>(string name, Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, typeof(T));
            port.name = name;
            port.portName = name;
            return port;
        }
    }

    // TODO: can we do this through a generic parameter in SimpleGraphNodeView instead?
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SimpleGraphNodeViewAttribute : Attribute
    {
        public Type ModelType { get; }

        public SimpleGraphNodeViewAttribute(Type modelType)
        {
            ModelType = modelType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SimpleGraphNodeMenuAttribute : Attribute
    {
        public Type GraphType { get; }
        public string MenuName { get; }

        public SimpleGraphNodeMenuAttribute(Type graphType, string menuName)
        {
            GraphType = graphType;
            MenuName = menuName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class SimpleGraphNodeCapabilitiesAttribute : Attribute
    {
        public Capabilities Capabilities { get; }

        public SimpleGraphNodeCapabilitiesAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }
    }
}

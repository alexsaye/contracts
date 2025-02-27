using System;
using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeMenuAttribute : Attribute
    {
        public string MenuName { get; }

        public NodeMenuAttribute(string menuName)
        {
            MenuName = menuName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeContextAttribute : Attribute
    {
        public Type[] Contexts { get; }

        public NodeContextAttribute(params Type[] contexts)
        {
            Contexts = contexts;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeInputAttribute : Attribute
    {
        public Port.Capacity Capacity { get; }

        public NodeInputAttribute(Port.Capacity capacity = Port.Capacity.Single)
        {
            Capacity = capacity;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeOutputAttribute : Attribute
    {
        public Port.Capacity Capacity { get; }

        public NodeOutputAttribute(Port.Capacity capacity = Port.Capacity.Multi)
        {
            Capacity = capacity;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class NodeCapabilitiesAttribute : Attribute
    {
        public Capabilities Capabilities { get; }

        public NodeCapabilitiesAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }
    }
}
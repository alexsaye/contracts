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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NodeContextAttribute : Attribute
    {
        public Type GraphType { get; }

        public NodeContextAttribute(Type graphType)
        {
            GraphType = graphType;
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
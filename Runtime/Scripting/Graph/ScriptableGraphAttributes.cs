using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NodeMenuAttribute : Attribute
    {
        public string MenuName { get; }
        public Vector2 Size { get;  }

        public NodeMenuAttribute(string menuName)
            : this(menuName, new Vector2(100, 100)) { } 

        public NodeMenuAttribute(string menuName, Vector2 size)
        {
            MenuName = menuName;
            Size = size;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeInputAttribute : Attribute
    {
        public string Name { get; }
        public Port.Capacity Capacity { get; }

        public NodeInputAttribute(string name, Port.Capacity capacity = Port.Capacity.Single)
        {
            Name = name;
            Capacity = capacity;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeOutputAttribute : Attribute
    {
        public string Name { get; }
        public Port.Capacity Capacity { get; }

        public NodeOutputAttribute(string name, Port.Capacity capacity = Port.Capacity.Multi)
        {
            Name = name;
            Capacity = capacity;
        }
    }
}

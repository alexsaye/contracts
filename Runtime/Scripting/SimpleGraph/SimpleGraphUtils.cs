using System;
using UnityEditor.Experimental.GraphView;

namespace SimpleGraph
{
    public static class SimpleGraphUtils
    {
        public static Port CreatePort<T>(string name, Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, typeof(T));
            port.name = name;
            port.portName = name;
            return port;
        }
    }
}

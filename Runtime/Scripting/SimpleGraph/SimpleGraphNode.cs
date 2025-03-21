using System;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public abstract class SimpleGraphNode
    {
        [SerializeField]
        public string Guid;

        [SerializeField]
        public Rect Position;

        public SimpleGraphNode()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}

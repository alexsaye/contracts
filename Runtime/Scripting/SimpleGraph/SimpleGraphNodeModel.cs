using System;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public abstract class SimpleGraphNodeModel
    {
        [SerializeField]
        public string Guid;

        [SerializeField]
        public Rect Position;

        public SimpleGraphNodeModel()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}

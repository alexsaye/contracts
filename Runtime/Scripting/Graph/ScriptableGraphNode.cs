using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraphNode : Node
    {
        public string Guid { get; private set; }

        public ScriptableGraphNode()
        {
        }

        public virtual ScriptableGraphNodeModel Save()
        {
            if (Guid == null)
            {
                Guid = System.Guid.NewGuid().ToString();
            }
            return new ScriptableGraphNodeModel(GetType(), GetPosition(), Guid);
        }

        public virtual void Load(ScriptableGraphNodeModel model)
        {
            Guid = model.Guid;
        }
    }
}

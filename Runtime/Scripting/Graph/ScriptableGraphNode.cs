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
            return new ScriptableGraphNodeModel(GetType(), GetPosition(), Guid);
        }

        public virtual void Load(ScriptableGraphNodeModel model = null)
        {
            Guid = model != null ? model.Guid : System.Guid.NewGuid().ToString();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    [CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/Blank Contract")]
    public class ScriptableContract : ScriptableObject
    {
        [SerializeField]
        protected ScriptableCondition resolving;

        [SerializeField]
        protected ScriptableCondition rejecting;

        [SerializeField]
        protected List<ScriptableContract> nextOnResolved = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnResolved => nextOnResolved;

        [SerializeField]
        protected List<ScriptableContract> nextOnRejected = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnRejected => nextOnRejected;

        public virtual IContract Build(UnityEvent update)
        {
            Debug.Log($"Building contract: {this}");
            var resolving = this.resolving != null ? this.resolving.Build(update) : Condition.Never;
            var rejecting = this.rejecting != null ? this.rejecting.Build(update) : Condition.Never;
            var contract = new Contract(resolving, rejecting);
            return contract;
        }
    }
}

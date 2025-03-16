using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public interface IConditionBuilder
    {
        public ICondition Build(UnityEvent updated);
    }
}

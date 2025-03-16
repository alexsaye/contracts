using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Contracts
{
    public interface IConditionBuilder
    {
        ICondition Build(UnityEvent updated);
    }
}

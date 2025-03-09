using UnityEngine.Events;

namespace Contracts
{
    internal interface IBuilder<T>
    {
        T Build(UnityEvent updated);
    }
}

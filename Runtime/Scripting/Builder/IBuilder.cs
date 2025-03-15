using UnityEngine.Events;

namespace Contracts
{
    public interface IBuilder<T>
    {
        T Build(UnityEvent updated);
    }
}

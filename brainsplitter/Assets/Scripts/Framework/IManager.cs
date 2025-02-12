using System.Collections;

namespace Framework
{
    public interface IManager
    {
        IEnumerator Initialize();
        void Dispose();
    }
}
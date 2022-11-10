using UnityEngine;

namespace Interfaces
{
    public abstract class BaseApplicationContextComponent : MonoBehaviour
    {
        public abstract void Init();
        public virtual void InitComplete(){}
        public virtual void Dispose(){}
    }
}
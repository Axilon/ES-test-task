using System;

namespace Data
{
    [Serializable]
    public struct Range<T>
    {
        public T Min;
        public T Max;
    }
}
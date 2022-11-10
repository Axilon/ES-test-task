using Enums;
using UnityEngine;

namespace Lightning
{
    public class BaseNodeComponent : MonoBehaviour
    {
        [SerializeField] private LightningType _type;
        [SerializeField] private PositionType _startType;

        private RectTransform _transform;
        public virtual bool HasTransitions => false;
        public LightningType Type => _type;
        public PositionType StartType => _startType;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
        }

        public void SetParent(Transform parent)
        {
            _transform.parent = parent;
        }

        public void SetPosition(Vector2 newPos)
        {
            _transform.position = newPos;
        }
    }

}
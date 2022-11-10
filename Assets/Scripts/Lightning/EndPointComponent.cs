using Enums;
using UnityEngine;

namespace Lightning
{
    public class EndPointComponent : MonoBehaviour
    {
        [SerializeField] private PositionType _endPositionType;
        [SerializeField] private LightningType _endNodeType;
        
        private RectTransform _transform;

        public PositionType EndPositionType => _endPositionType;
        public LightningType EndNodeType => _endNodeType;
        public Vector2 Position => _transform.position;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
        }
    }
}
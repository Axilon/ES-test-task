using UnityEngine;

namespace Lightning
{
    public class NodeWithTransitionComponent : BaseNodeComponent
    {
        [SerializeField] private EndPointComponent[] _endPoints;

        public EndPointComponent[] EndPoints => _endPoints;
        public override bool HasTransitions => _endPoints.Length > 0;
    }
}
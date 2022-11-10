using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Interfaces;
using Lightning;
using Scriptable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LightningGenerator
{
    public class LightningGenerator : BaseApplicationContextComponent
    {
        [SerializeField] private LightningNodesConfig _nodesConfig;
        [SerializeField] private float _widthOffset;
        [SerializeField] private float _heightOffset;
        [SerializeField] private ushort _minLightningWidth;
        [SerializeField] private ushort _lightsToSpawn;

        private Range<float> _xRange;
        private Range<float> _yRange;

        private LightningGeneratorHelper _helper;
        private List<BaseNodeComponent> _nodesWithTransition;
        private List<BaseNodeComponent> _completeNodes;

        
        public override void Init()
        {
            InitHelper();
            
            _xRange.Min = _widthOffset;
            _xRange.Max = Screen.width - _widthOffset;
            _yRange.Min = _heightOffset;
            _yRange.Max = Screen.height - _heightOffset;
        }

        private void InitHelper()
        {
            _helper = new LightningGeneratorHelper();
            _helper.SetUpNodes(_nodesConfig.BaseNodeComponent);
        }
        
        public async Task<GameObject> GenerateLightning()
        {
            var headNode = await _helper.GetStartNode();
            if (headNode == null) return null;
            
            var headInstance = Instantiate(headNode);
            var parent = CreateLightningHolder();
            var startPoint = GetStartPoint();
            
            SetParentAndPosition(headInstance, parent.transform, startPoint);
            
            for (int i = 0; i < headInstance.EndPoints.Length; i++)
            {
                if (i <= 0) await SpawnMainLightning(parent.transform,headInstance, headInstance.EndPoints[i].Position, 1);
                else await SpawnAdditiveLightning(parent.transform,headInstance, headInstance.EndPoints[i].Position);
            }

            return parent;
        }

        private Vector2 GetStartPoint()
        {
            var x = Random.Range(_xRange.Min, _xRange.Max);
            var y = _yRange.Max;
            return new Vector2(x, y);
        }

        private async Task SpawnAdditiveLightning(Transform parent, BaseNodeComponent fromNode, Vector2 lastPos)
        {
            if(!CanSpawnNewNode(fromNode, lastPos)) return;
            
            var transitionNode = fromNode as NodeWithTransitionComponent;
            foreach (var endPoint in transitionNode.EndPoints)
            {
                var newNode = await SpawnNextNode(endPoint, true);
                
                if (newNode == null) continue;
                
                SetParentAndPosition(newNode, parent, endPoint.Position);
                
                await SpawnAdditiveLightning(parent,newNode, endPoint.Position);
            }
        }

        private async Task SpawnMainLightning(Transform parent, BaseNodeComponent fromNode, Vector2 lastPos, int mainNodes)
        {
            if(!CanSpawnNewNode(fromNode, lastPos)) return;
            
            var transitionNode = fromNode as NodeWithTransitionComponent;
            
            for (int i = 0; i < transitionNode.EndPoints.Length; i++)
            {
                var endPoint = transitionNode.EndPoints[i];
                var newNode = await SpawnNextNode(endPoint, mainNodes >= _minLightningWidth);
                mainNodes++;
                
                if(newNode == null) continue;
                
                SetParentAndPosition(newNode, parent, endPoint.Position);
                
                if (i <= 0 && mainNodes < _minLightningWidth) 
                    await SpawnMainLightning(parent, newNode, endPoint.Position, mainNodes);
                else 
                    await SpawnAdditiveLightning(parent, newNode, endPoint.Position);
            }
        }

        private bool CanSpawnNewNode(BaseNodeComponent fromNode, Vector2 lastPos)
        {
            if(fromNode == null) return false;
            if(!fromNode.HasTransitions) return false;
            if(lastPos.x <= _xRange.Min || lastPos.x >= _xRange.Max) return false;
            if(lastPos.y <= _yRange.Min || lastPos.y >= _yRange.Max) return false;
            return true;
        }
        
        private async Task<BaseNodeComponent> SpawnNextNode( EndPointComponent endPoint, bool canBeCompleted)
        {
            var newNode = await _helper.GetNextNode(new EndPointData
                {
                    EndNodeType = endPoint.EndNodeType,
                    EndPositionType = endPoint.EndPositionType
                },
                canBeCompleted);

            if (newNode == null) return null;
            
            var instance = Instantiate(newNode);
            return instance;
        }

        private GameObject CreateLightningHolder()
        {
            var holder = new GameObject("Lightning");
            var canvasGroup = holder.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            
            holder.transform.SetParent(transform);
            
            return holder;
        }
        
        private void SetParentAndPosition(BaseNodeComponent node, Transform parent, Vector2 position)
        {
            node.SetParent(parent); 
            node.SetPosition(position);
        }

        public override void Dispose()
        {
            _helper?.Dispose();
        }
    }
}
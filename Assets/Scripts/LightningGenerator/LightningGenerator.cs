using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Lightning;
using Scriptable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LightningGenerator
{
    public class LightningGenerator : MonoBehaviour
    {
        [SerializeField] private LightningNodesConfig _nodesConfig;
        [SerializeField] private float _widthOffset;
        [SerializeField] private float _heightOffset;
        [SerializeField] private ushort _minLightningWidth;
        [SerializeField] private ushort _lightsToSpawn;
        
        private float _width;
        private float _height;

        private LightningGeneratorHelper _helper;
        private List<BaseNodeComponent> _nodesWithTransition;
        private List<BaseNodeComponent> _completeNodes;

        private void Awake()
        {
            InitHelper();
            
            _width = Screen.width;
            _height = Screen.height;
        }

        private void InitHelper()
        {
            _helper = new LightningGeneratorHelper();
            _helper.SetUpNodes(_nodesConfig.BaseNodeComponent);
        }
        private void Start()
        {
            for (int i = 0; i < _lightsToSpawn; i++)
            {
                GenerateLightning();
            }
        }
        
        private async void GenerateLightning()
        {
            var headNode = await _helper.GetStartNode();
            
            if (headNode == null) return;
            var headInstance = Instantiate(headNode);
            
            var startPoint = new Vector2(Random.Range(_widthOffset, _width - _widthOffset), _height - _heightOffset);
            
            headInstance.SetParent(transform);
            headInstance.SetPosition(startPoint);
            
            for (int i = 0; i < headInstance.EndPoints.Length; i++)
            {
                if (i <= 0) SpawnMainLightning(headInstance, headInstance.EndPoints[i].Position, 1);
                else SpawnAdditiveLightning(headInstance, headInstance.EndPoints[i].Position);
            }
        }

        private async void SpawnAdditiveLightning(BaseNodeComponent fromNode, Vector2 lastPos)
        {
            if(!CanSpawnNewNode(fromNode, lastPos)) return;
            
            var transitionNode = fromNode as NodeWithTransitionComponent;
            foreach (var endPoint in transitionNode.EndPoints)
            {
                var newNode = await SpawnNextNode(endPoint, true);
                if (newNode == null) continue;
                
                SpawnAdditiveLightning(newNode, endPoint.Position);
            }
        }

        private async void SpawnMainLightning(BaseNodeComponent fromNode, Vector2 lastPos, int mainNodes)
        {
            if(!CanSpawnNewNode(fromNode, lastPos)) return;
            
            var transitionNode = fromNode as NodeWithTransitionComponent;
            
            for (int i = 0; i < transitionNode.EndPoints.Length; i++)
            {
                var endPoint = transitionNode.EndPoints[i];
                var newNode = await SpawnNextNode(endPoint, mainNodes >= _minLightningWidth);
                mainNodes++;
                
                if(newNode == null) continue;
                
                if (i <= 0 && mainNodes < _minLightningWidth) 
                    SpawnMainLightning(newNode, endPoint.Position, mainNodes);
                else 
                    SpawnAdditiveLightning(newNode, endPoint.Position);
            }
        }

        private bool CanSpawnNewNode(BaseNodeComponent fromNode, Vector2 lastPos)
        {
            if(fromNode == null) return false;
            if(!fromNode.HasTransitions) return false;
            if(lastPos.x <= _widthOffset || lastPos.x >= _width - _widthOffset) return false;
            if(lastPos.y <= _heightOffset || lastPos.y >= _height - _heightOffset) return false;
            return true;
        }
        
        private async Task<BaseNodeComponent> SpawnNextNode(EndPointComponent endPoint, bool canBeCompleted)
        {
            var newNode = await _helper.GetNextNode(new EndPointData
                {
                    EndNodeType = endPoint.EndNodeType,
                    EndPositionType = endPoint.EndPositionType
                },
                canBeCompleted);

            if (newNode == null) return null;
            
            var instance = Instantiate(newNode);
            instance.SetParent(transform); 
            instance.SetPosition(endPoint.Position);
            return instance;
        }
        
        private void OnDestroy()
        {
            _helper.Dispose();
        }
    }
}
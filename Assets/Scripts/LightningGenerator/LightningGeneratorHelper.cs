using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Enums;
using Lightning;
using UnityEngine;

namespace LightningGenerator
{
    public class LightningGeneratorHelper
    {
        private Dictionary<(LightningType, bool), Func<EndPointData, BaseNodeComponent>> _actionsMap;

        private List<BaseNodeComponent> _boldTransitionNodes; 
        private List<BaseNodeComponent> _thinTransitionNodes; 
        private List<BaseNodeComponent> _boldCompleteNodes; 
        private List<BaseNodeComponent> _thinCompleteNodes;
        
        //todo; migrate to task manager
        private CancellationTokenSource _cancellationTokenSource;

        public LightningGeneratorHelper()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            FillActionsMap();
        }
        
        private void FillActionsMap()
        {
            _actionsMap = new Dictionary<(LightningType, bool), Func<EndPointData, BaseNodeComponent>>
            {
                {(LightningType.Bold, true), (endPoint) => GetNode(endPoint,_boldCompleteNodes)},
                {(LightningType.Bold, false), (endPoint) => GetNode(endPoint,_boldTransitionNodes)},
                {(LightningType.Thin, true), (endPoint) => GetNode(endPoint,_thinCompleteNodes)},
                {(LightningType.Thin, false), (endPoint) => GetNode(endPoint,_thinTransitionNodes)}
            };
        }
        
        public void SetUpNodes(GameObject[] nodesGO)
        {
            InitData();
            FillData(nodesGO);
        }

        private void InitData()
        {
            _boldTransitionNodes = new List<BaseNodeComponent>();
            _boldCompleteNodes = new List<BaseNodeComponent>();
            _thinTransitionNodes = new List<BaseNodeComponent>();
            _thinCompleteNodes = new List<BaseNodeComponent>();
        }

        private void FillData(GameObject[] nodesGO)
        {
            foreach (var go in nodesGO)
            {
                var component = go.GetComponent<BaseNodeComponent>();
                switch (component.Type)
                {
                    case LightningType.Bold when component.HasTransitions:
                        _boldTransitionNodes.Add(component);
                        continue;
                    case LightningType.Bold when !component.HasTransitions:
                        _boldCompleteNodes.Add(component);
                        continue;
                    case LightningType.Thin when component.HasTransitions:
                        _thinTransitionNodes.Add(component);
                        continue;
                    default:
                        _thinCompleteNodes.Add(component);
                        continue;
                }
            }
        }
        public async Task<NodeWithTransitionComponent> GetStartNode()
        {
            if(_boldTransitionNodes.Count <= 0 && _thinTransitionNodes.Count <= 0) return null;
            
            var randValue = new System.Random().Next();
            var randStartType = randValue > int.MaxValue * 0.5f ? LightningType.Bold : LightningType.Thin;

            var result = await Task.Run(() =>
            {
                var node = _actionsMap[(randStartType, false)].Invoke(null);
            
                if (node != null) return node;
                randStartType = randStartType == LightningType.Bold ? LightningType.Thin : LightningType.Bold;
            
                return _actionsMap[(randStartType,false)].Invoke(null);
            }, _cancellationTokenSource.Token);

            return result? result as NodeWithTransitionComponent : null;
        }
        
        public async Task<BaseNodeComponent> GetNextNode(EndPointData fromNode, bool canBeComplete)
        {
            var shouldBeComplete = canBeComplete && new System.Random().Next() > int.MaxValue * 0.5f;
            
            var result = await Task.Run(() =>
            {

                var paramsStack = new Stack<(LightningType, bool)>();

                switch (fromNode.EndNodeType)
                {
                    case LightningType.Bold when !shouldBeComplete:
                        paramsStack.Push((LightningType.Bold, true));
                        paramsStack.Push((LightningType.Bold, false));
                        break;
                    case LightningType.Bold:
                        paramsStack.Push((LightningType.Bold, false));
                        paramsStack.Push((LightningType.Bold, true));
                        break;
                    case LightningType.Thin when !shouldBeComplete:
                        paramsStack.Push((LightningType.Thin, true));
                        paramsStack.Push((LightningType.Thin, false));
                        break;
                    case LightningType.Thin:
                        paramsStack.Push((LightningType.Thin, false));
                        paramsStack.Push((LightningType.Thin, true));
                        break;
                }

                while (paramsStack.Count > 0)
                {
                    var func = paramsStack.Pop();
                    var node = _actionsMap[func].Invoke(fromNode);
                    if (node != null) return node;
                }

                return null;
            }, _cancellationTokenSource.Token);
            
            return result;
        }

        private BaseNodeComponent GetNode(EndPointData fromPoint, List<BaseNodeComponent> checkList)
        {
            var listCopy = new List<BaseNodeComponent>(checkList);
            var rand = new System.Random();

            while (listCopy.Count > 0)
            {
                var node = listCopy[rand.Next(0, listCopy.Count)];
                if (CanBeNextNode(fromPoint, node)) return node;
                listCopy.Remove(node);
            }
            
            return null;
        }
        
        private bool CanBeNextNode(EndPointData endPoint, BaseNodeComponent checkNode)
        {
            if (endPoint == null) return checkNode.StartType == PositionType.Top;
            
            switch (endPoint.EndPositionType)
            {
                case PositionType.Bottom when checkNode.StartType == PositionType.Top:
                case PositionType.Top when checkNode.StartType == PositionType.Bottom:
                case PositionType.Left when checkNode.StartType == PositionType.Right:
                case PositionType.Right when checkNode.StartType == PositionType.Left:
                    return true;
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            _actionsMap = null;
            _boldTransitionNodes = null;
            _boldCompleteNodes = null;
            _thinTransitionNodes = null;
            _thinCompleteNodes = null;
        }
    }
}
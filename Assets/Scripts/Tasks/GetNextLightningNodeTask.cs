using System.Threading;
using System.Threading.Tasks;
using Data;
using Lightning;
using LightningGenerator;
using UnityEngine;

namespace Tasks
{
    public class GetNextLightningNodeTask : BaseLightningNodeTask
    {
        private int _tries = 5;
        private EndPointData _endPointData;
        private bool _canBeComplete;

        public GetNextLightningNodeTask(LightningGeneratorHelper helper, CancellationToken cancellationToken) : base(helper, cancellationToken)
        {
        }


        public async Task<BaseNodeComponent> Get(EndPointData fromNode, bool canBeComplete)
        {
            _endPointData = fromNode;
            _canBeComplete = canBeComplete;
            
            //var component = await RunGetTask(TryGet);
            return null; //component == null ? null : Object.Instantiate(component);
        }

        /*
        private BaseNodeComponent TryGet()
        {
            var shouldBeComplete = _canBeComplete && new System.Random().Next() > int.MaxValue * 0.5f;

            while (_tries > 0)
            {
                var node = Helper.GetNextNode(_endPointData, shouldBeComplete);
                if(node != null) return node;
                _tries--;
            }

            if (!shouldBeComplete)
            {
                while (_tries > 0)
                {
                    var node = Helper.GetNextNode(_endPointData, true);
                    if(node != null) return node;
                    _tries--;
                }
            }
            return null;
        }
        */
        
        
        /*private async Task<BaseNodeComponent> TryGet(EndPointData fromNode, bool canBeComplete)
        {
            var shouldBeComplete = canBeComplete && new System.Random().Next() > int.MaxValue * 0.5f;
            
            var result = await Task.Run(() =>
            {
                while (_tries > 0)
                {
                    var node = _helper.GetNextNode(fromNode, shouldBeComplete);
                    if(node != null) return node;
                    _tries--;
                }
                return null;
            });

            if (result != null) return result;
            
            _tries = 5;
            return await TryGet(fromNode, false);
        }*/
    }
}
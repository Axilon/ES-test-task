using System.Threading;
using System.Threading.Tasks;
using Enums;
using Lightning;
using LightningGenerator;
using UnityEngine;

namespace Tasks
{
    public class GetStartLightningNodeTask : BaseLightningNodeTask
    {
        private int _tries = 5;
        
        public GetStartLightningNodeTask(LightningGeneratorHelper helper, CancellationToken cancellationToken) : base(helper, cancellationToken)
        {
        }
        
        public async Task<NodeWithTransitionComponent> Get()
        {
            //var component = await RunGetTask(TryGet);
            return null;//component == null ? null : Object.Instantiate(component);
        }

        /*
        private async Task<NodeWithTransitionComponent> TryGet()
        {
            var result = await Task.Run(() =>
            {
                while (_tries > 0)
                {
                    var node = Helper.GetStartNode();
                    if(node != null) return node;
                    _tries--;
                }
                return null;
            },CancellationToken);
            
            return result;
        }*/

        /*private NodeWithTransitionComponent TryGet()
        {
            /*var nodeType = new System.Random().Next() > int.MaxValue * 0.5f ? LightningType.Bold : LightningType.Thin;
            var node = Helper.GetStartNode(nodeType);
            if (node != null) return node;
            nodeType = nodeType == LightningType.Bold ? LightningType.Thin : LightningType.Bold;
            return Helper.GetStartNode(nodeType);#1#
        }*/
    }
}
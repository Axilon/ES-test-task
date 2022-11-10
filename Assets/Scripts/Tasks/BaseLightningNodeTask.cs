using System;
using System.Threading;
using System.Threading.Tasks;
using Lightning;
using LightningGenerator;

namespace Tasks
{
    public abstract class BaseLightningNodeTask
    {
        protected readonly LightningGeneratorHelper Helper;
        private readonly CancellationToken _cancellationToken;
        protected BaseLightningNodeTask(LightningGeneratorHelper helper, CancellationToken cancellationToken)
        {
            Helper = helper;
            _cancellationToken = cancellationToken;
        }
        
        protected async Task<T> RunGetTask<T>(Func<T> func) where T : BaseNodeComponent
        {
            var result = await Task.Run(func, _cancellationToken);
            return result;
        }
    }
}
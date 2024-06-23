using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace botplatform.Models.diagnostics
{
    public class errorCollector
    {
        List<string> errors = new();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async Task Add(string error)
        {
            await semaphore.WaitAsync();
            try
            {
                if (!errors.Contains(error))
                    errors.Add(error);
            } finally
            {
                semaphore.Release();
            }
            
        }

        public async Task<string[]> Get()
        {
            await semaphore.WaitAsync();
            try
            {
                string[] res = errors.ToArray();
                errors.Clear();
                return res;
            } finally
            {
                semaphore.Release();
            }
        }

        public async Task Clear()
        {
            await semaphore.WaitAsync();

            try
            {
                errors.Clear();
            } finally
            {
                semaphore?.Release();   
            }

        }
    }
}

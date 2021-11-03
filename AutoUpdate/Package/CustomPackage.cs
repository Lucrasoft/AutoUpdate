using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    class CustomPackage : IPackage
    {

        private byte[] bytes;

        public CustomPackage(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public Task<byte[]> GetContentAsync(Version version, Action<string, int> currentOperationTotalPercentDone)
        {
            return Task.FromResult(this.bytes);
        }
    }
}

using System.IO;
using System.Threading.Tasks;

namespace AsyncConverter.Tests.Test.Data.FixReturnValueToTaskTests
{
    public class Class
    {
        public async Task TestAsync()
        {
            using (Method2())
            {

            }
        }

        private Task<IDisposable> Method2()
        {
            throw new NotImplementedException();
        }
    }
}

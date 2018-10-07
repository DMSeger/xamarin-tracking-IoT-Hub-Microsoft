using Akavache;
using CML.Model;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using CML.Environment;

namespace CML.Services
{
    public class AccessRepo
    {
        private AccessRepo() { }
        private static AccessRepo instance;
        private static SemaphoreSlim _semaphore;

        public static SemaphoreSlim Semaphore
        {
            get
            {
                return _semaphore ?? 
                    (_semaphore = new SemaphoreSlim(1, 1));
            }
        }

        public static AccessRepo getInstance()
        {
            try
            {
                if (instance == null)
                {
                    instance = new AccessRepo();
                }

                return instance;
            }
            catch
            {
                return null;
            }
        }

        public static void InitializeDataBank()
        {
            BlobCache.ApplicationName = AppConstants.DEVICE_DEFAULT_NAME;
        }

        public async Task<InformationBuffer> ReadData()
        {
            try
            {
                var _informationBuffer = new InformationBuffer();

                _informationBuffer = await BlobCache.UserAccount.GetObject<InformationBuffer>("buffer");

                return _informationBuffer;
            }
            catch 
            {
                return null;
            }
        }

        public async Task InsertData(InformationBuffer _informationBuffer)
        {
            try
            {
                await BlobCache.UserAccount.InsertObject("buffer", _informationBuffer);
            }
            catch { }
        }

        public async Task SaveState(bool state)
        {
            try
            {
                await DumpCache("state");

                await BlobCache.UserAccount.InsertObject("state", state);
                await BlobCache.UserAccount.Flush();
            }
            catch { }
        }

        public async Task<bool?> ReadState()
        {
            try
            {
                return await BlobCache.UserAccount.GetObject<bool?>("state");
            }
            catch
            {
                return null;
            }
        }

        public async Task DumpCache(string key)
        {
            try
            {
                await BlobCache.UserAccount.Invalidate(key);
                await BlobCache.UserAccount.Vacuum();
                await BlobCache.UserAccount.Flush();
            }
            catch { }
        }
    }
}

using System;
using Android.App;
using Android.Content;
using Android.OS;
using System.Threading.Tasks;

namespace CML.Droid.Services
{
    [Service]
    class ReceiveMessagesTaskService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Task.Run(() => {
                try
                {
                    var thread = new TaskRecvMsgs();
                    thread.RunCounter().Wait();
                }
                catch (Exception e)
                {

                }
            });
            return StartCommandResult.Sticky;
        }
    }
}
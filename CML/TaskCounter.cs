using System.Threading;
using System.Threading.Tasks;
using CML.Messages;
using CML.ViewModel;
using Android.Net;
using Android.Content;
using Xamarin.Forms;

namespace CML
{
    public class TaskCounter
    {

        private Context context = null;

        public TaskCounter(Context context)
        {
            this.context = context;
        }
        public async Task RunCounter(CancellationToken token)
        {
            await Task.Run(async () => {

                //var count = 0;
                var wakeMessage = new WakeDeviceMessage();
                int timeDelay = 1000;
                var timeLimitCallWakeUp = System.DateTime.Now;

                while (true) {

                    token.ThrowIfCancellationRequested();

                    try
                    {
                        timeLimitCallWakeUp = System.DateTime.Now;
                        if (DetectNetwork())
                        {
                            MainPageViewModel.SendLocationSync();
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MessagingCenter.Send(wakeMessage, "WakeDeviceMessage");
                            });
                        }

                        var diffTime = (timeLimitCallWakeUp - System.DateTime.Now).TotalSeconds;

                        if (diffTime > 30)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MessagingCenter.Send(wakeMessage, "WakeDeviceMessage");
                            });
                        }

                        await Task.Delay(timeDelay);

                        await MainPageViewModel.GetDataLocationSync();

                    }
                    catch {; }
                }
            }, token);
        }

        public bool DetectNetwork()
        {
            bool isOnline = false;
            try
            {
                var connectivityManager = (ConnectivityManager)this.context.GetSystemService(Context.ConnectivityService);
                NetworkInfo networkInfo = connectivityManager.ActiveNetworkInfo;
                isOnline = networkInfo.IsConnected;
            }
            catch {; }

            return isOnline;
        }
    }
}

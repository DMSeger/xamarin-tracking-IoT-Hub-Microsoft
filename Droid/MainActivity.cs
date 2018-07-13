using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Permissions;
using Xamarin.Forms;
using CML.Messages;
using CML.Droid.Services;
using GalaSoft.MvvmLight.Ioc;
using CML.Interfaces;
using CML.Droid.Providers;
using System;
using static Android.OS.PowerManager;
using System.Threading.Tasks;

namespace CML.Droid
{
    [Activity(Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        private PowerManager _power;
        public PowerManager Power
        {
            get
            {
                if (_power == null)
                {
                    _power = (PowerManager)GetSystemService(Context.PowerService);
                }
                return _power;
            }
        }

        private WakeLock _fullWakeLock;
        public WakeLock FullWakeLock
        {
            get
            {
                if (_power == null)
                {
                    _fullWakeLock = Instance.Power.NewWakeLock(WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, "fullwakelocktag");
                }
                return _fullWakeLock;
            }
        }

        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App());

            Instance = this;

            UserDialogs.Init(Instance);

            //Incluir aqui serviço para obter mensagens via IoT.

            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message =>
            {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message =>
            {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StopService(intent);
            });

            MessagingCenter.Subscribe<WakeDeviceMessage>(this, "WakeDeviceMessage", message =>
            {
                WakeDevice();
            });

            SimpleIoc.Default.Register<IDeviceId, DeviceIdAndroidProvider>();          
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void WakeDevice()
        {
            try
            {
                FullWakeLock.Acquire();
                //3s com a tela acessa
                Task.Delay(3000).Wait();
                FullWakeLock.Release();
            }
            catch (Exception e)
            {; }
        }
    }
}

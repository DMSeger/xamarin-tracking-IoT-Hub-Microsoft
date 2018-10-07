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
using System.Net;
using System.IO;

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

            var rcvMsgService = new Intent(this, typeof(ReceiveMessagesTaskService));
            StartService(rcvMsgService);

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

            MessagingCenter.Subscribe<string>(this, "Download", message =>
            {
                Download(message);
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

        private async void Download(string _url)
        {
            try
            {
                var webClient = new WebClient();
                var url = new Uri(_url);
                byte[] x = await webClient.DownloadDataTaskAsync(url);

                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string localFilename = "com.arcelormittal.cml.prd.apk";
                string localPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/" + localFilename;
                if (File.Exists(localPath))
                {
                    File.Delete(@localPath);                    
                }
                FileStream fs = File.Create(@localPath);
                fs.Close();
                File.WriteAllBytes(localPath, x);
                //
                // writes to local storage  
               
                Intent promptInstall = new Intent(Intent.ActionView).SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(localPath)), "application/vnd.android.package-archive");
                promptInstall.AddFlags(ActivityFlags.NewTask);
                // The PendingIntent to launch our activity if the user selects this notification
                PendingIntent contentIntent = PendingIntent.GetActivity(this, 0, promptInstall, 0);
                var nBuilder = new Notification.Builder(this);
                StartActivity(promptInstall);   
            }
            catch (Exception e)
            {

            }
        }
    }
}

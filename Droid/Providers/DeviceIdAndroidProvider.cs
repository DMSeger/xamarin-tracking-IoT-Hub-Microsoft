using Android.Content;
using Android.Telephony;

using CML.Interfaces;

namespace CML.Droid.Providers
{
    public class DeviceIdAndroidProvider : IDeviceId
    {
        public string GetDeviceId()
        {
            TelephonyManager mTelephonyMgr = (TelephonyManager)MainActivity.Instance.ApplicationContext.GetSystemService(Context.TelephonyService);

            return mTelephonyMgr.DeviceId;
        }
    }
}
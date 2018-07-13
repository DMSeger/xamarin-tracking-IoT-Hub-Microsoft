using UIKit;
using CoreLocation;

namespace CML.iOS
{
    public class LocationManager
    {
        protected CLLocationManager locMgr;

        public LocationManager()
        {
            this.locMgr = new CLLocationManager();
            this.locMgr.PausesLocationUpdatesAutomatically = false;

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                locMgr.RequestAlwaysAuthorization(); // works in background
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                locMgr.AllowsBackgroundLocationUpdates = true;
            }
            if (CLLocationManager.LocationServicesEnabled)
            {
                //TODO - Invoke shared code (?)
            }
        }

        public CLLocationManager LocMgr
        {
            get { return locMgr; }
        }
    }
}
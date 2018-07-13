using CML.Messages;
using CML.ViewModel;
using CML.Views;
using Xamarin.Forms;

namespace CML
{
    public partial class App : Application
    {
        public static ViewModelLocator _locator;

        public static ViewModelLocator Locator
        {
            get
            {
                return _locator ?? (_locator = new ViewModelLocator());
            }
        }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPageView();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            //MessagingCenter.Send(new WakeDeviceMessage(), "WakeDeviceMessage");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

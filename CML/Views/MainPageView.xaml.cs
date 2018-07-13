using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CML.ViewModel;

namespace CML.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageView : ContentPage
    {
        private MainPageViewModel _viewModel;
        private bool _firstAppearing;

        public MainPageView()
        {
            InitializeComponent();
            _viewModel = App.Locator.MainPageViewModel;
            BindingContext = _viewModel;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e) { }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!_firstAppearing)
            {
                _firstAppearing = true;
                _viewModel.InitializeDeviceId();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Akavache;
using CML.Environment;
using CML.Interfaces;
using CML.Messages;
using CML.Model;
using CML.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Plugin.Battery;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace CML.ViewModel
{
    public class MainPageViewModel : ViewModelBase
    {
        #region Attributes

        private ImageSource _ActionImage;
        private string _latLonInfo;
        private string _speedCompare;
        private string _notSentMsg;
        private int _sendMsgFail;
        private string _connectionString;
        private string _actionText;
        private bool _aquisitionStart;
        
        private bool _tripStarted;
        private string _deviceId;

        private bool _dataAquisitionFinished;
        private bool _sendDataFinished;
        private string _versionName;

        #endregion

        #region Properties

        public ImageSource ActionImage
        {
            get { return _ActionImage; }
            set
            {
                Set(() => ActionImage, ref _ActionImage, value);
            }
        }

        public string LatLonInfo
        {
            get { return _latLonInfo; }
            set
            {
                Set(() => LatLonInfo, ref _latLonInfo, value);
            }
        }

        public string SpeedCompare
        {
            get { return _speedCompare; }
            set
            {
                Set(() => SpeedCompare, ref _speedCompare, value);
            }
        }

        public string NotSentMsg
        {
            get { return _notSentMsg; }
            set
            {
                Set(() => NotSentMsg, ref _notSentMsg, value);
            }
        }

        public int SendMsgFail
        {
            get { return _sendMsgFail; }
            set
            {
                Set(() => SendMsgFail, ref _sendMsgFail, value);
            }
        }

        public string ActionText
        {
            get { return _actionText; }
            set
            {
                Set(() => ActionText, ref _actionText, value);
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                Set(() => ConnectionString, ref _connectionString, value);
            }
        }

        public bool AquisitionStart
        {
            get { return _aquisitionStart; }
            set
            {
                Set(() => AquisitionStart, ref _aquisitionStart, value);
            }
        }

        public bool TripStarted
        {
            get { return _tripStarted; }
            set
            {
                Set(() => TripStarted, ref _tripStarted, value);
                ScreenTapCommand.RaiseCanExecuteChanged();
            }
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set
            {
                Set(() => DeviceId, ref _deviceId, value);
            }
        }

        public bool DataAquisitionFinished
        {
            get { return _dataAquisitionFinished; }
            set
            {
                Set(() => DataAquisitionFinished, ref _dataAquisitionFinished, value);
            }
        }

        public bool SendDataFinished
        {
            get { return _sendDataFinished; }
            set
            {
                Set(() => SendDataFinished, ref _sendDataFinished, value);
            }
        }

        public string VersionName
        {
            get { return _versionName; }
            set
            {
                Set(() => VersionName, ref _versionName, value);
            }
        }

        #endregion

        #region Commands

        private AccessRepo accessRepo = AccessRepo.getInstance();
        private RelayCommand _screenTapCommand;
        public RelayCommand ScreenTapCommand
        {
            get
            {
                return _screenTapCommand ?? (_screenTapCommand = new RelayCommand(AquisitionTriggered));//, () => { return AquisitionEnbl; }));
            }
        }

        #endregion

        #region Constructor

        static MainPageViewModel _MainPageViewModel;
        public MainPageViewModel()
        {
            AccessRepo.InitializeDataBank();
            ActionImage = ImageSource.FromFile("play.png");
            ActionText = "Inicializando sistema de aquisição...";
            AquisitionStart = false;
            TripStarted = false;
            LatLonInfo = "Lat.: -- / Long.: --";
            SpeedCompare = "Veloc. Atual: 0 km/h;";
            NotSentMsg = "Aguard. Envio: 0";
            _MainPageViewModel = this;

            ConnectionString = AppConstants.DEVICE_CONNECTION_STRING;
            VersionName = string.Format("V{0}", AppConstants.DEVICE_APP_VERSION_NUMBER);

            HandleGeneratedMessages();
            DataAquisitionFinished = true;
            SendDataFinished = true;
            UpdateInitialState();
        }

        #endregion

        #region Layout Control

        private async void UpdateInitialState()
        {
            try
            {
               var state = await accessRepo.ReadState();
               AquisitionStart = state == true;
            }
            catch
            {
                AquisitionStart = false;
            }

            if (AquisitionStart)
            {
                SendMsgFail = 0;
                ActionImage = ImageSource.FromFile("pause.png");
                ActionText = "Viagem em ANDAMENTO. Toque no botão para ENCERRAR a viagem.";
                MessagingCenter.Send(new StartLongRunningTaskMessage(), "StartLongRunningTaskMessage");
            }
            else
            {
                ActionImage = ImageSource.FromFile("play.png");
                ActionText = "Toque no botão para INICIAR a viagem.";
            }
        }

        public void InitializeDeviceId()
        {
            DeviceId = "ID: " + SimpleIoc.Default.GetInstance<IDeviceId>().GetDeviceId();
        }

        private void AquisitionTriggered()
        {
            AquisitionStart = !AquisitionStart;

            if (AquisitionStart)
            {
                StartTrip();
            }
            else
            {
                EndTrip();
            }
        }

        async void EndTrip()
        {
            var popUp = new ConfirmConfig();
            popUp.SetTitle("ATENÇÃO!");
            popUp.SetMessage("Deseja mesmo ENCERRAR viagem?");
            popUp.SetCancelText("Cancelar");
            popUp.SetOkText("Aceitar");
            var resposta = await UserDialogs.Instance.ConfirmAsync(popUp);
            if (resposta)
            {
                ActionImage = ImageSource.FromFile("play.png");
                ActionText = "Toque no botão para INICIAR a viagem.";
                MessagingCenter.Send(new StopLongRunningTaskMessage(), "StopLongRunningTaskMessage");                
                AquisitionStart = false;
                var endTrip = true;
                await AcquireData(endTrip);
                TripStarted = false;
                await SendData();
            }
            else
            {
                AquisitionStart = true;
            }
        }

        async void StartTrip()
        {
            var popUp = new ConfirmConfig();
            popUp.SetTitle("ATENÇÃO!");
            popUp.SetMessage("Deseja mesmo INICIAR viagem?");
            popUp.SetCancelText("Cancelar");
            popUp.SetOkText("Aceitar");
            var resposta = await UserDialogs.Instance.ConfirmAsync(popUp);
            if (resposta)
            {
                TripStarted = true;
                SendMsgFail = 0;
                ActionImage = ImageSource.FromFile("pause.png");
                ActionText = "Viagem em ANDAMENTO. Toque no botão para ENCERRAR viagem.";
                MessagingCenter.Send(new StartLongRunningTaskMessage(), "StartLongRunningTaskMessage");
                AquisitionStart = true;
            }
            else
            {
                AquisitionStart = false;
            }
        }

        private void UpdateScrnInfo(Position LatLonPosition)
        {
            LatLonInfo = "Lat.: " + Math.Round(LatLonPosition.Latitude, 5) + " / Long.: " + Math.Round(LatLonPosition.Longitude, 5);
            SpeedCompare = "Veloc.: " + Math.Round(LatLonPosition.Speed, 2) + " km/h";
        }

        #endregion
        
        #region Geolocation Control
        
        void HandleGeneratedMessages()
        {
            MessagingCenter.Subscribe<TickedMessage>(this, "TickedMessage", async message =>
            {
                if (DataAquisitionFinished)
                {
                    await AcquireData();
                }
            });

            MessagingCenter.Subscribe<ConnectAndSendMessage>(this, "ConnectAndSendMessage", async message =>
            {
                if (SendDataFinished)
                {
                    await SendData();
                }
            });
        }

        public async Task<Position> GetPosition()
        {
            //ACURACIDADE EM PES EH DE 20FT - 6.1m (aproximadamente)

            //avaliar nivel de acuracidade para a distancia em metros
            double[] v_accu_to_eval_meters = { 6.5d, 10d, 15d, 20d, 25d, 30d, 35d, 
                                               40d, 55d, 60d, 65d, 70d, 75d, 80d, 
                                               85d, 90d, 95d, 100d };

            //intervalo de tempo
            TimeSpan ts = TimeSpan.FromSeconds(3);

            Position position = null;

            IGeolocator l_locator = CrossGeolocator.Current;

            if (l_locator.IsGeolocationEnabled && l_locator.IsGeolocationAvailable)
            {
                bool isPositionAquired = false;

                foreach (double accuracyToEval in v_accu_to_eval_meters)
                {
                    if (!isPositionAquired)
                    {
                        l_locator.DesiredAccuracy = accuracyToEval;

                        position = await l_locator.GetPositionAsync(ts);

                        isPositionAquired = (position != null && position.Accuracy <= accuracyToEval);
                    }

                    if (isPositionAquired) break;
                }
            }

            return position;
        }
              
        public static async Task GetDataLocationSync()
        {
            try
            {
                if (_MainPageViewModel.DataAquisitionFinished) await _MainPageViewModel.AcquireData();
            }
            catch 
            {
                _MainPageViewModel.WakeUp();
            }
        }

        public async Task AcquireData(bool endTrip = false)
        {
            DataAquisitionFinished = false;

            try
            {
                await AccessRepo.Semaphore.WaitAsync();
                try
                {
                    await accessRepo.SaveState(AquisitionStart);
                }
                finally
                {
                    AccessRepo.Semaphore.Release();
                }

                await AccessRepo.Semaphore.WaitAsync();
            }
            catch 
            {
                WakeUp();
                return;
            }

            var _informationBuffer = new InformationBuffer();

            try
            {
                _informationBuffer = await BlobCache.UserAccount.GetObject<InformationBuffer>("buffer");
            }
            catch 
            {
                _informationBuffer.Buffer.Clear();
            }

            try
            {             
                var latLongPosition = await GetPosition();

                //Caso não consiga obter localização envia 500 como código de erro.
                var Latitude = 0d;
                var Longitude = 0d;
                var speed = 0d;
                var status = 500;

                if (latLongPosition != null)
                {
                    Latitude = latLongPosition.Latitude;
                    Longitude = latLongPosition.Longitude;
                    speed = latLongPosition.Speed;
                    status = 200; //200 é o código de conseguiu obter a localização.
                    UpdateScrnInfo(latLongPosition);
                }

                var info = new DeviceInformation()
                {
                    EndTrip = endTrip,
                    AquisitionDate = DateTime.Now,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    Speed = speed,
                    Battery = CrossBattery.Current.RemainingChargePercent,
                    DeviceId = SimpleIoc.Default.GetInstance<IDeviceId>().GetDeviceId(),
                    Status = status
                };

                _informationBuffer.Buffer.Add(info);

                await accessRepo.InsertData(_informationBuffer);
                  
                   
            }
            finally
            {
                AccessRepo.Semaphore.Release();
            }

            DataAquisitionFinished = true;
        }

        public static void SendLocationSync()
        {
            try
            {
                if (_MainPageViewModel.SendDataFinished) _MainPageViewModel.SendData().Wait(15000);
            }
            catch 
            {
                _MainPageViewModel.WakeUp();
            }

        }

        private async Task SendData(string message = "")
        {
            DeviceClient _deviceClient = null;

            try
            {
                SendDataFinished = false;

                var _informationBuffer = new InformationBuffer();
                _informationBuffer.Buffer = new List<DeviceInformation>();

                await AccessRepo.Semaphore.WaitAsync();

                try
                {
                    _informationBuffer = await BlobCache.UserAccount.GetObject<InformationBuffer>("buffer");
                }
                catch 
                {
                    _informationBuffer.Buffer.Clear();
                }                               

                if (_informationBuffer.Buffer.Count() > 0)
                {
                    try
                    {
                        var bufferJson = Serialize(JsonConvert.SerializeObject(_informationBuffer));

                        var msg = new Message(bufferJson);
                        msg.Properties.Add(AppConstants.DEVICE_APP_PROP_MAP_SYSORI, AppConstants.DEVICE_DEFAULT_NAME);
                        msg.Properties.Add(AppConstants.DEVICE_APP_PROP_MAP_CODINT, AppConstants.DEVICE_DEFAULT_MSG_ID);

                        _deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString, TransportType.Http1);
                        int count = _informationBuffer.Buffer.Count();

                        await _deviceClient.SendEventAsync(msg);

                        if (_informationBuffer.Buffer.Count() == count)
                        {
                            _informationBuffer.Buffer.Clear();
                            await accessRepo.DumpCache("buffer");
                            if (SendMsgFail > 0)
                            {
                                SendMsgFail = 0;
                                NotSentMsg = "Aguard. Envio: " + SendMsgFail;
                            }
                        }
                        else
                        {
                            _informationBuffer.Buffer.RemoveRange(0, count);
                            await accessRepo.InsertData(_informationBuffer);

                            SendMsgFail = _informationBuffer.Buffer.Count();
                            NotSentMsg = "Aguard. Envio: " + SendMsgFail;
                        }
                    }
                    catch
                    {
                        WakeUp();

                        SendMsgFail = _informationBuffer.Buffer.Count();
                        NotSentMsg = "Aguard. Envio: " + SendMsgFail;
                    }
                }
            }
            finally
            {
                if (_deviceClient != null)
                {
                    await _deviceClient.CloseAsync();

                    _deviceClient.Dispose();
                }

                AccessRepo.Semaphore.Release();

                SendDataFinished = true;

                GC.Collect();
            }
        }
                
        #endregion

        #region Utils Methods
        

        private async void ClearBuffer()
        {
            var _informationBuffer = new InformationBuffer();
            _informationBuffer.Buffer = new List<DeviceInformation>();

            await AccessRepo.Semaphore.WaitAsync();

            try
            {
                _informationBuffer = await BlobCache.UserAccount.GetObject<InformationBuffer>("buffer");
            }
            catch
            {
                _informationBuffer.Buffer.Clear();
            }

            _informationBuffer.Buffer.Clear();
            await accessRepo.DumpCache("buffer");
        }

        public byte[] Serialize(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }

        private async Task ClearBufferAsync(AccessRepo accessRepo, InformationBuffer _informationBuffer)
        {
            try
            {
                await accessRepo.DumpCache("buffer");
                _informationBuffer.Buffer.RemoveRange(0, _informationBuffer.Buffer.Count() - 1);
                await accessRepo.InsertData(_informationBuffer);
            }
            catch {; }
        }

        private void WakeUp()
        {
            try
            {
                var wakeMessage = new WakeDeviceMessage();

                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(wakeMessage, "WakeDeviceMessage");
                });
            }
            catch {; }
        }

        #endregion
    }

   
}

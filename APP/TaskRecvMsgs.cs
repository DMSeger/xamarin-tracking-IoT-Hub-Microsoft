using CML.Environment;
using Java.IO;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CML
{
    public class TaskRecvMsgs
    {
        public TaskRecvMsgs()
        {

        }

        public async Task RunCounter()
        {
            await Task.Run(async () => 
            {
                while (true)
                {
                    try
                    {
                        DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(AppConstants.DEVICE_CONNECTION_STRING, TransportType.Http1);
                        var msg = await _deviceClient.ReceiveAsync();
                        if (msg == null)
                            continue;
                        IDictionary<string, string> props = msg.Properties;
                        string url = props["download"];

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(url, "Download");
                        });
                    }
                    catch {; }
                    //Definir o que vai ter nessa mensagem..
                }
            });
        }
    }
}

using CML.Environment;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(AppConstants.DEVICE_CONNECTION_STRING, TransportType.Http1);

                    var msg = await _deviceClient.ReceiveAsync();

                    if (msg != null)
                        continue;

                    //Definir o que vai ter nessa mensagem..
                }
            });
        }
    }
}

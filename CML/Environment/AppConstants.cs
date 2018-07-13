
namespace CML.Environment
{
    public class AppConstants
    {
        //DEV/QA
        //public const string DEVICE_CONNECTION_STRING = "HostName=AZR-IOT-DEV.azure-devices.net;DeviceId=CML;SharedAccessKey=3cCRCGgJPW3xJ0ksM04odhL6E9b8EOyuHjy0M7dYw8s=";

        //PRD
        // public const string DEVICE_CONNECTION_STRING = "HostName=AMT-IOT-PRD.azure-devices.net;DeviceId=CML;SharedAccessKey=aSozh1thV7dwY3uSpJGeS5/ecf0k5F0sa9qaslu4lmc=";

        //DEV/QA
        public const string DEVICE_CONNECTION_STRING = "HostName=cml.azure-devices.net;DeviceId=cml;SharedAccessKey=3qvkQhGkDb6ljo/EZ0xDNFIzGiIWMJAhgoie9RZMzXc=";

        //APP VERSION NUMBER
        public const string DEVICE_APP_VERSION_NUMBER = "1.13";

        //PRORIEDADE APLICACAO - IDENTIFICADOR DO APLICATIVO
        public const string DEVICE_APP_PROP_MAP_SYSORI = "MAP_SYSORI";

        //PROPRIEDADE APLICACAO - CODIGO DE INTERFACE PADRAO
        public const string DEVICE_APP_PROP_MAP_CODINT = "MAP_CODINT";

        //NOME DO APLICATIVO PADRAO
        public const string DEVICE_DEFAULT_NAME = "CML";

        //ID DO TIPO DA MENSAGEM PADRAO DO APLICATIVO
        public const string DEVICE_DEFAULT_MSG_ID = "ST00CML01";
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CCSLightControlTool
{

    #region Enums

    public enum CHANNEL
    {
        [Description("00")]
        L1,
        [Description("01")]
        L2,
        [Description("02")]
        L3,
        [Description("03")]
        L4,
        [Description("04")]
        L5,
        [Description("05")]
        L6,
        [Description("06")]
        L7,
        [Description("07")]
        L8,
        [Description("FF")]
        ALL
    }
    public enum INSTRUCTION
    {

        [Description("F")]
        LightIntensitySetting,

        [Description("S")]
        LightingModeSetting,

        [Description("L")]
        OnOffSetting,

        [Description("M")]
        SettingStatusCheck,

        [Description("C")]
        ErrorStatusCheck,

        [Description("R")]
        AllChannelInitialization,

        [Description("E01")]
        IPAddress,

        [Description("E02")]
        SubnetMask,

        [Description("E03")]
        DefaultGateway,

        [Description("E04")]
        ReceptionPortSetting
    }
    public enum STROBE
    {
        [Description("01")]
        _40µs,
        [Description("02")]
        _80µs,
        [Description("03")]
        _120µs,
        [Description("04")]
        _200µs,
        [Description("05")]
        _600µs,
        [Description("06")]
        _1ms,
        [Description("07")]
        _4ms,
        [Description("08")]
        _10ms,
        [Description("09")]
        _20ms,
        [Description("10")]
        _40ms
    }

    #endregion

    public class CCSLightControl
    {
        #region Attributes

        private const string header = "@";
        private const string delimeter = "\r\n";

        #endregion

        #region Constructor

        public CCSLightControl()
        {
            this.IPAddress = new IPAddress(new byte[] { 192, 168, 0, 2 });
            this.Port = 40001;
            IPEndPoint = new IPEndPoint(this.IPAddress, this.Port);
        }
        public CCSLightControl(byte[] ip, int port)
        {
            this.IPAddress = new IPAddress(ip);
            this.Port = port;
            IPEndPoint = new IPEndPoint(this.IPAddress, this.Port);
        }

        #endregion

        #region Properties


        private IPAddress IPAddress { get; set; }
        private int Port { get; set; }
        private IPEndPoint IPEndPoint { get; set; }


        #endregion

        #region Private Methods

        private HttpStatusCode SendData(byte[] command, string info = "")
        {
            string response = "";
            try
            {
                Thread.Sleep(1000);
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {

                    var result = socket.BeginConnect(this.IPEndPoint, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(1000, true);
                    if (success)
                    {
                        int bytesSent = socket.Send(command);
                        response = socket.Receive();
                        socket.EndConnect(result);

                        socket.Disconnect(false);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    else
                    {
                        socket.Disconnect(false);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        throw new SocketException(10060); // Connection timed out.
                    }
                }
            }
            catch (SocketException)
            {
                response = null;
            }
            return ResponseHandler(info, response);
        }
        private byte[] GetCommand(string cmd)
        {
            string command = $"{header}{cmd}";
            string chkSum = GetCheckSum(command);
            return $"{command}{chkSum}{delimeter}".GetBytes();
        }
        private string GetCheckSum(string command)
        {
            char[] chkSum = (command.GetBytes().Select(x => (int)x)).Sum().ToString("X").ToCharArray();
            return $"{chkSum[chkSum.Length - 2]}{ chkSum[chkSum.Length - 1]}";
        }
        private string GetLongIPAddress(byte[] ip) => $"{((int)ip[0]).ToString("000")}.{((int)ip[1]).ToString("000")}.{((int)ip[2]).ToString("000")}.{((int)ip[3]).ToString("000")}";

        private HttpStatusCode ResponseHandler(string info, string response)
        {
            if (response == null)
            {
                MessageBox.Show("Connection Timeout.\n Could not connect to device.", "Timeout error", MessageBoxButton.OK, MessageBoxImage.Error);
                return HttpStatusCode.RequestTimeout;
            }
            if (!ResponseSucces(response))
            {
                MessageBox.Show($"Task:\n{info}\nStatus: Unsuccessful\nError code: {response}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return HttpStatusCode.InternalServerError;
            }
            return HttpStatusCode.OK;
        }
        private bool ResponseSucces(string response)
        {
            if (response.Contains($"{header}00O"))
                return true;
            return false;
        }

        #endregion

        #region Public Methods

        public HttpStatusCode SetIpAddress(byte[] ip)
        {
            byte[] cmd = GetCommand($"{CHANNEL.L1.GetEnumDescription()}{INSTRUCTION.IPAddress.GetEnumDescription()}{GetLongIPAddress(ip)}");
            return SendData(cmd, "Setting the IP Address");
        }
        public HttpStatusCode SetSubnetMask(byte[] subnetMask)
        {
            byte[] cmd = GetCommand($"{CHANNEL.L1.GetEnumDescription()}{INSTRUCTION.SubnetMask.GetEnumDescription()}{GetLongIPAddress(subnetMask)}");
            return SendData(cmd, "Setting the Subnet Mask");
        }
        public HttpStatusCode SetDefaultGateway(byte[] defaultGateway)
        {
            byte[] cmd = GetCommand($"{CHANNEL.L1.GetEnumDescription()}{INSTRUCTION.DefaultGateway.GetEnumDescription()}{GetLongIPAddress(defaultGateway)}");
            return SendData(cmd, "Setting the Default Gateway");
        }
        public HttpStatusCode SetPort(int port)
        {
            byte[] cmd = GetCommand($"{CHANNEL.L1.GetEnumDescription()}{INSTRUCTION.ReceptionPortSetting.GetEnumDescription()}{port.ToString("00000")}");
            return SendData(cmd,"Setting the port number");
        }

        public void SetLightIntensity(CHANNEL channel, byte instensity)
        {
            byte[] cmd = GetCommand($"{channel.GetEnumDescription()}{INSTRUCTION.LightIntensitySetting.GetEnumDescription()}{((int)instensity).ToString("000")}");
            SendData(cmd, "Setting the light intensisty");
        }
        public void SetLightModeToContinuousMode(CHANNEL channel)
        {
            byte[] cmd = GetCommand($"{channel.GetEnumDescription()}{INSTRUCTION.LightingModeSetting.GetEnumDescription()}00");
            SendData(cmd, "Setting channel lightmode to continuous mode");
        }
        public void SetLightModeToStrobeMode(CHANNEL channel, STROBE strobeSetting)
        {
            byte[] cmd = GetCommand($"{channel.GetEnumDescription()}{INSTRUCTION.LightingModeSetting.GetEnumDescription()}{strobeSetting.GetEnumDescription()}");
            SendData(cmd, "Setting channel lightmode to stobe mode");
        }
        public void SetOnOff(CHANNEL channel, bool turnOn)
        {
            string isOn = turnOn ? "1" : "0";
            byte[] cmd = GetCommand($"{channel.GetEnumDescription()}{INSTRUCTION.OnOffSetting.GetEnumDescription()}{isOn}");
            SendData(cmd, "Setting channel on/off");
        }
        public void DoStatusCheck(CHANNEL channel)
        {
            byte[] cmd = GetCommand($"{channel.GetEnumDescription()}{INSTRUCTION.SettingStatusCheck.GetEnumDescription()}");
            SendData(cmd, "Performing a status check");
        }
        public HttpStatusCode DoErrorStatusCheck()
        {
            byte[] cmd = GetCommand($"{CHANNEL.L1.GetEnumDescription()}{INSTRUCTION.ErrorStatusCheck.GetEnumDescription()}");
            return SendData(cmd, "Performing an error check");
        }
        
        #endregion

    }

    public static class Extentions
    {
        public static byte[] GetBytes(this string str, Encoding encoding = null)
        {
            if (encoding == null)
                return Encoding.ASCII.GetBytes(str);
            return encoding.GetBytes(str);
        }

        public static string Receive(this Socket socket, int bufferSize = 1024, Encoding encoding = null, int index = 0)
        {
            byte[] bytes = new byte[bufferSize];
            int bytesRec = socket.Receive(bytes);
            string receivedMessage = string.Empty;
            if (encoding == null)
                receivedMessage = Encoding.ASCII.GetString(bytes, index, bytesRec);
            else
                receivedMessage = encoding.GetString(bytes, index, bytesRec);
            return receivedMessage;
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

}

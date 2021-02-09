using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace EasyCom.Connection.Serial
{
    public class Settings:IConnectionSettings
    {
        public int ComPort { get; set; } = -1;
        public uint Baudrate { get; set; } = 0;
        public bool RTSEnable { get; set; } = false;
        public bool DTREnable { get; set; } = false;
        public BasicItem<ushort> DataBits { get; set; } = null;
        public BasicItem<StopBits> StopBits { get; set; } = null;
        public BasicItem<Parity> Parity { get; set; } = null;
        public BasicItem<Handshake> Handshake { get; set; } = null;

        public bool Equal(Settings settings)
        {

            if (ComPort != settings.ComPort)
                return false;
            if (Baudrate != settings.Baudrate)
                return false;
            if (RTSEnable != settings.RTSEnable)
                return false;
            if (DTREnable != settings.DTREnable)
                return false;
            if (DataBits.Value != settings.DataBits.Value)
                return false;
            if (StopBits.Value != settings.StopBits.Value)
                return false;
            if (Parity.Value != settings.Parity.Value)
                return false;
            if (Handshake.Value != settings.Handshake.Value)
                return false;
            return true;
        }

        public void Parse(string key, string value, string report)
        {
            Settings ConnectionSettings = this;
            if (key is null)
                return;
            if (key.Equals("sercfg", StringComparison.InvariantCulture))
            {

                //19200,8,n,1,N

                //@"([0-9]+),([5-8]),([noems]),(1|1\.5|2),([NXRD])"
                Match cmpResult = Regex.Match(value, @"(.+),(.+),(.+),(.+),(.+)");
                StringBuilder resultStr = new StringBuilder();

                Object optionResult;
                string wrongArgumentStr = "\n{0} '{1}' is wrong argument!";
                if (cmpResult.Success)
                {
                    Connection.Serial.Options options = Connection.Serial.PageSetting.SerialOptions;

                    Match BaudrateMatch = Regex.Match(cmpResult.Groups[1].Value, @"[0-9]+");
                    if (BaudrateMatch.Success)
                        ConnectionSettings.Baudrate = uint.Parse(BaudrateMatch.Value, CultureInfo.InvariantCulture);
                    else
                        resultStr.Append(string.Format(CultureInfo.InvariantCulture, wrongArgumentStr, "Baudrate", cmpResult.Groups[1].Value));

                    optionResult = options.DataBitsList.getOptionByName(cmpResult.Groups[2].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.DataBits = (BasicItem<ushort>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(CultureInfo.InvariantCulture, wrongArgumentStr, "Data Bits", cmpResult.Groups[2].Value));


                    optionResult = options.ParityList.getOptionByName(cmpResult.Groups[3].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.Parity = (BasicItem<System.IO.Ports.Parity>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(CultureInfo.InvariantCulture, wrongArgumentStr, "Parity", cmpResult.Groups[3].Value));

                    optionResult = options.StopBitsList.getOptionByName(cmpResult.Groups[4].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.StopBits = (BasicItem<System.IO.Ports.StopBits>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(CultureInfo.InvariantCulture, wrongArgumentStr, "Stop Bit", cmpResult.Groups[4].Value));

                    optionResult = options.HandshakeList.getOptionByName(cmpResult.Groups[5].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.Handshake = (BasicItem<System.IO.Ports.Handshake>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(CultureInfo.InvariantCulture, wrongArgumentStr, "Handshake", cmpResult.Groups[5].Value));

                    /*
                    Console.WriteLine(resultStr);
                    Debug.WriteLine(ConnectionSettings.Baudrate, "Test1");
                    */
                    /*
                    string DataBits = cmpResult.Groups[2].Value;
                    string Parity = cmpResult.Groups[3].Value;
                    int StopBit = int.Parse(cmpResult.Groups[4].Value, CultureInfo.InvariantCulture);
                    string FlowControl = cmpResult.Groups[5].Value;
                    */

                }
            }
            else if (key.Equals("dtr", StringComparison.InvariantCulture))
            {
                if (value != null)
                {
                    if (value.Equals("1", StringComparison.InvariantCulture))
                        ConnectionSettings.DTREnable = true;
                    else
                        ConnectionSettings.DTREnable = false;
                }

            }
            else if (key.Equals("rts", StringComparison.InvariantCulture))
            {
                if (value != null)
                {
                    if (value.Equals("1", StringComparison.InvariantCulture))
                        ConnectionSettings.RTSEnable = true;
                    else
                        ConnectionSettings.RTSEnable = false;
                }
            }
        }
    }
}

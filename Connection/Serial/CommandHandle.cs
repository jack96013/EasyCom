using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyCom.Connection.Serial
{
    public partial class SerialHelper
    {
        public void CommandHandle(string key, string value, string report)
        {
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
                        resultStr.Append(string.Format(wrongArgumentStr, "Data Bits", cmpResult.Groups[2].Value));


                    optionResult = options.ParityList.getOptionByName(cmpResult.Groups[3].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.Parity = (BasicItem<System.IO.Ports.Parity>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(wrongArgumentStr, "Parity", cmpResult.Groups[3].Value));

                    optionResult = options.StopBitsList.getOptionByName(cmpResult.Groups[4].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.StopBits = (BasicItem<System.IO.Ports.StopBits>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(wrongArgumentStr, "Stop Bit", cmpResult.Groups[4].Value));

                    optionResult = options.HandshakeList.getOptionByName(cmpResult.Groups[5].Value);
                    if (optionResult != null)
                    {
                        ConnectionSettings.Handshake = (BasicItem<System.IO.Ports.Handshake>)optionResult;
                    }
                    else
                        resultStr.Append(string.Format(wrongArgumentStr, "Handshake", cmpResult.Groups[5].Value));

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
                if (value.Equals("1", StringComparison.InvariantCulture))
                    ConnectionSettings.DTREnable = true;
                else
                    ConnectionSettings.DTREnable = false;
            }
            else if (key.Equals("rts", StringComparison.InvariantCulture))
            {
                if (value.Equals("1", StringComparison.InvariantCulture))
                    ConnectionSettings.RTSEnable = true;
                else
                    ConnectionSettings.RTSEnable = false;
            }
        }
    }
}

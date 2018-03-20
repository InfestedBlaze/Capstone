using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConsoleApp2_BlueToothTest05
{
    class Program
    {
        static bool _bContinue;
        static SerialPort _serialPort;
        static string[] _sIncomingDataArray;
        //static string _sDeviceID = "000666766D07"; //dummy device
        static string _sDeviceID = "000666D1FCBE"; //real device
        static string _sVirtualCOMPort = "";
        static int _iTimeOut = 25;

        static void Main(string[] args)
        {
            List<ManagementObject> lsMObj = new List<ManagementObject>();
            string[] sPortNames = SerialPort.GetPortNames(); // Get real serial ports
            List<string> otherSerial = new List<string>();

            #region ManagementObject 
            try
            {
                string sQuery = "SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0";
                ManagementObjectSearcher moSearcher = new ManagementObjectSearcher(sQuery);
                lsMObj = moSearcher.Get().Cast<ManagementObject>().ToList();

                foreach (ManagementObject obj in lsMObj)
                {
                    try
                    {
                        string caption = obj["Caption"].ToString();

                        if (obj.ToString().Contains(_sDeviceID) && caption.Contains("(COM"))
                        {
                            string sMatch = @"COM[0-9]";

                            Regex r = new Regex(sMatch, RegexOptions.IgnoreCase);

                            Match m = r.Match(caption);

                            while (m.Success)
                            {
                                _sVirtualCOMPort = m.ToString();
                                Console.WriteLine("'{0}' found at position {1}", _sVirtualCOMPort, m.Index);
                                m = m.NextMatch();
                            }

                            Thread readThread = new Thread(Read);

                            _serialPort = new SerialPort(_sVirtualCOMPort, 115200);


                            _serialPort.ReadTimeout = _iTimeOut;
                            _serialPort.WriteTimeout = _iTimeOut;

                            _serialPort.Open();
                            _bContinue = true;
                            readThread.Start();
                            readThread.Join();
                            _serialPort.Close();
                        }
                    }
                    catch
                    { }
                }
                moSearcher.Dispose();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Some exception..." + e);
            }

            Console.WriteLine("\nNo device found");
            Console.ReadKey();

            #endregion
        }

        public static void Read()
        {
            while (_bContinue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    _sIncomingDataArray = message.Split(',');

                    foreach (var item in _sIncomingDataArray)
                    {
                        if (item != "$DATA")
                        {
                            Console.Write("{0,7:G5} ", float.Parse(item));
                        }
                    }
                    Console.WriteLine();
                }
                catch (TimeoutException e)
                {
                    _bContinue = false;
                    //Trace.WriteLine(e);
                }
            }
        }
    }
}

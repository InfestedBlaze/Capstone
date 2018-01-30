using System;
using System.IO.Ports;
using System.Threading;
//using System.Collections;
//using System.Collections.Generic;

public class PortChat
{
    static bool _continue;
    static SerialPort _serialPort0, _serialPort1;
    static string[] _incomingDataArray0;
    static string[] _incomingDataArray1;

    public static void Main()
    {
        Thread readThread = new Thread(Read);

        // create a new SerialPort object with default settings.
        _serialPort0 = new SerialPort("COM4",115200);
        //_serialPort1 = new SerialPort("COM7",115200);

        // Set the read/write timeouts
        _serialPort0.ReadTimeout = 500;
        _serialPort0.WriteTimeout = 500;

        _serialPort0.Open();
        //_serialPort1.Open();
        _continue = true;
        readThread.Start();

        Console.WriteLine("Data follows: \r\n");

        readThread.Join();
        _serialPort0.Close();
        //_serialPort1.Close();
    }

    public static void Read()
    {
        while (_continue)
        {
            try
            {
                string message0 = _serialPort0.ReadLine();
                //string message1 = _serialPort1.ReadLine();

                _incomingDataArray0 = message0.Split(',');
                //_incomingDataArray1 = message1.Split(',');

                Console.Write("Board 1: ");
                foreach (var item in _incomingDataArray0)
                {   
                    if (item != "$DATA")
                    {
                        Console.Write("{0,7:G5} ", float.Parse(item));
                    }
                }
                Console.WriteLine();

                //Console.Write("Board 2: ");
                //foreach (var item in _incomingDataArray1)
                //{
                //    if (item != "$DATA")
                //    {
                //        Console.Write("{0,7:G5} ", float.Parse(item));
                //    }
                //}
                //Console.WriteLine();
                //Console.WriteLine();
            }
            catch (TimeoutException) { }
        }
    }
}
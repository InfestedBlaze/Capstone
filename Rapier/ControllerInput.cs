using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Ports;

public struct ControllerData
{
    public float rotX, rotY, rotZ, tranX, tranY, tranZ;

    public ControllerData(float RotationX, float RotationY, float RotationZ, float TranslationX, float TranslationY, float TranslationZ)
    {
        rotX = RotationX;
        rotY = RotationY;
        rotZ = RotationZ;
        tranX = TranslationX;
        tranY = TranslationY;
        tranZ = TranslationZ;
    }
}

public class ControllerInput {
    
    private SerialPort _serialPort;
    private Queue<ControllerData> rollingWindow = new Queue<ControllerData>();
    private byte rwSize = 10;

	// Use this for initialization
	public void OpenCommunication (string COMPort) {
        _serialPort = new SerialPort(COMPort, 115200, Parity.None, 8, StopBits.One);

        // Set the read/write timeouts
        _serialPort.ReadTimeout = 10;
        _serialPort.WriteTimeout = 10;

        //Open the communication between devices
        _serialPort.Open();
    }

    //Will read in the values of our controller
    public ControllerData readData()
    {
        //Need to make sure we don't have too much data. 
        //Goes into the dowhile with 4 pieces of data
        while(rollingWindow.Count >= rwSize)
        {
            rollingWindow.Dequeue();
        }

        do
        {
            //Array of controller data
            //Ordering:
            //Rotation:    X,Y,Z
            //Translation: X,Y,Z
            float[] transforms = new float[6];

            try
            {
                //Get in the CSV from the controller
                string input = _serialPort.ReadLine();
                string[] individualVals = input.Trim().Split(new char[] { ',' });

                //Parse the data as a float
                for (int i = 0; i < 6; i++)
                {
                    transforms[i] = float.Parse(individualVals[i]);
                }
            }
            catch { }

            //Sanitize our input, and put into our running window
            rollingWindow.Enqueue(sanitizeInput(transforms));

        } while (rollingWindow.Count < rwSize);

        //Set the average of our rolling window into our output
        ControllerData output = new ControllerData();
        output.rotX = rollingWindow.Average(data => data.rotX);
        output.rotY = rollingWindow.Average(data => data.rotY);
        output.rotZ = rollingWindow.Average(data => data.rotZ);
        output.tranX = rollingWindow.Average(data => data.tranX);
        output.tranY = rollingWindow.Average(data => data.tranY);
        output.tranZ = rollingWindow.Average(data => data.tranZ);

        return output;
    }

    private ControllerData sanitizeInput(float[] inputs)
    {
        //Sanitize rotation
        for(int i = 0; i < 3; i++)
        {
            inputs[i] = (inputs[i] / 10);
            if(Math.Abs(inputs[i]) < 0.3f || Math.Abs(inputs[i]) > 110)
            {
                inputs[i] = 0; //Use previous value, not 0
            }
        }
        //Sanitize translation
        for (int i = 3; i < 6; i++)
        {

        }

        return new ControllerData(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[5]);
    }

    //Will close the communication between controller and simulation
    public void CloseCommunication()
    {
        //Must have an open connection to close
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }
}

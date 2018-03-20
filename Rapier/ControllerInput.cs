using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Ports;

public struct ControllerData
{
    //We have labels for our rotation and translations
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
    public ControllerData(float[] data)
    {
        rotX = data[0];
        rotY = data[1];
        rotZ = data[2];
        tranX = data[3];
        tranY = data[4];
        tranZ = data[5];
    }

    public float this [int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return rotX;
                case 1:
                    return rotY;
                case 2:
                    return rotZ;
                case 3:
                    return tranX;
                case 4:
                    return tranY;
                case 5:
                    return tranZ;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

public class ControllerInput {
    
    private SerialPort _serialPort;
    private Queue<ControllerData> rollingWindow = new Queue<ControllerData>();
    private byte rwSize = 3;
    public const byte TIMEOUT = 10;

	// Use this for initialization
	public void OpenCommunication (string COMPort) {
        _serialPort = new SerialPort(COMPort, 115200, Parity.None, 8, StopBits.One);

        // Set the read/write timeouts to TIMEOUT ms
        _serialPort.ReadTimeout = TIMEOUT;
        _serialPort.WriteTimeout = TIMEOUT;

        //Open the communication between devices
        _serialPort.Open();
    }

    //Will read in the values of our controller
    public ControllerData readData()
    {
        //Need to make sure we don't have too much data. 
        //Goes into the do while with rwSize-1 pieces of data
        while(rollingWindow.Count >= rwSize)
        {
            rollingWindow.Dequeue();
        }

        //Always grab a new piece of data when we come through the function
        do
        {
            //Array of controller data
            //Ordering:
            //Rotation:    X,Y,Z
            //Translation: X,Y,Z
            float[] transforms = new float[6];

            //Try catch is used to let the serial port timeout.
            //If we don't read anything, let it continue without crashing.
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

            //Use this to get raw data
            //return new ControllerData(transforms);

            //Sanitize our input, and put into our running window
            rollingWindow.Enqueue(sanitizeInput(transforms));

        //Make sure that we have filled up our rolling window. Should only apply to startup
        } while (rollingWindow.Count < rwSize);
        
        //Return the last piece of data
        return rollingWindow.ToList().Last();
    }

    private ControllerData sanitizeInput(float[] inputs)
    {
        //We need at least two data points to extrapolate data
        if (rollingWindow.Count >= 2)
        {
            //Get our data points accessible
            List<ControllerData> rwList = rollingWindow.ToList();

            for (int i = 0; i < inputs.Length; i++)
            {
                //Our input is a bad piece of data, we want to extrapolate a point for it.
                if (inputs[i] == 0)
                {
                    //Get the specific piece of data needed, in degrees
                    inputs[i] = extrapolateData(rwList[0][i], rwList[1][i]);

                    //Convert back to degrees per second
                    inputs[i] /= (TIMEOUT / 1000f);
                }
                //We have a difference of greater than 10 degrees from our last point (rotation only)
                else if (i < 3 && Math.Abs(rwList[1][i] - (inputs[i] * (TIMEOUT / 1000f))) > 10)
                {
                    //Only +-30 from our last point
                    if (inputs[i] < 0)
                    {
                        inputs[i] = rwList[1][i] - 10;
                    }
                    else
                    {
                        inputs[i] = rwList[1][i] + 10;
                    }

                    //Convert back to degrees per second
                    inputs[i] /= (TIMEOUT / 1000f);
                }
            }
        }

        //Sanitize rotation
        for (int i = 0; i < 3; i++)
        {
            //Rotation is given in degrees/second
            //dividing by our period in seconds gives us degrees
            inputs[i] = (inputs[i] * (TIMEOUT / 1000f));

            //Set cutoff points for good data
            if (Math.Abs(inputs[i]) < 0.1f)
            {
                inputs[i] = 0;
            }
            else if (Math.Abs(inputs[i]) > 40) //Greater than |40|, set to +-40
            {
                inputs[i] = Math.Min(Math.Max(-40, inputs[i]), 40);
            }
        }
        //Sanitize translation
        for (int i = 3; i < 6; i++)
        {
            //Turn acceleration into displacement. (a * t^2 / 2)
            inputs[i] = inputs[i] * (float)Math.Pow((TIMEOUT / 1000f), 2) / 2; //input(m/s^2) * 25(ms)^2 /2 = input(m)
        }
        
        return new ControllerData(inputs);
    }
    
    private float extrapolateData(float arg1, float arg2)
    {
        //Assume a linear line, add the difference of (1, 2) back onto 2
        return arg2 + (arg2 - arg1);
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

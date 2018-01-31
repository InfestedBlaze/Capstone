using System.IO.Ports;

public class ControllerInput {

    private SerialPort _serialPort;

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
    public float[] readData()
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
        
        //Return the data
        return transforms;
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

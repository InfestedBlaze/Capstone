using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RapierBehavior : MonoBehaviour {

    private ControllerInput input;
    private GameObject mainCamera;
    private Vector3 translation;
    private Vector3 rotation;

	// Use this for initialization
	void Start ()
    {
        //Grab the camera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        //Initialize our controller
        input = new ControllerInput();
        input.OpenCommunication("COM4");

        Reset();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Get transforms from controller
        ControllerData transforms = input.readData();

        //Can be deleted later
        Debug.Log("Rotation: " + rotation.x + "x : " + rotation.y + "y : " + rotation.z + "z");
        Debug.Log("Translation: " + translation.x + "x : " + translation.y + "y : " + translation.z + "z");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\XRotation.txt", transforms.rotX.ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\YRotation.txt", transforms.rotY.ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\ZRotation.txt", transforms.rotZ.ToString() + "\r\n");
        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\XTranslation.txt", transforms.tranX.ToString() + "\r\n");
        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\YTranslation.txt", transforms.tranY.ToString() + "\r\n");
        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\ZTranslation.txt", transforms.tranZ.ToString() + "\r\n");

        //Change transforms
        rotation.x = (rotation.x + transforms.rotX) % 360; //Add the rotation to our current rotation. Don't go above 360
        rotation.y = (rotation.y - transforms.rotY) % 360;
        rotation.z = (rotation.z - transforms.rotZ) % 360;

        Vector3 gravity = effectGravity(rotation);

        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\Xgravity.txt", gravity.x.ToString() + "\r\n");
        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\Ygravity.txt", gravity.y.ToString() + "\r\n");
        //File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\Zgravity.txt", gravity.z.ToString() + "\r\n");

        translation.x += transforms.tranX - gravity.x;  //Add the delta translation to our current translation
        translation.y += transforms.tranY - gravity.y;  //While also removing gravity from our translation
        translation.z += transforms.tranZ - gravity.z;

        //Apply transforms
        this.transform.rotation = Quaternion.Euler(rotation);
        this.transform.position = translation;

        //Have the camera follow the sword
        Vector3 swordPos = this.transform.position; //Get position of sword
        mainCamera.transform.position = new Vector3(swordPos.x, swordPos.y + 0.777f, swordPos.z - 1.376f); //Set the position of the camera relative to sword
    }

    private void Reset()
    {
        //Reset transforms
        rotation = new Vector3(0,0,180); //180 is so the hand guard is facing down
        translation = new Vector3(0,0,0);

        //Apply transforms
        this.transform.rotation = Quaternion.Euler(rotation);
        this.transform.position = translation;
    }

    private void OnApplicationQuit()
    {
        input.CloseCommunication();
    }

    private Vector3 effectGravity(Vector3 orientation)
    {
        //0.003065625 is displacement due to gravity

        Vector3 gravity = new Vector3();
        //X effect of gravity
        gravity.x = -(9.81f) * (float)Math.Sin(orientation.y);
        //Y effect of gravity
        gravity.y =  (9.81f) * (float)Math.Cos(orientation.y) * (float)Math.Sin(orientation.z);
        //Z effect of gravity
        gravity.z =  (9.81f) * (float)Math.Cos(orientation.y) * (float)Math.Cos(orientation.z);

        //Acceleration to displacement conversion. a * t^2 / 2
        gravity.x = gravity.x * (float)Math.Pow((ControllerInput.TIMEOUT / 1000f), 2) / 2;
        gravity.y = gravity.y * (float)Math.Pow((ControllerInput.TIMEOUT / 1000f), 2) / 2;
        gravity.z = gravity.z * (float)Math.Pow((ControllerInput.TIMEOUT / 1000f), 2) / 2;

        return gravity;
    }
}

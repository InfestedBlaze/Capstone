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
        input.OpenCommunication();

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
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\XTranslation.txt", transforms.tranX.ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\YTranslation.txt", transforms.tranY.ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\ZTranslation.txt", transforms.tranZ.ToString() + "\r\n");

        //Change transforms
        rotation.x = (rotation.x + transforms.rotX) % 360; //Add the rotation to our current rotation. Don't go above 360
        rotation.y = (rotation.y - transforms.rotY) % 360;
        rotation.z = (rotation.z - transforms.rotZ) % 360;

        translation.x += transforms.tranX;  //Add the delta translation to our current translation
        translation.y += transforms.tranY;  
        translation.z += transforms.tranZ;

        translation.x %= 10;    //Don't let the translation go too far
        translation.y %= 10;
        translation.z %= 10;

        //Apply transforms
        this.transform.rotation = Quaternion.Euler(rotation);
        this.transform.position = translation;
        //Remove gravity from our position
        this.transform.Translate(0, 0, 0.0004905f, Space.World);

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
}

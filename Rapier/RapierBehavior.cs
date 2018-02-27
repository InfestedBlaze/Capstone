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

        Debug.Log("Rotation: " + rotation.x + "x : " + rotation.y + "y : " + rotation.z + "z");
        Debug.Log("Translation: " + translation.x + "x : " + translation.y + "y : " + translation.z + "z");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\XTranslation.txt", Mathf.Abs(transforms.tranX).ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\YTranslation.txt", Mathf.Abs(transforms.tranY).ToString() + "\r\n");
        File.AppendAllText(@"C:\Users\nwasylyshyn1\Desktop\ZTranslation.txt", Mathf.Abs(transforms.tranZ).ToString() + "\r\n");

        //Change transforms
        rotation.x = (rotation.x + transforms.rotX) % 360; //Add the rotation to our current rotation. Don't go above 360
        rotation.y = (rotation.y - transforms.rotY) % 360;
        rotation.z = (rotation.z - transforms.rotZ) % 360;

        translation.x += transforms.tranX;
        translation.y += transforms.tranY;
        translation.z += transforms.tranZ;

        //Apply transforms
        this.transform.rotation = Quaternion.Euler(rotation);
        //this.transform.position = translation;

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
        this.transform.eulerAngles = rotation;
        this.transform.position = translation;
    }

    private void OnApplicationQuit()
    {
        input.CloseCommunication();
    }
}

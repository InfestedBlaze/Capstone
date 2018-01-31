using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Get transforms from controller
        float[] transforms = input.readData();

        foreach(var i in transforms)
        {
            Debug.Log(i.ToString());
        }

        //Change transforms
        rotation    = new Vector3(transforms[0], transforms[1], transforms[2]);
        translation = new Vector3(transforms[3], transforms[4], transforms[5]);

        //Apply transforms
        this.transform.Rotate(rotation);
        this.transform.Translate(translation);

        //Have the camera follow the sword
        Vector3 swordPos = this.transform.position; //Get position of sword
        mainCamera.transform.position = new Vector3(swordPos.x, swordPos.y + 0.777f, swordPos.z - 1.376f); //Set the position of the sword
    }
}

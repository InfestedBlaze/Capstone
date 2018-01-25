using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapierBehavior : MonoBehaviour {

    private GameObject mainCamera;
    private Vector3 translation;
    private Vector3 rotation;

	// Use this for initialization
	void Start ()
    {
        //Grab the camera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        
        //Initialize all of our translations
        translation = new Vector3(0, 0, 0);
        //Initialize all of our rotations
        //Sword upright, handguard outwards
        rotation = new Vector3(-90, 0, 180);
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Get transforms from controller
        SetTranslation(++translation.x, translation.y, translation.z);
        SetRotation(rotation.x, rotation.y, ++rotation.z);

        //Apply transforms
        this.transform.position = translation;
        this.transform.eulerAngles = rotation;
        //Have the camera follow the sword
        mainCamera.transform.position = new Vector3(translation.x, translation.y + 0.777f, translation.z - 1.376f);
    }

    //This function is a public way of changing our rotation
    public void SetRotation(float X, float Y, float Z)
    {
        rotation = new Vector3(X, Y, Z);
        //this.transform.eulerAngles = rotation;
    }
    //This function is a public was of changing our translation
    public void SetTranslation(float X, float Y, float Z)
    {
        translation = new Vector3(X, Y, Z);
        //this.transform.position = translation;
    }
}

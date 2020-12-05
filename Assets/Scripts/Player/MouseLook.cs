using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerBody;

    World world;

    public bool inUI;

    float xRotation = 0f;

   

    // Start is called before the first frame update
    void Start()
    {
       world = GameObject.Find("World").GetComponent<World>();
        
    }

    // Update is called once per frame
    void Update()
    {

        mouseSensitivity = world.settings.mouseSensitivity;

        if(!inUI)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }

        
    }
}

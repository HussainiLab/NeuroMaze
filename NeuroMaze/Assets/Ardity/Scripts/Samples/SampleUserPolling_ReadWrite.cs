/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class SampleUserPolling_ReadWrite : MonoBehaviour
{
    public SerialController serialController;
    public Follower myPlayer;
    public float lastDistance, currentDistance = 0;
    public bool resetEncoderDistance, collectData;       // Flag to control reset of arduino optical enconder to zero revolutions

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
    }
    
    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // Will ask the arduino to reset the distance of the optical encoder on the
        // mouse treadmill to zero units. This happens once at each start of session. 
        if (resetEncoderDistance)
        {
            serialController.SendSerialMessage("R");
            Debug.Log("Reset sent");
            resetEncoderDistance = false;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            serialController.SendSerialMessage("Z");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            serialController.SendSerialMessage("X");
        }

        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        string message = serialController.ReadSerialMessage();
        if (message == null)
            return;

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
            // Debug.Log("Connection established");
            resetEncoderDistance = true;
        else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
            Debug.Log("Connection attempt failed or disconnection detected");
        else
            // Set sensor data to player distance
            // Debug.Log("Message arrived: " + message);
        
        if (collectData)
        {
            try
            {
                currentDistance = float.Parse(message);
                lastDistance = currentDistance;
                myPlayer.distanceTravelled = currentDistance;
            }
            catch
            {
                Debug.LogError("Read error at time: " + (Time.time));
                myPlayer.distanceTravelled = lastDistance;
            }
        }
    }
}

using UnityEngine;
using System.Collections;

public class DisplayActivator : MonoBehaviour
{

    /// <summary>
        /// Manages activation of multiple displays (i.e multiple monitors)
    /// </summary>
    
    // Simple loop to check for available displays and activate them. 
    // We are only expecting 2 additonal displays
    private void Start()
    {
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
}
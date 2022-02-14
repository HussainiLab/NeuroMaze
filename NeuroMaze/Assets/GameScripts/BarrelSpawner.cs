using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarrelSpawner : MonoBehaviour
{
    
    /// <summary>
        /// Class for handling object placements in maze for object maze
        /// Controls two sets of cylnder objects (left and right) along the linear track
    /// </summary>
    
    // Instantiate cylinders and lists of type 'Toggle' to control cylinder spawns
    public GameObject leftWallCylinders;
    public GameObject rightWallCylinders;
    // These lists will hold the Toggles which control spawn behavior
    public Toggle[] left_toggle_collection = new Toggle[13];
    public Toggle[] right_toggle_collection = new Toggle[13];

    // Flag to check if toggle states have been changed
    bool toggleChanged = false;
    // Instantiate Toggle object
    Toggle toggle;  

    // Call if any toggle state has changed 
    void ToggleValueChanged()
    {
        toggleChanged = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Attach toggle listeners to all toggles. Will invoke ToggleValueChanged() upon state change
        foreach (Toggle toggle in left_toggle_collection)
        {
            toggle.onValueChanged.AddListener(delegate
            {
                ToggleValueChanged();
            });
        }

        foreach (Toggle toggle in right_toggle_collection)
        {
            toggle.onValueChanged.AddListener(delegate
            {
                ToggleValueChanged();
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If a toggle state changes
        if (toggleChanged)
        {
            // Iterate though the toggles to find the state change
            int i = 0;
            foreach (Toggle toggle in left_toggle_collection)
            {
                // If we find an 'on' toggle, set the corresponding cylinder object to active (i.e spawn in)
                if (toggle.isOn)
                {
                    leftWallCylinders.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    leftWallCylinders.transform.GetChild(i).gameObject.SetActive(false);
                }
                i++;
            }

            i = 0;
            foreach (Toggle toggle in right_toggle_collection)
            {
                if (toggle.isOn)
                {
                    rightWallCylinders.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    rightWallCylinders.transform.GetChild(i).gameObject.SetActive(false);
                }
                i++;
            }
        }

        // Reset toggle changed flag 
        toggleChanged = false;
    }
}

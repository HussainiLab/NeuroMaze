using UnityEngine;
using UnityEngine.UI;

public class SetRewardTile : MonoBehaviour
{
    /// <summary>
        /// Class to control setting reward tiles in reward mode
    /// </summary>
    
    // Instantiate two lists of oggles, one list per set of wall tiles (left and right)
    public Toggle[] left_toggle_collection = new Toggle[13];
    public Toggle[] right_toggle_collection = new Toggle[13];
    public Follower myPlayer; // Instantiate player reference

    // Set flag to detect change in toggle state
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
        // Attach toggle listeners to all toggles
        for (int i = 0; i < 13; i++)
        {
            left_toggle_collection[i].onValueChanged.AddListener(delegate
            {
                ToggleValueChanged();
            });

            right_toggle_collection[i].onValueChanged.AddListener(delegate
            {
                ToggleValueChanged();
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Conditional will only run when a toggle value ahs been changed
        if (toggleChanged)
        {
            // Iterate through the toggles
            for (int i = 0; i < 13; i++)
            {
                Toggle left_toggle = left_toggle_collection[i];
                Toggle right_toggle = right_toggle_collection[i];
                
                // If we find an 'on' toggle 
                if (left_toggle.isOn)
                {
                    // Set the corresponding tile to a reward tile, and mark it green
                    myPlayer.Maze.transform.GetChild(1).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.green;
                    // Add this reward tile reference to a list (used Follower script for reward initiation)
                    if (!myPlayer.leftRewardTiles.Contains(i))
                    {
                        myPlayer.leftRewardTiles.Add(i);
                    }
                }
                else
                {
                    // If the toggle we encouter is off, set tile to default color
                    myPlayer.Maze.transform.GetChild(1).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.white;
                    // Remove this reward tile reference from list
                    if (myPlayer.leftRewardTiles.Contains(i))
                    {
                        myPlayer.leftRewardTiles.Remove(i);
                    }
                }

                // Same procedure as above for right toggles and tiles
                if (right_toggle.isOn)
                {
                    myPlayer.Maze.transform.GetChild(2).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.green;
                    if (!myPlayer.rightRewardTiles.Contains(i))
                    {
                        myPlayer.rightRewardTiles.Add(i);
                    }

                }
                else
                {
                    myPlayer.Maze.transform.GetChild(2).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.white;
                    if (myPlayer.rightRewardTiles.Contains(i))
                    {
                        myPlayer.rightRewardTiles.Remove(i);
                    }
                }
            }
        }

        // Reset toggle flag to prepare for next toggle change detection
        toggleChanged = false;
    }
}
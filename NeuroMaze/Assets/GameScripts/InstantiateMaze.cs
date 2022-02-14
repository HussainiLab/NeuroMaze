using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InstantiateMaze : MonoBehaviour
{
    /// <summary>
        /// Main class to control in-game player movement based off sensor input
        /// Also controls visual maze cues
    /// </summary>
    
    
    public GameObject darkHall, lightHall, objectHall;  // Instantiate Hall object types
    public Follower myPlayer;                           // Instantiate reference to player
    public Button placeObjects, rewardButton, backButton_1, backButton_2;   // Buttons for main UI
    public TMP_Dropdown dropMenu;                                           // Drop menu holds maze types to choose from

    // $ $ $
    public Toggle alternateToggle;
    public int prev_context, curr_context;
    
    public SampleUserPolling_ReadWrite arduino_due;     // Public access to arduino output
    
    public int prev_DropdownValue, curr_DropdownValue; // To check if dropdown menu has changed

    // We work with clones of the mazes so that we are not manipulating the original prefabs
    GameObject lightHallClone, darkHallClone, objectHallClone;

    // Start is called before the first frame update
    void Start()
    {
        // Set reward control to false (since the default maze does not involve reward signals)
        myPlayer.rewardSet = false;


        // Grab dropdown menu
        dropMenu = GetComponent<TMP_Dropdown>();
        prev_DropdownValue = curr_DropdownValue = dropMenu.value;
        curr_context = prev_context = prev_DropdownValue;

        // Grab 'place objects' button as well as 'back' button, and add listeners (for when clicked)
        Button btn_placeObj = placeObjects.GetComponent<Button>();
        Button btn_reward = rewardButton.GetComponent<Button>();
        Button btn_back_1 = backButton_1.GetComponent<Button>();
        Button btn_back_2 = backButton_2.GetComponent<Button>();

        btn_placeObj.onClick.AddListener(PlaceObjectClick);
        btn_reward.onClick.AddListener(rewardClick);
        btn_back_1.onClick.AddListener(delegate { backClick(0); });
        btn_back_2.onClick.AddListener(delegate { backClick(1); });

        // Instantiate all the mazes into the game. 
        lightHallClone = Instantiate(lightHall, new Vector3(9, -5, 21), Quaternion.identity);
        darkHallClone = Instantiate(darkHall, new Vector3(9, -5, 21), Quaternion.identity);

        // This maze is already Instantiated and just needs to be moved into the main player camera view.
        objectHallClone = GameObject.Find("objectHall");
        objectHallClone.transform.position = new Vector3(9, -5, 21);
        
        // Deactivate objects since default hall is lightHall
        objectHallClone.SetActive(false);
        darkHallClone.SetActive(false);

        myPlayer.Maze = lightHallClone; // Set a reference to the current default maze in 'Follower',
                                        // which is the script responsible for controling player movement and end plate flashing
    }

    // Sets maze and session properties when object placement option is chosen.
    void PlaceObjectClick()
    {
        arduino_due.resetEncoderDistance = true; // Reset arduino measurement of optical encoder to zero
        objectHallClone.SetActive(true);         // Activate the object hall
        dropMenu.value = 2;                      // Update the dropMenu value to reflect the object hall was chosen

        // Move the clone out of the main view to a secondary camera
        objectHallClone.transform.localPosition = new Vector3(-407.9749f, -180.6628f, -39.08943f);
        // Get rid of the walls and the roof so the user can see where the objects are palced in the maze
        objectHallClone.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(false);
        objectHallClone.transform.GetChild(3).gameObject.SetActive(false);
        objectHallClone.transform.GetChild(4).gameObject.SetActive(false);
        objectHallClone.transform.GetChild(5).gameObject.SetActive(false);
        objectHallClone.transform.GetChild(6).gameObject.SetActive(true);   // Place in dummy player capsule for reference
    }

    // If user chose reward option
    void rewardClick()
    {
        arduino_due.resetEncoderDistance = true; // Reset arduino measurement of optical encoder to zero

        // Highlight the reward tiles to green when switching to reward menu
        foreach (int tileIndex in myPlayer.leftRewardTiles)
        {
            objectHallClone.transform.GetChild(1).GetChild(0).GetChild(tileIndex).gameObject.transform.GetComponent<Renderer>().material.color = Color.green;
        }

        foreach (int tileIndex in myPlayer.rightRewardTiles)
        {
            objectHallClone.transform.GetChild(2).GetChild(0).GetChild(tileIndex).gameObject.transform.GetComponent<Renderer>().material.color = Color.green;
        }

        // Deactivate the other mazes except for object hall
        lightHallClone.SetActive(false);
        darkHallClone.SetActive(false);
        objectHallClone.SetActive(true);
        myPlayer.Maze = objectHallClone;
        // Deactivate the roof and the end plate
        objectHallClone.transform.GetChild(3).gameObject.SetActive(false);
        objectHallClone.transform.GetChild(4).gameObject.SetActive(false);
    }

    // Navigating back to the main UI
    void backClick(int state)
    {
        // If navigating back from set object positions menu
        if (state == 0)
        {
            // Place the object hall back to main camera view, reactivate the walls and roof.
            objectHallClone.transform.position = new Vector3(9, -5, 21);
            objectHallClone.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(true);
            objectHallClone.transform.GetChild(3).gameObject.SetActive(true);
            objectHallClone.transform.GetChild(4).gameObject.SetActive(true);
            objectHallClone.transform.GetChild(5).gameObject.SetActive(true);
            objectHallClone.transform.GetChild(6).gameObject.SetActive(false); // Deactivate dummy player capsule
        }

        // If navigating back from reward menu 
        else if (state == 1)
        {
            // Ensure we are set to object hall option upin main menu return
            dropMenu.value = 2;
            // Ensure roof and walls of object hall are activated
            objectHallClone.transform.GetChild(3).gameObject.SetActive(true);
            objectHallClone.transform.GetChild(4).gameObject.SetActive(true);

            // Ensure the green highlights on the tiles from setting reward tiles is removed
            for (int i = 0; i < 13; i++)
            {
                objectHallClone.transform.GetChild(1).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.white;
                objectHallClone.transform.GetChild(2).GetChild(0).GetChild(i).gameObject.transform.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // $ $ $
        if (curr_DropdownValue != prev_DropdownValue)
        {
            curr_context = curr_DropdownValue;
        }
        // If the user chooses a different maze option
        // This will only execute when there is a switch in the maze option.
        if (curr_context != prev_context)
        {

            // Switch out the maze with the option. 
            switch (curr_context)
            {
                // LightHall
                case 0:
                    lightHallClone.SetActive(true);
                    darkHallClone.SetActive(false);
                    objectHallClone.SetActive(false);
                    
                    // We make sure to set a reference to chosen maze in Follower,
                    // so that the Follower script can access the maze endplate (for flashing)
                    myPlayer.Maze = lightHallClone; 

                    myPlayer.rewardSet = false;
                    break;

                // DarkHall
                case 1:
                    lightHallClone.SetActive(false);
                    darkHallClone.SetActive(true);
                    objectHallClone.SetActive(false);
                    myPlayer.Maze = darkHallClone;
                    myPlayer.rewardSet = false;
                    break;

                // ObjectHall
                case 2:
                    lightHallClone.SetActive(false);
                    darkHallClone.SetActive(false);
                    objectHallClone.SetActive(true);
                    objectHallClone.transform.GetChild(6).gameObject.SetActive(false);
                    myPlayer.Maze = objectHallClone;
                    myPlayer.rewardSet = true;      // Only set reward flag to true when we switch to the object maze. Now the relay switch is active. 
                    break;
            }

            Debug.Log("Maze Switched!");
            // Set the prev and current choices to the same value
            // to prepare for detecting the next switch
            prev_DropdownValue = curr_DropdownValue;
            prev_context = curr_context; 

        }

        // Check and update dropdown menu option every frame
        curr_DropdownValue = dropMenu.value;
    }
}
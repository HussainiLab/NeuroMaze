using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathCreation;
using System.IO;
using SFB;
using System.Linq;

public class Follower : MonoBehaviour
{
    /// <summary>
        /// Main class to control in-game player movement based off sensor input
        /// Also controls visual maze cues
    /// </summary>
    
    public PathCreator pathCreator; // Sets path of player movement
    public GameObject Maze;         // Holds reference to maze type


    // $ $ $
    public Toggle alternateToggle;
    
    public Button startButton, concludeButton, 
        testRunningButton, chooseSaveDirectoryButton, exitButton;   // Main UI buttons
    public InstantiateMaze InstantiateMaze;                         // Holds reference to initial maze upon startup
    public SampleUserPolling_ReadWrite arduino_due;                 // Public access to arduino readings from optical encoder
    public TCPClient TCPClient;                                     // Access to TCP object for establishing
                                                                    // remote access to Intan neural recording software
    public bool rewardSet;                              // Flag that indicates a reward tile(s) is set.
    public float lastDistance, distanceTravelled = 0;   // Raw arduino input 
    public float cyclicPlayerDistance = 0;              // Variable that controls player distance separate from arduino input
                                                        
    // Lists to keep track of which tiles are reward tiles in object maze
    public List<int> leftRewardTiles;                   
    public List<int> rightRewardTiles;

    // String keeps track of directory chosen to save position file.
    private string _path;
    private string filename = "";
    private List<(float, float)> ListofPlayerDistance;

    float startTimer = -1;   // Timer used to detect how long subject is near a reward panel (for object maze)
    float elapsedTime = 0;   // Timer used to correct 
    bool goForward = true;   // Flag that ensures subject can't move forward during flash routine
    bool plateFlash = false; // Flag to indicate plate flashing routine is on/off
    bool recordData = false; // Flag to determine if position data should be actively stored. 
    
    // These boolean lists keep track of whether the reward state at each tile (for each wall) is allowed to invoke the arduino.
    // When the state is true, the tiles are marked 'valid' and can be used for reward signaling (i.e send a signal to arduino for relay 'on'), and vice versa. 
    // This extra layer of control is important to prevent the tiles from invoking a reward response more than once per lap. (Ex if the subject
    // keeps standing near a reward tile, it will keep receiving a reward and may be disinclined to keep moving through the maze.) 
    List<bool> leftRewardTileState;
    List<bool> rightRewardTileState;
    
    // This function will flash the endplate to signal the subject that the maze is 
    // teleporting back to the start
    IEnumerator FlashEndPlate()
    {
        // Grab the endplate of the maze
        GameObject endPlate = Maze.transform.GetChild(5).gameObject;
       
        // Flash plate black and white
        for (int i = 0; i < 5; i++)
        {
            endPlate.GetComponent<Renderer>().material.color = Color.white;
            yield return new WaitForSeconds(0.25f);
            endPlate.GetComponent<Renderer>().material.color = Color.black;
            yield return new WaitForSeconds(0.25f);
        }

        // After flashing is complete
        lastDistance = distanceTravelled; // Set last and preset distance to same value
        goForward = true;                 // Allow forward movement 
    }
    
    // Method to mediate relay switch signal to arduino. Used to engage/disengage reward.
    IEnumerator relayControl()
    {
        arduino_due.serialController.SendSerialMessage("Z");    // Serial message 'Z' will activate the relay
        yield return new WaitForSeconds(3f);
        arduino_due.serialController.SendSerialMessage("X");    // Serial message 'X' will deactivate relay
    }

    void Start()
    {
        // Disengage player movement upon initial startup
        goForward = false;

        // Delete any previous position file
        if (System.IO.File.Exists(filename))
        {
            File.Delete(filename);
        }

        ListofPlayerDistance = new List<(float, float)>();          // Set List data structure to hold player distances 
        leftRewardTileState = Enumerable.Repeat(true, 13).ToList(); 
        rightRewardTileState = Enumerable.Repeat(true, 13).ToList();

        // Start session
        Button start_button = startButton.GetComponent<Button>();
        start_button.onClick.AddListener(delegate { sessionHandler(0); });
        // Conclude session
        Button conclude_button = concludeButton.GetComponent<Button>();
        concludeButton.onClick.AddListener(delegate { sessionHandler(1); });
        // Test run (do not collect data)
        Button test_running_button = testRunningButton.GetComponent<Button>();
        test_running_button.onClick.AddListener(delegate { sessionHandler(2); });
        // Save directory button
        Button choose_saveDir_button = chooseSaveDirectoryButton.GetComponent<Button>();
        choose_saveDir_button.onClick.AddListener(delegate { sessionHandler(3); });
        // Exit program
        Button exit_button = exitButton.GetComponent<Button>();
        exit_button.onClick.AddListener(delegate { sessionHandler(4); });
        
    }

    // Handles different session parameters absed on buttons
    void sessionHandler(int state)
    {
        switch (state)
        {
            // Start button
            case 0:
                
                // If the TCP Client failed to connect to Intan, do not start session.
                if (TCPClient.globalFailConnect)
                {
                    return;
                }

                // If a directory for saving position data was not chosen, highlight save directory button
                // to signal user, and return.
                if (filename.Length == 0)
                {
                    chooseSaveDirectoryButton.GetComponentInChildren<Image>().color = Color.yellow;
                    return;
                }

                //// Makeshift delay timer so the subsequent intan error can be caught (in case it occurs)
                //startTimer = Time.time;
                //while (Time.time - startTimer < 1)
                //{
                //}
                //startTimer = -1;

                //// Intan error: This error occurs in TCP script. 
                //// If the basefile was not set, do not start the session.
                //if (TCPClient.intanError)
                //{
                //    return;
                //}

                //else // Else reset the intan error (to remove error message.) 
                //{
                //    TCPClient.intanError = false;
                //}
                
                cyclicPlayerDistance = 0;                   // Set in-game player distance to zero
                arduino_due.resetEncoderDistance = true;    // Reset the arduino's distance count to zero as well
                goForward = true;                           // Enable player movement
                arduino_due.collectData = true;             // Enable arduino recording of optical encoder 
                recordData = true;                          // Allow Unity to record player distance for saving
                elapsedTime = Time.time;                    // Grab reference of start time.

                // Deactivate other maze options
                InstantiateMaze.placeObjects.gameObject.SetActive(false);
                InstantiateMaze.rewardButton.gameObject.SetActive(false);
                break;

            // Conclude button
            case 1:
                
                // If we aren't recording data, don't do anything
                if (!recordData)
                {
                    return;
                }

                goForward = false;                       // Disable player movement
                cyclicPlayerDistance = 0;                // Set player distance to zero
                arduino_due.resetEncoderDistance = true; // Reset Ardunos reference to encoder distance to zero
                arduino_due.collectData = false;         // Stop arduino from recording data
                recordData = false;                      // Stop unity from collecting data for saving
                transform.position = pathCreator.path.GetPointAtDistance(0); // Teleport the player back to the start
                WritePositionData();                                         // Write the position data to a text file

                // Reactivate the main UI buttons
                InstantiateMaze.placeObjects.gameObject.SetActive(true);
                InstantiateMaze.rewardButton.gameObject.SetActive(true);
                break;

            // Test run button. Can be used to check if subject movementis translating
            // to in-game movement without recording data.
            case 2:

                recordData = false;

                // If we just clicked test run
                if (testRunningButton.GetComponentInChildren<Text>().text == "Test running (no data collected)")
                {
                    // Deactivate alternate mazes
                    InstantiateMaze.placeObjects.gameObject.SetActive(false);
                    InstantiateMaze.rewardButton.gameObject.SetActive(false);
                    
                    // Allow arduino to measure optical encoder revolutions
                    arduino_due.collectData = true;
                    // Enable player movement
                    goForward = true;
                    // Set button text to 'stop run'
                    testRunningButton.GetComponentInChildren<Text>().text = "Stop test";
                }
                else
                {
                    // If the user clicked stop test
                    // Re-activate the main UI buttons
                    InstantiateMaze.placeObjects.gameObject.SetActive(true);
                    InstantiateMaze.rewardButton.gameObject.SetActive(true);
                    goForward = false;               // Disable player movement
                    arduino_due.collectData = false; // Stop arduino data collection
                    cyclicPlayerDistance = 0;        // Set player distance to zero
                    arduino_due.resetEncoderDistance = true; // Set Arduino measurement of optical encoder to zero
                    transform.position = pathCreator.path.GetPointAtDistance(0); // Teleport player to maze start
                    testRunningButton.GetComponentInChildren<Text>().text = "Test running (no data collected)";
                }

                break;

            // Choose directory button
            case 3:
                // Open fie dialog box to chose save folder
                var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Application.dataPath, true);
                WriteResult(paths);
                // Reset save directory button color in case it was previously highlighted yellow
                chooseSaveDirectoryButton.GetComponentInChildren<Image>().color = Color.white;
                break;

            // Exit button
            case 4:
                Debug.Log("Quitting");
                Application.Quit();
                break;
        }
    }
    void FixedUpdate()
    {

        if (goForward)
        {
            // If the raw arduino input is increasing, increment the player distance
            // This extra variable is necessary because the arduino input is monotonically increasing
            // and is not paused (meaning if the subject continues walking when the goForward flag is set to false)
            // the arduno will still send distances that are increasing. This can cause the mice to 'teleport' to a non-start
            // position after the plate flash is complete and goForward is set to true.
            // This variable ensures we can easily control where the in-game player is meant to be 
            
            // If we have moved since the last frame
            if (distanceTravelled - lastDistance > 0)
            {
                // Increase the cyclic distance by the difference and scale it down by 1/5th. 
                // The scale down is needed to keep player movement at a rasonable speed.
                cyclicPlayerDistance += (distanceTravelled - lastDistance) * 0.8f;
            }

            // When player distance increases, move player along pre-defined path
            transform.position = pathCreator.path.GetPointAtDistance(cyclicPlayerDistance);
            lastDistance = distanceTravelled;

            // If we are in reward mode
            if (rewardSet)
            {
                // Check each tile for a reward flag
                foreach (int tileIndex in leftRewardTiles)
                {
                    // If we find a reward tile and the player is within a specific distance of the tile
                    if (Vector3.Distance(transform.position, Maze.transform.GetChild(1).GetChild(0).GetChild(tileIndex).gameObject.transform.position) < 8.5 && leftRewardTileState[tileIndex])
                    {
                        // Start a timer if one hasn't been started already. Default startTimer value is -1.
                        if (startTimer < 0)
                        {
                            startTimer = Time.time;
                            Debug.Log(startTimer);
                        }

                        else
                        {
                            // If timer passes 2 seconds, offer reward
                            if (Time.time - startTimer > 2)
                            {
                                Debug.Log("REWARD!");
                                StartCoroutine(relayControl()); // Start the co-routine for reward stimulus
                                startTimer = -1;                // Reset timer
                                // Mark the tile as non-reward (i.e it has been visited)
                                leftRewardTileState[tileIndex] = false;
                            }
                        }
                    }

                    else
                    {
                        startTimer = -1;
                    }
                }

                // Same routine as above for right reward tiles. 
                foreach (int tileIndex in rightRewardTiles)
                {
                    if (Vector3.Distance(transform.position, Maze.transform.GetChild(2).GetChild(1).GetChild(tileIndex).gameObject.transform.position) < 8.5 && rightRewardTileState[tileIndex])
                    {

                        if (startTimer == 0)
                        {
                            startTimer = Time.time;
                        }

                        else
                        {
                            if (Time.time - startTimer > 2)
                            {
                                Debug.Log("REWARD!");
                                StartCoroutine(relayControl());
                                startTimer = -1;
                                rightRewardTileState[tileIndex] = false;
                            }
                        }
                    }

                    else
                    {
                        startTimer = 0;
                    }
                }
            }
        }

        // If we are within 5 units of distance from the maze endPlate and plate is yet to flash
        if (Vector3.Distance(transform.position, pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1)) < 5 && !plateFlash)
        {

            goForward = false;               // Stop movement
            plateFlash = true;               // Set plate flashing flag to true
            StartCoroutine(FlashEndPlate()); // Start plate flashing co-routine 

            // $ $ $ 
            if (InstantiateMaze.curr_context == 1)
            {
                InstantiateMaze.curr_context = 0;
            }
            else
            {
                InstantiateMaze.curr_context = 1;
            }
            
            // Reset the boolean lists that mark tile 'reward' states before the next lap starts. 
            leftRewardTileState = Enumerable.Repeat(true, 13).ToList();
            rightRewardTileState = Enumerable.Repeat(true, 13).ToList();

            
            // Else if we have passed the plate and have teleported to the start (i.e distance is above 60 units)
        } else if (Vector3.Distance(transform.position, pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1)) > 60)
        {
            // Set plate flashing to false
            plateFlash = false;

        }

        // Continually record positon data and reward data to list
        if (recordData)
        {
            ListofPlayerDistance.Add((Time.time - elapsedTime, cyclicPlayerDistance));
        }

    }

    // Called when user concludes experiment
    void WritePositionData()
    {
        string textToWrite = "POSITION DATA" + "\r\n";
        // Writing position data to text file
        Debug.Log("Writing positiion data to file... please wait.");
        foreach ((float, float) data in ListofPlayerDistance)
        {
            textToWrite = textToWrite + data.Item1 + ";" + data.Item2 + "\r\n";
        }
        File.AppendAllText(filename, textToWrite);
        Debug.Log("Writing complete!");
    }

    public void WriteResult(string[] paths)
    {
        if (paths.Length == 0)
        {
            return;
        }

        _path = "";
        foreach (var p in paths)
        {
            _path += p;
        }

        filename = _path + @"\position.txt";
        Debug.Log(filename);
    }
}
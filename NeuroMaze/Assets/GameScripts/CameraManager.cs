using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    /// <summary>
        /// Central class for camera view control of player, mazes, 
        /// and alternate views from UI options
    /// </summary>
    
    // Set player, object selection ui, and reward selection ui cameras respectively
    public Camera playerCamera, objectHallCamera, rewardCamera;
    // Set buttons to access 'set object' and 'set reward' options, and back buttons.
    public Button placeObjects, rewardButton, backButton_1, backButton_2;
    // Create 3 UI canvases. 
    public Canvas mainCanvas, secondCanvas, thirdCanvas;

    // Start is called before the first frame update
    void Start()
    {
        Button btn_placeObj = placeObjects.GetComponent<Button>();  // Direct user to set objects menu
        Button btn_back_1 = backButton_1.GetComponent<Button>();    // Takes user back to main menu
        Button btn_back_2 = backButton_2.GetComponent<Button>();    
        Button btn_reward = rewardButton.GetComponent<Button>();    // Directs user to set reward menu
        
        // Add listeners to all buttons
        btn_placeObj.onClick.AddListener(placeObjectClick);
        btn_reward.onClick.AddListener(rewardClick);
        btn_back_1.onClick.AddListener(backClick);
        btn_back_2.onClick.AddListener(backClick);

        rewardCamera.enabled = false;
        objectHallCamera.enabled = false;

    }

    // Function to set cameras when reward button clicked
    void rewardClick()
    {
        // Only enable the rewardCamera
        rewardCamera.enabled = true;        
        playerCamera.enabled = false;       
        objectHallCamera.enabled = false; 

        // Only activate the third canvas to view the reward UI
        mainCanvas.gameObject.SetActive(false);
        secondCanvas.gameObject.SetActive(false);
        thirdCanvas.gameObject.SetActive(true);
    }

    // Function to mnage camera and object states when navigating back to main UI
    void backClick()
    {
        // only enable 1st person player camera
        playerCamera.enabled = true;
        objectHallCamera.enabled = false;
        rewardCamera.enabled = false;

        // Only enable main UI
        mainCanvas.gameObject.SetActive(true);
        secondCanvas.gameObject.SetActive(false);
        thirdCanvas.gameObject.SetActive(false);
    }

    // Camera management for when 'set object' is chosen
    void placeObjectClick()
    {
        // Only enable object hall camera
        playerCamera.enabled = false;
        objectHallCamera.enabled = true;

        // Only enable second canvas UI
        mainCanvas.gameObject.SetActive(false);
        secondCanvas.gameObject.SetActive(true);
        thirdCanvas.gameObject.SetActive(false);
    }
}

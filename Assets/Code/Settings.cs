using System;                                                                       //To be able to catch errors and using StringSplitOptions and do math
using System.Collections;                                                           //To use IEnumerator (co rotine)
using UnityEngine;
using UnityEngine.SceneManagement;                                                  //To be able to switch scenes
using UnityEngine.UI;                                                               //Required when Using UI elements.

/*



Screen.orientation = ScreenOrientation.?



//*/

public class Settings : MonoBehaviour
{
    //The tings to get the information from and write the info to on load
    public Scrollbar ScrollbarPanel;
    public Slider AnimationTime;
    public Slider StepsBuffer;
    public Slider MaxFrameRate;
    public Toggle MuteMusic;
    public Toggle MuteSounds;
    public Toggle HideStatusBar;
    public Toggle LockDevice;
    public Toggle InverseZoom;
    public Toggle InverseMove;
    public Toggle DontResetZoom;
    public Toggle FPSCounter;
    public GameObject LoadAnimationBlackScreen;									    //to select the black screen
    public GameObject LoadAnimationBar;                                             //To enable and rotate the rotatingbar
    private float AnimationCurrentTime = 0;                                         //Where the animation is at
    public float LoadingTime = 0.3f;                                                //The time to show the loading animation

    private float InputVertical;                                                    //If a vertical input is registred (keyboard)
    private float InputHorizontal;                                                  //If a Horizontal input is registred (keyboard)
    private bool Vertical = false;          										//If a Vertical key is pressed and registered
    private bool Horizontal = false;                                                //If a horizontal key is pressed and registered
    private Vector2 touchOrigin = -Vector2.one;                                     //To store location of screen touch origin for touch controls.
    private string MovesBuffer = "";        										//list of next moves
    private bool Konami = false;

    public void Start()
    {
        StartCoroutine(LoadingAnimationIn());                                       //Show start animation
        LoadSettings();
        if (PlayerPrefs.GetInt("AnimationSpeed", -1) == -1)
        {
            Button_BackToMenu();
        }
        ScrollbarPanel.value = 1;
    }
    private void Update()
    {
        U1GetInputs();
    }
    void U1GetInputs()                                                              //Get input and convert these to a string
    {
        if (Input.GetKeyDown(KeyCode.Escape))                                       //If player pressed back of ESC
        {
            Button_BackToMenu();                                                    //Go back to menu
        }
        InputVertical = Input.GetAxisRaw("Vertical");                               //Set input controls to keys
        InputHorizontal = Input.GetAxisRaw("Horizontal");                           //Set input controls to keys
        if (Input.touchCount == 1)                                                  //Check if Input has registered a touch
        {
            Touch myTouch = Input.touches[0];                                       //Store the first touch detected.
            if (myTouch.phase == TouchPhase.Began)                                  //Check if the phase of that touch equals Began
            {
                touchOrigin = myTouch.position;                                     //If so, set touchOrigin to the position of that touch
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)       //If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
            {
                Vector2 touchEnd = myTouch.position;                                //Set touchEnd to equal the position of this touch
                float x = touchEnd.x - touchOrigin.x;                               //Calculate the difference between the beginning and end of the touch on the x axis.
                float y = touchEnd.y - touchOrigin.y;                               //Calculate the difference between the beginning and end of the touch on the y axis.
                touchOrigin.x = -1;                                                 //Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
                if (Mathf.Abs(x) > Mathf.Abs(y))                                    //Check if the difference along the x axis is greater than the difference along the y axis.
                    InputHorizontal = x;                                            //x is set horizontal
                else
                    InputVertical = y;                                              //y is set horizontal
            }
        }
        if (InputVertical != 0 || InputHorizontal != 0 || Vertical == true || Horizontal == true)
        {
            if (Vertical == true)		                                            //If forward and backwards are not pressed & are triggered before
            {
                if (InputVertical == 0)
                {
                    Vertical = false;											    //Reset so forward and backwards can be triggered again
                }
            }
            else if (InputVertical < 0)                                             //if button forward is pressed & not handled before
            {
                Vertical = true;													//Set this press is being handled already
                MovesBuffer = MovesBuffer + "A";								    //Handle the press
            }
            else if (InputVertical > 0)		                                        //If button backwards is pressed & not handled before
            {
                Vertical = true;													//Set this press is being handled already
                MovesBuffer = MovesBuffer + "B";									//Handle the press
            }
            if (Horizontal == true)		                                            //If left and right are not pressed & are triggered before
            {
                if (InputHorizontal == 0)
                {
                    Horizontal = false;											    //Reset so left and right can be triggered again
                }
            }
            else if (InputHorizontal < 0)			                                //if button left is pressed & not handled before
            {
                Horizontal = true;													//Set this press is being handled already
                MovesBuffer = MovesBuffer + "C";								    //Handle the press
            }
            else if (InputHorizontal > 0)		                                    //If button right is pressed & not handled before
            {
                Horizontal = true;												    //Set this press is being handled already
                MovesBuffer = MovesBuffer + "D";								    //Handle the press
            }
            if (MovesBuffer.Length > 7)
            {
                if (MovesBuffer.Substring(MovesBuffer.Length - 8, 8) == "BBAACDCD")
                {
                    Konami = true;
                }
            }
        }
    }
    public void DeleteSettings()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.DeleteKey("AnimationSpeed");
        PlayerPrefs.DeleteKey("StepsBuffer");
        PlayerPrefs.DeleteKey("MaxFrameRate");
        PlayerPrefs.DeleteKey("MuteMusic");
        PlayerPrefs.DeleteKey("MuteSounds");
        PlayerPrefs.DeleteKey("LockDevice");
        PlayerPrefs.DeleteKey("HideStatusBar");
        PlayerPrefs.DeleteKey("InverseZoom");
        PlayerPrefs.DeleteKey("InverseMove");
        PlayerPrefs.DeleteKey("DontResetZoom");
        PlayerPrefs.DeleteKey("FPSCounter");
        PlayerPrefs.DeleteKey("Konami");
    }
    public void SaveSettings()
    {
        DeleteSettings();
        PlayerPrefs.SetInt("AnimationSpeed", System.Convert.ToInt16(AnimationTime.value));
        PlayerPrefs.SetInt("StepsBuffer", System.Convert.ToInt16(StepsBuffer.value));
        PlayerPrefs.SetInt("MaxFrameRate", System.Convert.ToInt16(MaxFrameRate.value));
        Application.targetFrameRate = PlayerPrefs.GetInt("MaxFrameRate");           //Set MaxFrame rate
        Screen.fullScreen = false;

        if (MuteMusic.isOn)
        {
            PlayerPrefs.SetInt("MuteMusic", 1);
        }
        else
        {
            PlayerPrefs.SetInt("MuteMusic", 0);
        }
        if (MuteSounds.isOn)
        {
            PlayerPrefs.SetInt("MuteSounds", 1);
        }
        else
        {
            PlayerPrefs.SetInt("MuteSounds", 0);
        }
        if (HideStatusBar.isOn)
        {
            Screen.fullScreen = true;
            PlayerPrefs.SetInt("HideStatusBar", 1);
        }
        else
        {
            Screen.fullScreen = false;
            PlayerPrefs.SetInt("HideStatusBar", 0);
        }
        if (LockDevice.isOn)
        {
            PlayerPrefs.SetInt("LockDevice", 1);
        }
        else
        {
            PlayerPrefs.SetInt("LockDevice", 0);
        }
        if (InverseZoom.isOn)
        {
            PlayerPrefs.SetInt("InverseZoom", 1);
        }
        else
        {
            PlayerPrefs.SetInt("InverseZoom", 0);
        }
        if (InverseMove.isOn)
        {
            PlayerPrefs.SetInt("InverseMove", 1);
        }
        else
        {
            PlayerPrefs.SetInt("InverseMove", 0);
        }
        if (DontResetZoom.isOn)
        {
            PlayerPrefs.SetInt("DontResetZoom", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DontResetZoom", 0);
        }
        if (FPSCounter.isOn)
        {
            PlayerPrefs.SetInt("FPSCounter", 1);
        }
        else
        {
            PlayerPrefs.SetInt("FPSCounter", 0);
        }
        if (Konami == true)
        {
            PlayerPrefs.SetInt("Konami", 1);
        } else
        {
            PlayerPrefs.SetInt("Konami", 0);
        }
    }
    public void LoadSettings()
    {
        //Fetch the value from the PlayerPrefs. If this name doesnt exists, then return the default 
        AnimationTime.value = PlayerPrefs.GetInt("AnimationSpeed", 250);
        StepsBuffer.value = PlayerPrefs.GetInt("StepsBuffer", 3);
        MaxFrameRate.value = PlayerPrefs.GetInt("MaxFrameRate", 60);
        if (PlayerPrefs.GetInt("MuteMusic", 0) == 0) 
        {
            MuteMusic.isOn = false;
        }
        else
        {
            MuteMusic.isOn = true;
        }
        if (PlayerPrefs.GetInt("MuteSounds", 0) == 0)
        {
            MuteSounds.isOn = false;
        }
        else
        {
            MuteSounds.isOn = true;
        }
        if (Screen.fullScreen == false)
        {
            HideStatusBar.isOn = false;
            PlayerPrefs.SetInt("HideStatusBar", 0);
            //JELLE TODO FIXME
        }
        else
        {
            HideStatusBar.isOn = true;
            PlayerPrefs.SetInt("HideStatusBar", 1);
            //JELLE TODO FIXME
        }
        if (PlayerPrefs.GetInt("LockDevice", 0) == 0)
        {
            LockDevice.isOn = false;
        }
        else
        {
            LockDevice.isOn = true;
        }
        if (PlayerPrefs.GetInt("InverseZoom", 0) == 0)
        {
            InverseZoom.isOn = false;
        }
        else
        {
            InverseZoom.isOn = true;
        }
        if (PlayerPrefs.GetInt("InverseMove", 0) == 0)
        {
            InverseMove.isOn = false;
        }
        else
        {
            InverseMove.isOn = true;
        }
        if (PlayerPrefs.GetInt("DontResetZoom", 0) == 0)
        {
            DontResetZoom.isOn = false;
        }
        else
        {
            DontResetZoom.isOn = true;
        }
        if (PlayerPrefs.GetInt("FPSCounter", 0) == 0)
        {
            FPSCounter.isOn = false;
        }
        else
        {
            FPSCounter.isOn = true;
        }
        if ((PlayerPrefs.GetInt("Konami", 0) == 1))
        {
            Konami = true;
        }
        else
        {
            Konami = false;
        }
    }
    IEnumerator LoadingAnimationIn()
    {
        int Size = System.Convert.ToInt32(Math.Round((Math.Sqrt(Math.Pow((Screen.width + Screen.height) / 2, 2) * 2)), 0));    //Calculate size of the loading screen to cover the complete screen
        var rectTransform = LoadAnimationBlackScreen.GetComponent<RectTransform>(); //Select it so we can change it size
        rectTransform.sizeDelta = new Vector2(Size, Size);                          //Set the size
        Vector2 from = new Vector2((-Screen.width + -Screen.height) / 2 - 10, Screen.height / 2);  //Set the 'from' position
        Vector2 to = new Vector2(Screen.width / 2, Screen.height / 2);              //End position of LoadAnimationBlackScreen

        LoadAnimationBar.SetActive(false);
        bool ShowAnimation = true;                                                  //Start to show animation
        while (ShowAnimation == true)                                               //While animation isn't done
        {
            LoadAnimationBlackScreen.transform.position = Vector2.Lerp(to, from, AnimationCurrentTime / LoadingTime);   //move the screen to the percentage
            AnimationCurrentTime += Time.deltaTime;                                 //Move counter forward
            if (AnimationCurrentTime > LoadingTime)                                 //If at end of animation
            {
                LoadAnimationBlackScreen.transform.position = from;                 //make sure player is at the end position
                if (AnimationCurrentTime > (LoadingTime + 0.1f))                    //If at end of animation
                {
                    AnimationCurrentTime = 0;                                       //Reset
                    ShowAnimation = false;                                          //Reset
                }
            }
            yield return null;
        }
        LoadAnimationBar.SetActive(false);
        LoadAnimationBlackScreen.SetActive(false);
    }
    IEnumerator LoadingAnimationOutAndLoadLevel(string SceneToLoad)
    {
        int Size = System.Convert.ToInt32(Math.Round((Math.Sqrt(Math.Pow((Screen.width + Screen.height) / 2, 2) * 2)), 0));    //Calculate size of the loading screen to cover the complete screen
        var rectTransform = LoadAnimationBlackScreen.GetComponent<RectTransform>(); //Select it so we can change it size
        rectTransform.sizeDelta = new Vector2(Size, Size);                          //Set the size
        Vector2 from = new Vector2((-Screen.width + -Screen.height) / 2 - 10, Screen.height / 2);  //Set the 'from' position
        Vector2 to = new Vector2(Screen.width / 2, Screen.height / 2);              //End position of LoadAnimationBlackScreen
        LoadAnimationBlackScreen.SetActive(true);
        LoadAnimationBar.SetActive(true);
        bool ShowAnimation = true;                                                  //Start to show animation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneToLoad);
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f || ShowAnimation == true)
        {
            LoadAnimationBlackScreen.transform.position = Vector2.Lerp(from, to, AnimationCurrentTime / LoadingTime);   //move the screen to the percentage
            AnimationCurrentTime += Time.deltaTime;                                 //Move counter forward
            LoadAnimationBar.transform.Rotate(new Vector3(0, 0, 400) * asyncLoad.progress);
            if (AnimationCurrentTime > LoadingTime)                                 //If at end of animation
            {
                LoadAnimationBlackScreen.transform.position = to;                   //make sure player is at the end position
                if (AnimationCurrentTime > (LoadingTime + 0.1f))                    //If at end of animation
                {
                    AnimationCurrentTime = 0;                                       //Reset
                    ShowAnimation = false;                                          //Reset
                }
            }
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;                                      //Go to the screen
    }
    public void Button_Reset()
    {
        DeleteSettings();
        LoadSettings();
    }
    public void Button_BackToMenu()
    {
        SaveSettings();
        StartCoroutine(LoadingAnimationOutAndLoadLevel("MainMenu"));                //Start loading screen in background, and go to it when done
    }
}

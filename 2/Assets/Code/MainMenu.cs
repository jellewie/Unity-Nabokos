using System;                                                                       //To be able to catch errors and using StringSplitOptions and do math
using System.Collections;                                                           //To use IEnumerator (co rotine)
using UnityEngine;
using UnityEngine.SceneManagement;                                                  //To be able to switch scenes

public class MainMenu : MonoBehaviour
{
    private string SceneToLoad;
    public GameObject LoadAnimationBlackScreen;									    //to select the black screen
    public GameObject LoadAnimationBar;                                             //To enable and rotate the rotatingbar
    private float AnimationCurrentTime = 0;                                         //Where the animation is at
    public float LoadingTime = 0.3f;                                                //The time to show the loading animation

    void Start()
    {
        StartCoroutine(LoadingAnimationIn());                                       //Show start animation
        //Debug.Log(("Size of LoadAnimationBlackScreen for 1280*720 is " +  Mathf.Sqrt(((1280 / 2) * (1280 / 2)) + ((1280 / 2) * (1280 / 2)))) + (Mathf.Sqrt(((720 / 2) * (720 / 2)) + ((720 / 2) * (720 / 2)))));
        //Debug.Log(("Offset of LoadAnimationBlackScreen for 1280*720 is "1280+ 720/2);
        if (PlayerPrefs.GetInt("AnimationSpeed", -1) == -1)
        {
            Debug.Log("First time setup there are no settings");
            LoadSceneSettings();
        }
        if (PlayerPrefs.GetInt("LockDevice", 0) == 0)
        {
            Screen.fullScreen = false;
        } 
        else
        {
            Screen.fullScreen = true;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    public void LoadScenePlayMode()
    {
        StartCoroutine(LoadingAnimationOutAndLoadLevel("PlayMode"));                //Start loading screen in background, and go to it when done
    }
    public void LoadSceneSettings()
    {
        StartCoroutine(LoadingAnimationOutAndLoadLevel("Settings"));                //Start loading screen in background, and go to it when done
    }
    public void LoadSceneEditMode()
    {
        StartCoroutine(LoadingAnimationOutAndLoadLevel("EditMode"));                //Start loading screen in background, and go to it when done
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
    public void OpenUrlYoutube()
    {
        Application.OpenURL("https://www.youtube.com/jellewho?sub_confirmation=1");
    }
    public void OpenUrlTwitter()
    {
        Application.OpenURL("https://twitter.com/jellewie");
    }
}
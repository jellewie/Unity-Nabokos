using UnityEngine;

public class Bouncy : MonoBehaviour {
    float a;
    bool Toggle;                                                                    //If the text need to zoom in or out
    float StepSize;                                                                 //The size of each step to take (60 steps per second or so)
    GameObject Object;                                                              //The object this code is attached to
    public float AnimationTime = 2;                                                 //Time in seconds for the anmiation to complete a loop
    public float Offset = 0.2f;                                                     //Bounce distance
    private void Start()
    {
        AnimationTime *= 60;                                                        //Change animationtime from seconds to frames
        StepSize = Offset / AnimationTime /2;                                       //Calc stepsize
        Object = gameObject;                                                        //Get command that this code is attacked to
    }
    void Update()
    {
        if (Object.activeSelf)                                                      //If the object is active
        {
            if (Toggle)                                                             //If going up
            {
                if (a >= AnimationTime)                                             //If we bounce out all the way
                {
                    Toggle = false;                                                 //Toggle so we are going in again
                }
                a += 1;                                                             //Add 1 (we are expanding)
            }
            else                                                                    //Else we are going down
            {
                if (a <= 0)                                                         //If we bounce in all the way
                {
                    Toggle = true;                                                  //Toggle so we are going out again
                }
                a -= 1;                                                             //Remove 1 (we are shrinking)
            }
            float x = (a * StepSize) + 1 - Offset;                                  //Calc the scale size
            Object.transform.localScale = new Vector3(x, x, x);                     //Execute scale
        }
    }
}

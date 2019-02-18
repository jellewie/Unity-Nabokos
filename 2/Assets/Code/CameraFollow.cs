using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public GameObject Player;                                                       //Object to folow
    private Vector3 Offset;                                                         //Offset between player and camera
    void Start()
    {
        Offset = transform.position - Player.transform.position;                    //Set the offset
    }
    private void LateUpdate()
    {
        transform.position = Player.transform.position + Offset;                    //Set camera to the player + offset
    }
}
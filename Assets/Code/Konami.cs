using UnityEngine;

public class Konami : MonoBehaviour
{
    public GameObject KonamiFolder;

    void Start()
    {
        if (PlayerPrefs.GetInt("Konami", 0) == 1)
        {
            KonamiFolder.SetActive(true);
        } else
        {
            KonamiFolder.SetActive(false);
        }
    }
}

using UnityEngine;

public class StartupManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;

        int targetWidth = Mathf.RoundToInt(screenWidth * 0.5f);
        int targetHeight = Mathf.RoundToInt(screenHeight * 0.5f);

        Screen.SetResolution(targetWidth, targetHeight, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{

    public GameObject mainMenuObject;
    public GameObject settingsObject;
    
    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;

    [Header("Settings Menu UI Elements")]
    public Slider viewDistSlider;
    public TextMeshProUGUI viewDistText;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityText;
    public Toggle threadingToggle;

    Settings settings;

    private void Awake()
    {
        if(!File.Exists(Application.dataPath + "/Data/Settings/settings.cfg"))
        {
            Debug.Log("No Settings file found, creating new one");

            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/Data/Settings/settings.cfg", jsonExport);
        }
        else
        {
            Debug.Log("Settings file found, loading settings");

            string jsonImport = File.ReadAllText(Application.dataPath + "/Data/Settings/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }
   public void StartGame()
   {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.worldSizeInChunks;
        SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
   }

   public void EnterSettings()
   {
        viewDistSlider.value = settings.viewDistance;
        UpdateViewDistSlider();
        mouseSensitivitySlider.value = settings.mouseSensitivity;
        UpdateMouseSensitivitySlider();
        threadingToggle.isOn = settings.enableThreading;


        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
   }

   public void LeaveSettings()
   {
        settings.viewDistance = (int)viewDistSlider.value;
        settings.mouseSensitivity = mouseSensitivitySlider.value;
        settings.enableThreading = threadingToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/Data/Settings/settings.cfg", jsonExport);

        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
   }

   public void QuitGame()
   {
        Application.Quit();
   }

   public void UpdateViewDistSlider()
   {
        viewDistText.text = "View Distance: " + viewDistSlider.value;
   }

   public void UpdateMouseSensitivitySlider()
   {
        mouseSensitivityText.text = "Mouse Sensitivity: " + mouseSensitivitySlider.value.ToString("F1");
   }
}

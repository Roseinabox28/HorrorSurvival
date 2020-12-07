using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
public class EscapeMenu : MonoBehaviour
{

    public GameObject menuBG;
    public GameObject escapeMenuObject;
    public GameObject settingsObject;
    [Header("Escape Menu UI Elements")]
    public TMP_InputField worldNameField;

    [Header("Settings Menu UI Elements")]
    public Slider viewDistSlider;
    public TextMeshProUGUI viewDistText;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityText;
    public Toggle threadingToggle;

    public bool gameSaved = false;

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
    
    public void OpenMenu()
    {
        gameSaved = false;
        Debug.Log("World name: " + World.instance.worldData.worldName);
        worldNameField.text = World.instance.worldData.worldName;

        menuBG.SetActive(true);
        escapeMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }

    public void CloseMenu()
    {
        World.instance.worldData.worldName = worldNameField.text;
        settings.viewDistance = (int)viewDistSlider.value;
        settings.mouseSensitivity = mouseSensitivitySlider.value;
        settings.enableThreading = threadingToggle.isOn;

        menuBG.SetActive(false);
        escapeMenuObject.SetActive(false);
        settingsObject.SetActive(false);
    }

    public void SaveAndQuit()
    {
        StartCoroutine("SaveAndQuitCor");
    }

    IEnumerator SaveAndQuitCor()
    {
        if(SaveSystem.isSaving)
        {
            Debug.Log("Game is in the process of saving, waiting for it to finish");
            yield return null;
        }
        else if(!gameSaved)
        {
            SaveGame();
            Debug.Log("Game hasn't been saved from here");
            
        }
        else
        {
            Debug.Log("Game has been saved, closing game");
            Application.Quit();
        }
        yield return null;
    }
    

    public void SaveGame()
    {
        World.instance.worldData.worldName = worldNameField.text;
        SaveSystem.SaveWorld(World.instance.worldData);
        gameSaved = true;
    }

    public void EnterSettings()
   {
        viewDistSlider.value = settings.viewDistance;
        UpdateViewDistSlider();
        mouseSensitivitySlider.value = settings.mouseSensitivity;
        UpdateMouseSensitivitySlider();
        threadingToggle.isOn = settings.enableThreading;


        escapeMenuObject.SetActive(false);
        settingsObject.SetActive(true);
   }

   public void LeaveSettings()
   {
        settings.viewDistance = (int)viewDistSlider.value;
        settings.mouseSensitivity = mouseSensitivitySlider.value;
        settings.enableThreading = threadingToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/Data/Settings/settings.cfg", jsonExport);

        escapeMenuObject.SetActive(true);
        settingsObject.SetActive(false);
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

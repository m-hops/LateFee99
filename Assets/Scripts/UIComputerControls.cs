using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIComputerControls : MonoBehaviour
{
    
    public Toggle ToggleBad;
    public Toggle ToggleGood;
    public Toggle ToggleUnknown;
    public TMPro.TMP_Dropdown TitleDropdown;

    public void Save()
    {
        var gameVariableContainer = FindObjectOfType<GameVariableContainer>();
        if(gameVariableContainer != null)
        {
            string quality = "Unset";
            if (ToggleBad.isOn) quality = "Bad";
            if (ToggleGood.isOn) quality = "Good";
            if (ToggleUnknown.isOn) quality = "Unknown";
            gameVariableContainer.SetValue($"Quality[{TitleDropdown.value}]", quality);
            Debug.Log($"Computer saved: Quality[{TitleDropdown.value}] = \"{quality}\"");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

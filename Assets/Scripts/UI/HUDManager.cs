using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager active;
    [SerializeField] GameObject interactionPrompt;
    [SerializeField] GameObject[] holdActionPrompts;

    void Awake()
    {
        active = this;
        HoldingPrompt(new List<ButtonPrompt>());
    }

    public void InteractionPrompt(bool visible, string command)
    {
        interactionPrompt.SetActive(visible);
        TMP_Text text = interactionPrompt.GetComponentInChildren<TMP_Text>();
        text.text = command;
    }
    public void HoldingPrompt(List<ButtonPrompt> buttons)
    {
        for (int i = 0; i < holdActionPrompts.Length; i++)
        {
            bool show = buttons.Contains((ButtonPrompt)i);
            holdActionPrompts[i].SetActive(show);
        }
    }
}
public enum ButtonPrompt
{ 
    Drop,
    ToInventory,
    toBelt,
    Use
}


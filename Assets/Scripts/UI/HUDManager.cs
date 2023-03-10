using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager active;
    [SerializeField] GameObject centerPoint;
    [SerializeField] GameObject arrow;
    [SerializeField] bool lockArrowPosition;
    Vector2 arrowPosition;
    [SerializeField] PromptObject[] promptObjects;

    void Awake()
    {
        active = this;
    }
    private void Start()
    {
        OrganizePOs();
        arrow.SetActive(false);
        arrowPosition = arrow.transform.position;
    }
    void OrganizePOs()
    {
        PromptObject[] temp = new PromptObject[promptObjects.Length];
        foreach (PromptObject po in promptObjects)
        {
            temp[(int)po._prompt] = po;
        }
        promptObjects = temp;
    }

    public void ShowCrosshair(bool show)
    {
        centerPoint.SetActive(show);
    }
    public void ShowArrow(bool show, Vector2 position, Vector2 direction, Color color)
    {
        arrow.SetActive(show);
        /*switch (lockArrowPosition)
        {
            case true:
                arrow.transform.position = arrowPosition;
                break;
            case false:
                arrow.transform.position = position;
                break;
        }*/
        float zRotation = Vector2.SignedAngle(Vector2.up, direction);
        arrow.transform.eulerAngles = new Vector3(0, 0, zRotation);
        Image image = arrow.GetComponent<Image>();
        image.color = color;
    }
    public void ShowPrompts(List<ButtonPrompt> prompts)
    {
        foreach (PromptObject po in promptObjects)
        {
            po._object.SetActive(prompts.Contains(po._prompt));
        }
    }
    public void ShowPromptCommand(ButtonPrompt button, string command)
    {
        GameObject go = promptObjects[(int)button]._object;
        go.SetActive(command != "");
        TMP_Text text = go.GetComponentInChildren<TMP_Text>();
        text.text = command;
    }
}
public enum ButtonPrompt
{ 
    //portables & devices
    Grab,
    Drop,
    toHerbarium,
    toBelt,
    Use,

    //book
    page,
    chapt,
    select
}

[System.Serializable]
public struct PromptObject
{
    public ButtonPrompt _prompt;
    public GameObject _object;

    public PromptObject(ButtonPrompt buttonPrompt, GameObject gameObject)
    {
        _prompt = buttonPrompt;
        _object = gameObject;
    }
}


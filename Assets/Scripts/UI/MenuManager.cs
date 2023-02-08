using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public Menu[] menus;
    List<Menu> openMenus = new List<Menu>();

    public int GetMenuIndex(string menuName)
    {
        for (int i = 0; i < menus.Length; i++) {
            switch (menus[i].Name == menuName) {
                case true:
                    return i;
            }
        }
        return -1;
    }
    public void OpenMenu(int index)
    {
        openMenus.Add(menus[index]);
        menus[index].GO.SetActive(true);
        CheckPause();
    }

    public void CloseCurrentMenu()
    {
        Menu current = openMenus[openMenus.Count - 1];
        openMenus.RemoveAt(openMenus.Count - 1);
        current.GO.SetActive(false);
    }

    void CheckPause()
    {
        if (openMenus.Count > 0)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
}
[System.Serializable]
public struct Menu
{
    public GameObject GO;
    public string Name;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] Menu[] menus;

    private void Awake()
    {
        Instance = this;
    }

    public void openMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].open();
            } 
            else if (menus[i].isopen)
            {
                closeMenu(menus[i]);
            }
        }
    }

    public void openMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].isopen)
            {
                closeMenu(menus[i]);
            }
        }
        menu.open();
    }

    public void closeMenu(Menu menu)
    {
        menu.close();
    }
}

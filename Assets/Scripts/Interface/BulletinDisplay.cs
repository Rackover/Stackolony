using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletinDisplay : MonoBehaviour {

    public class Bulletin
    {
        int ID = 0;
        int receptionCycle = 0;
    }

    public GameObject bulletinWindow;

    public void ToggleBulletinDisplay()
    {
        bulletinWindow.SetActive(!bulletinWindow.activeSelf);
    }

}

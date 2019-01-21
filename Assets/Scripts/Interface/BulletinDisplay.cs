using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletinDisplay : MonoBehaviour {

    public bool hasUnread = false;
    public GameObject notification;
    public GameObject bulletinWindow;

    public Text title;
    public Text subTitle;
    public Text text;

    BulletinsManager.Bulletin currentBulletin;

    public void RefreshBulletin()
    {
        if (
            currentBulletin == null ||
            GameManager.instance.bulletinsManager.GetBulletin().id != currentBulletin.id
        ) {
            currentBulletin = GameManager.instance.bulletinsManager.GetBulletin();
            Localization loc = GameManager.instance.localization;

            title.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinTitle").ToUpper();
            subTitle.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinSubtitle");
            text.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinText");

            SetUnread(!bulletinWindow.activeSelf);
        }
    }

    public void ToggleBulletinDisplay()
    {
        SetUnread(false);
        bulletinWindow.SetActive(!bulletinWindow.activeSelf);
    }

    public void SetUnread(bool unread)
    {
        hasUnread = unread;
        notification.SetActive(unread);
    }


    private void Update()
    {
        if (!GameManager.instance.IsInGame()) return;

        RefreshBulletin();
    }
}

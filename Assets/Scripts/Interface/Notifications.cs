using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifications : MonoBehaviour {

    List<Notification> notifications = new List<Notification>();
    Canvas canvas;

    public Color achievementsColor = Color.cyan;
    public float textBrightness = 1.2f;
    public float stripBrightness = 1.1f;
    public float tabDarkness = 0.2f;
    public float yOffset = 40;
    public float positionCatchupSpeed = 0.04f;
    public GameObject example;

    public class Notification
    {
        public Color mainColor;
        public Localization.Line locLine;
        public float duration = 5f;
        public bool isEternal = false;
        public GameObject gameObject;
        public Image[] images;
        public Text text;
        public Transform transform;

        public Notification(string localizationId, Color color, params string[] additionalValues)
        {
            mainColor = color;
            locLine = new Localization.Line("notification", localizationId, additionalValues);
        }

        public Notification(Localization.Line loc, Color color)
        {
            mainColor = color;
            locLine = loc;
        }
    }

    private void Start()
    {        
        canvas = GetComponentInParent<Canvas>();
        GameManager.instance.achievementManager.AchievementUnlocked += (id) => {
            Notify(
                new Notification(
                    new Localization.Line("notification", "achievementUnlocked", GameManager.instance.localization.GetLineFromCategory("achievementName", "achievement"+id)),
                    achievementsColor
                )
            );
        };
    }

    private void Update()
    {

        // Not updating notifications if the game is paused - everything should stay frozen
        if (GameManager.instance.IsPaused()) {
            return;
        }

        if (canvas == null) {
            canvas = GetComponentInParent<Canvas>();
        }

        // Notifications to keep will be put in a new list
        List<Notification> newList = new List<Notification>();

        int index = 0;
        float factor = canvas.scaleFactor;

        foreach (Notification not in notifications) {

            bool destroy = UpdateLifespan(not);
            
            if (!destroy) {
                newList.Add(not);
            }

            // Position catch up
            float yShould = -yOffset * index;
            CatchUpPosition(not, yShould);

            index++;
        }
        notifications = newList;
    }

    /// <summary>
    /// Creates a new notification
    /// </summary>
    /// <param name="notification"></param>
    public void Notify(Notification notification)
    {
        float factor = canvas.scaleFactor;

        GameObject nO = Instantiate(example, transform);
        nO.GetComponent<Image>().color = GetTabColor(notification.mainColor);
        nO.GetComponentsInChildren<Image>()[1].color = GetStripColor(notification.mainColor);

        Localization loc = GameManager.instance.localization;

        Text text = nO.GetComponentInChildren<Text>();
        text.color = GetTextColor(notification.mainColor);
        text.text = loc.GetLine(notification.locLine);
        notification.text = text;

        nO.transform.position = new Vector3(
              nO.transform.position.x,
              nO.transform.position.y - yOffset* factor*notifications.Count,
              nO.transform.position.z
         );
        notification.transform = nO.transform;

        notification.gameObject = nO;
        notification.images = nO.GetComponentsInChildren<Image>();

        GameManager.instance.soundManager.Play("Notification");

        notifications.Add(notification);
    }

    void CatchUpPosition(Notification not, float yPosition)
    {
        if (not.transform.localPosition.y != yPosition)
            not.transform.localPosition = new Vector3(
                  not.transform.localPosition.x,
                  Mathf.Lerp(not.transform.localPosition.y, yPosition, 0.05f),
                  not.transform.localPosition.z
         );
    }

    // Returns true if the notification has reached end of life
    bool UpdateLifespan(Notification not)
    {
        // Lifespan calculation
        if (!not.isEternal) {
            not.duration -= Time.deltaTime;
            if (not.duration <= 0f) {
                Destroy(not.gameObject);
                return true;
            }
            else {
                // Smooth fadeout when near destruction
                if (not.duration <= 1f) {
                    foreach (Image image in not.images) {
                        image.color = new Color(image.color.r, image.color.g, image.color.b, not.duration);
                    }
                    Text txt = not.text;
                    txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, not.duration);
                }
            }
        }
        return false;
    }

    Color GetStripColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.white, stripBrightness);
        newColor.a = 1f;
        return newColor;
    }

    Color GetTextColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.white, textBrightness);
        newColor.a = 1f;
        return newColor;
    }

    Color GetTabColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.black, tabDarkness);
        newColor.a = 1f;
        return newColor;
    }
}

using UnityEngine;
using System;
using System.Collections.Generic;

public class Environment
{

    public static Dictionary<string, Func<string>> variables = new Dictionary<string, Func<string>>() {
        {
            "PLAYER_NAME", () => {
                return GameManager.instance.player.playerName;
            }
        },
        {
            "CITY_NAME", () => {
                return GameManager.instance.cityManager.cityName;
            }
        },
        {
            "CURRENT_CYCLE", () => {
                return GameManager.instance.temporality.cycleNumber.ToString();
            }
        }
    };
}

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Environment
{

    static Dictionary<string, Func<string>> variables = new Dictionary<string, Func<string>>() {
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
        },
        {
            "TOTAL_CITIZEN_COUNT", () => {
                return GameManager.instance.populationManager.citizenList.Count.ToString();
            }
        },
        {
            "STOPPED_RIOTS_COUNT", () => {
                return GameManager.instance.achievementManager.stats.stoppedRiots.ToString();
            }
        },
        {
            "STOPPED_FIRES_COUNT", () => {
                return GameManager.instance.achievementManager.stats.stoppedFires.ToString();
            }
        },
        {
            "FIRES_COUNT", () => {
                return FireManager.currentFire.ToString();
            }
        },
        {
            "BRIDGE_COUNT", () => {
                return GameManager.instance.gridManagement.bridgesList.Count.ToString();
            }
        },
        {
            "HIGHEST_TOWER_HEIGHT", () => {
                return GameManager.instance.achievementManager.stats.maxTowerHeight.ToString();
            }
        },
        {
            "LONGEST_BRIDGE_LENGTH", () => {
                return GameManager.instance.achievementManager.stats.maxBridgeLength.ToString();
            }
        },
        {
            "GAME_COUNT", () => {
                return GameManager.instance.achievementManager.stats.gamesPlayed.ToString();
            }
        },
        {
            "ZERO_MOOD_POPULATION_COUNT", () => {
                int count = 0;
                foreach(Population pop in GameManager.instance.populationManager.populationTypeList){
                    if (GameManager.instance.populationManager.GetAverageMood(pop) <= 0f) {
                        count ++;
                    }
                }
                return count.ToString();
            }
        }
    };

    public static string GetVariable(string varName)
    {
        try {
            return variables[varName].Invoke();
        }
        catch(KeyNotFoundException e) {
            string msg = "Accessed unknown environment variable [" + varName + "]:\n" + e.ToString();
            Debug.LogError(msg);
            Logger.Error(msg);
            return "";
        }
    }

    public static string[] GetVarNames()
    {
        return variables.Keys.ToArray();
    }
}

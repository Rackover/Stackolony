using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Paths {

    static public string GetOptionsFile()
    {
        string path = Application.persistentDataPath + "/options";
        CreateDirectoryIfNotExists(path);
        string file = "options.txt";
        return path + "/" + file;
    }
    static public string GetSaveFile(string cityName)
    {
        string path = Application.persistentDataPath + "/savegame";
        CreateDirectoryIfNotExists(path);
        string file = cityName + ".bin";
        return path + "/" + file;
    }
    static public string GetLocFolder()
    {
        string path = Application.streamingAssetsPath + "/localization";
        CreateDirectoryIfNotExists(path);
        return path;
    }

    static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}

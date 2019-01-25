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
    static public string GetSaveFolder()
    {
        string path = Application.persistentDataPath + "/savegame";
        CreateDirectoryIfNotExists(path);
        return path;
    }
    static public string GetSaveFile(string cityName)
    {
        string path = GetSaveFolder();
        string file = cityName + ".bin";
        return path + "/" + file;
    }
    static public string GetLocFolder()
    {
        string path = Application.streamingAssetsPath + "/localization";
        CreateDirectoryIfNotExists(path);
        return path;
    }
    static public string GetBulletinFolder()
    {
        string path = Application.streamingAssetsPath + "/bulletins";
        CreateDirectoryIfNotExists(path);
        return path;
    }
    static public string GetBulletinFile()
    {
        string path = GetBulletinFolder();
        string file = "bulletins" + ".xml";
        return path + "/" + file;
    }
    static public string GetNamesFolder()
    {
        string path = Application.streamingAssetsPath + "/names";
        CreateDirectoryIfNotExists(path);
        return path;
    }
    static public string GetNamesFile()
    {
        string path = GetBulletinFolder();
        string file = "names" + ".txt";
        return path + "/" + file;
    }

    static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}

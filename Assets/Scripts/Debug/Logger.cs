﻿using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using UnityEngine;

public class Logger : MonoBehaviour
{
    StringBuilder builder = new StringBuilder();
    public string logFile = "stackolony.log";
    public string locale = "fr-FR";
    public int callerLength = 18;
    public int methodLength = 12;
    public int flushEverySecond = 1;
    public bool flushToUnityConsole = false;
    public enum type {DEBUG, INFO, WARNING, ERROR };
    string fullPath;
    CultureInfo culture;

    private void Awake()
    {
        culture = new CultureInfo(locale);
        string firstLine = "LOG START - " + DateTime.Now.ToString(culture.DateTimeFormat.LongTimePattern);
        fullPath = Application.persistentDataPath + "/" + logFile;
        File.WriteAllText(fullPath, firstLine+"\n");
        StartCoroutine(Flush());
    }

    public static void Debug(params string[] msgs) { LogMessage(type.DEBUG, msgs); }
    public static void Info(params string[] msgs) { LogMessage(type.INFO, msgs); }
    public static void Warn(params string[] msgs) { LogMessage(type.WARNING, msgs); }
    public static void Error(params string[] msgs) { LogMessage(type.ERROR, msgs); }
    public static void Throw(params string[] msgs) {
        LogMessage(type.ERROR, new string[1] { "================== FATAL ==================" });
        LogMessage(type.ERROR, msgs);
        UnityEngine.Debug.LogError(string.Join(" ", msgs).ToString());
        Crash();
    }

    private static void LogMessage(type type, params string[] msgs)
    {
        if (!GameManager.instance.ENABLE_LOGS) {
            return;
        }

        // Get informations from stackframe
        Logger loggerInstance = FindObjectOfType<Logger>();
        StackFrame sf = new StackFrame(2, true);
        string file = sf.GetFileName().Replace(Application.dataPath.Replace("/", "\\") + "\\Scripts\\", "");
        string method = sf.GetMethod().Name;

        // Formatting the "X=>Y" file/method pattern
        string caller =
            file.Substring(0, Mathf.Min(file.Length, loggerInstance.callerLength)).PadRight(loggerInstance.callerLength)
            + "=>" 
            + method.Substring(0, Mathf.Min(method.Length, loggerInstance.methodLength)).PadRight(loggerInstance.methodLength);

        // Debug line formatting
        string line =
            DateTime.Now.ToString(loggerInstance.culture.DateTimeFormat.LongTimePattern)
            + " ["
            + type.ToString()
            + "] "
            + "[" 
            + caller
            + "] - " 
            + string.Join(" ", msgs);
        loggerInstance.builder.Append(line).AppendLine();
    }

    private static void Crash()
    {
        Logger loggerInstance = FindObjectOfType<Logger>();
        loggerInstance.StartCoroutine(loggerInstance.Flush(true));
    }

    IEnumerator Flush(bool exit=false)
    {
        // Writes to disk
        string line = builder.ToString();
        File.AppendAllText(fullPath, line);
        if (flushToUnityConsole) {
            string[] lines = line.Split('\n');
            foreach(string consoleLine in lines) {
                UnityEngine.Debug.Log(consoleLine);
            }
        }

        // Kills the app if exit is set to true - useful to write logs before quitting
        if (exit) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Resets the string builder
        builder = new StringBuilder();
        yield return new WaitForSeconds(flushEverySecond);
        StartCoroutine(Flush());
        yield return true;
    }
}
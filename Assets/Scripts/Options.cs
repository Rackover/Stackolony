using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;

public class Options
{
    public enum Option { borderSensitivity, rotationSensitivity, grabSensitivity, musicVolume, sfxVolume, voiceVolume, lang, enableDrifting, animatedCitizens };    
    Dictionary<Option, IOption> options = new Dictionary<Option, IOption>();

    public interface IOption
    {
        void Reset();
        void SetFromString(string val);
        string ToString();
    };

    public interface IGenericTypeOption
    {
        Type GetGenericType();
        object[] GetRange();
    }

    public class SliderOption : IOption
    {
        public float defaultValue { get; }
        public float minValue { get; }
        public float maxValue { get; }
        public float value;

        public SliderOption(float min, float def, float max)
        {
            defaultValue = def;
            minValue = min;
            maxValue = max;
            value = defaultValue;
        }

        public void Set(float val)
        {
            if (val <= maxValue && val >= minValue) {
                value = val;
            }
        }

        public void SetFromString(string str)
        {
            Set(float.Parse(str));
        }


        public void Reset()
        {
            value = defaultValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class CheckboxOption : IOption
    {
        public bool defaultValue { get; }
        public bool value;

        public CheckboxOption(bool def)
        {
            defaultValue = def;
            value = defaultValue;
        }

        public void Set(bool val)
        {
            value = val;
        }

        public void Reset()
        {
            value = defaultValue;
        }

        public void SetFromString(string str)
        {
            Set(Convert.ToBoolean(str));
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class SelectOption<T> : IOption, IGenericTypeOption
    {
        public int defaultIndex { get; }
        public int index;
        public T[] possibleValues { get; private set; }
        public Action onValidate;

        public SelectOption(T[] _possibleValues, int _defaultIndex=0)
        {
            defaultIndex = _defaultIndex;
            index = defaultIndex;
            possibleValues = _possibleValues;
        }

        public void Set(int newIndex)
        {
            index = newIndex < possibleValues.Length ? newIndex : defaultIndex;
        }

        public void Set(T newValue)
        {
            for (int i = 0; i < possibleValues.Length; i++) {
                if (EqualityComparer<T>.Default.Equals(possibleValues[i], newValue)) {
                    index = i;
                    return;
                }
            }
        }

        public void SetRange(T[] range)
        {
            possibleValues = range;
        }
        public void Reset()
        {
            index = defaultIndex;
        }

        public void SetFromString(string str)
        {
            Set(Convert.ToInt32(str));
            onValidate.Invoke();
        }

        public override string ToString()
        {
            return index.ToString();
        }

        public object[] GetRange()
        {
            return possibleValues.Cast<object>().ToArray();
        }

        public Type GetGenericType()
        {
            return possibleValues[0].GetType();
        }
    }

    public Options()
    {
        options[Option.borderSensitivity] = new SliderOption(6f, 15f, 50f);
        options[Option.rotationSensitivity] = new SliderOption(0.2f, 1f, 3f);
        options[Option.grabSensitivity] = new SliderOption(0.1f, 0.8f, 2f);
        options[Option.musicVolume] = new SliderOption(0f, 1f, 1f);
        options[Option.sfxVolume] = new SliderOption(0f, 0.2f, 1f);
        options[Option.voiceVolume] = new SliderOption(0f, 4f, 10f);

        // Localization is a bit tricky to load
        SelectOption<Localization.Lang> langOption = new SelectOption<Localization.Lang>(GameManager.instance.localization.GetLanguages().ToArray());
        options[Option.lang] = langOption;
        langOption.onValidate += delegate {
            if (langOption.possibleValues.Length > 0) {
                GameManager.instance.localization.LoadLocalization(langOption.possibleValues[langOption.index]);
            }
        };

        options[Option.enableDrifting] = new CheckboxOption(true);
        options[Option.animatedCitizens] = new CheckboxOption(true);
    }

    public override string ToString()
    {
        string data = "";
        foreach (KeyValuePair<Option, IOption> option in options) {
            data += option.Key.ToString() + "=" + option.Value.ToString() + "\n";
        }
        return data;
    }

    public Dictionary<Option, IOption> Get()
    {
        return options;
    }

    public IOption Get(Option key)
    {
        return options[key];
    }

    public float GetFloat(Option key)
    {
        return ((SliderOption)options[key]).value;
    }

    public bool GetBool(Option key)
    {
        return ((CheckboxOption)options[key]).value;
    }

    void LoadFromString(string data)
    {
        string[] lines = data.Split('\n');
        foreach (string line in lines) {
            string[] option = line.Split('=');
            if (option.Length < 2) {
                continue;
            }
            try {
                options[(Option)Enum.Parse(typeof(Option), option[0])].SetFromString(option[1]);
            }
            catch(KeyNotFoundException e) {
                // garbage option, skipping
            }
        }
    }

    public void WriteToDisk(string path)
    {
        if (File.Exists(path)) {
            File.Delete(path);
        }
        File.WriteAllText(path, ToString());
        Logger.Info("Successfully wrote options file to [" + path + "]");
    }

    public void LoadFromDisk(string path)
    {
        if (!File.Exists(path)) {
            return;
        }
        string content = File.ReadAllText(path);
        LoadFromString(content);
    }

    public void WriteAndLoadFromDisk(string path)
    {
        WriteToDisk(path);
        LoadFromDisk(path);
    }

}
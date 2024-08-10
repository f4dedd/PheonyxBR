using System;
using System.IO;
using System.Reflection;
using Godot;
using Godot.Collections;
using Menu;

namespace Phoenix;

public class Constants
{
    public static string RootFolder {get;} = Directory.GetCurrentDirectory();
    public static string UserFolder {get;} = OS.GetUserDataDir();
    public static float CursorSize {get;} = 0.2625f;
    public static float GridSize {get;} = 3.0f;
    public static Vector2 Bounds {get;} = new Vector2(GridSize / 2 - CursorSize / 2, GridSize / 2 - CursorSize / 2);
    public static float HitBoxSize {get;} = 0.07f;
    public static float HitWindow {get;} = 55f;
}

public class Settings
{
    public static string Skin {get; set;} = "default";
    public static float Sensitivity {get; set;} = 0.66f;
    public static float Parallax {get; set;} = 0.1f;
    public static float ApproachRate {get; set;} = 35.0f;
    public static float ApproachDistance {get; set;} = 16.0f;
    public static float ApproachTime {get; set;} = ApproachDistance / ApproachRate;
    public static float FadeIn {get; set;} = 0;
    public static bool FadeOut {get; set;} = true;
    public static bool Pushback {get; set;} = true;
    public static float NoteSize {get; set;} = 0.875f;
    public static string[] Colors {get; set;} = {"#00ffed", "#ff8ff9"};
}

public class Util
{
    private static bool Initialized = false;
    private static string[] UserDirectories = new string[]{"maps", "profiles", "skins", "replays"};
    private static string[] SkinFiles = new string[]{"cursor.png", "grid.png", "note.obj"};
    private static Dictionary<string, bool> IgnoreProperties = new Dictionary<string, bool>(){
        ["_import_path"] = true,
        ["owner"] = true,
        
        ["anchor_left"] = true,
        ["anchor_top"] = true,
        ["anchor_right"] = true,
        ["anchor_bottom"] = true,

        ["layout_mode"] = true,
        ["global_position"] = true
    };

    public static void Setup()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;

        if (!Directory.Exists($"{Constants.UserFolder}/cache"))
        {
            Directory.CreateDirectory($"{Constants.UserFolder}/cache");
        }

        foreach (string cacheFile in Directory.GetFiles($"{Constants.UserFolder}/cache"))
        {
            File.Delete(cacheFile);
        }

        for (int i = 0; i < UserDirectories.Length; i++)
        {
            string Folder = UserDirectories[i];

            if (!Directory.Exists($"{Constants.UserFolder}/{Folder}"))
		    {
		    	Directory.CreateDirectory($"{Constants.UserFolder}/{Folder}");
		    }
        }

        if (!Directory.Exists($"{Constants.UserFolder}/skins/default"))
        {
            Directory.CreateDirectory($"{Constants.UserFolder}/skins/default");
        }

        foreach (string skinFile in SkinFiles)
        {
            Godot.FileAccess file = Godot.FileAccess.Open($"res://skin/{skinFile}", Godot.FileAccess.ModeFlags.Read);
            
            if (!File.Exists($"{Constants.UserFolder}/skins/default/{skinFile}"))
            {
                Godot.FileAccess.Open($"{Constants.UserFolder}/skins/default/{skinFile}", Godot.FileAccess.ModeFlags.Write).StoreBuffer(file.GetBuffer((long)file.GetLength()));
            }
        }

        if (!File.Exists($"{Constants.UserFolder}/current_profile.txt"))
        {
            File.WriteAllText($"{Constants.UserFolder}/current_profile.txt", "default");
        }

        if (!File.Exists($"{Constants.UserFolder}/profiles/default.json"))
        {
            SaveSettings();
        }

        LoadSettings(Godot.FileAccess.Open($"{Constants.UserFolder}/current_profile.txt", Godot.FileAccess.ModeFlags.Read).GetLine());
    }

    public static void SaveSettings(string profile = "default")
    {
        Dictionary data = new Dictionary(){
            ["_Version"] = 1,
            ["Skin"] = Settings.Skin,
            ["Sensitivity"] = Settings.Sensitivity,
            ["Parallax"] = Settings.Parallax,
            ["ApproachRate"] = Settings.ApproachRate,
            ["ApproachDistance"] = Settings.ApproachDistance,
            ["FadeIn"] = Settings.FadeIn,
            ["FadeOut"] = Settings.FadeOut,
            ["Pushback"] = Settings.Pushback,
            ["NoteSize"] = Settings.NoteSize,
            ["Colors"] = Settings.Colors
        };

        File.WriteAllText($"{Constants.UserFolder}/profiles/{profile}.json", Json.Stringify(data, "\t"));
    }

    public static void LoadSettings(string profile = "default")
    {
        try
        {
            Godot.FileAccess file = Godot.FileAccess.Open($"{Constants.UserFolder}/profiles/{profile}.json", Godot.FileAccess.ModeFlags.Read);
            Dictionary data = (Dictionary)Json.ParseString(file.GetAsText());

            file.Close();

            Settings.Skin = (string)data["Skin"];
            Settings.Sensitivity = (float)data["Sensitivity"];
            Settings.Parallax = (float)data["Parallax"];
            Settings.ApproachRate = (float)data["ApproachRate"];
            Settings.ApproachDistance = (float)data["ApproachDistance"];
            Settings.ApproachTime = Settings.ApproachDistance / Settings.ApproachRate;
            Settings.FadeIn = (float)data["FadeIn"];
            Settings.FadeOut = (bool)data["FadeOut"];
            Settings.Pushback = (bool)data["Pushback"];
            Settings.NoteSize = (float)data["NoteSize"];
            Settings.Colors = (string[])data["Colors"];

            MainMenu.Notify("Loaded settings successfully");
        }
        catch (Exception exception)
        {
            MainMenu.Notify("Could not load settings", 2);
            throw Logger.Error($"Could not load settings; {exception.Message}");
        }
    }

    public static T Clone<T>(T reference, bool recursive = true) where T : Node, new()
    {
        T clone = new T();
        
        foreach (Dictionary entry in reference.GetPropertyList())
        {
            if ((int)entry["type"] == 0 || IgnoreProperties.ContainsKey((string)entry["name"]))
            {
                continue;
            }

            clone.Set((string)entry["name"], reference.Get((string)entry["name"]));
        }

        if (recursive)
        {
            Array<Node> children = reference.GetChildren();
            
            for (int i = 0; i < children.Count; i++)
            {   
                Node childClone = (Node)typeof(Util).GetMethod("Clone", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(children[i].GetType()).Invoke(null, new object[]{children[i], recursive});

                clone.AddChild(childClone);
            }
        }

        return clone;
    }
}
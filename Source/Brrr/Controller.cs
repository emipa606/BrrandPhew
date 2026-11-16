using Mlie;
using UnityEngine;
using Verse;

namespace Brrr;

public class Controller : Mod
{
    private static Settings settings;
    public static string CurrentVersion;

    public Controller(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
        CurrentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "Brrr.Name".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoWindowContents(inRect);
    }
}
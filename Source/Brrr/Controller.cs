using UnityEngine;
using Verse;

namespace Brrr;

public class Controller : Mod
{
    public static Settings Settings;

    public Controller(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
    }

    public override string SettingsCategory()
    {
        return "Brrr.Name".Translate();
    }

    public override void DoSettingsWindowContents(Rect canvas)
    {
        Settings.DoWindowContents(canvas);
    }
}
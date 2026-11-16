using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Brrr;

[StaticConstructorOnStartup]
internal static class MultiplayerSupport
{
    private static readonly Harmony harmony = new("rimworld.pelador.brr.multiplayersupport");

    static MultiplayerSupport()
    {
        if (!MP.enabled)
        {
            return;
        }

        var array = new[]
        {
            AccessTools.Method(typeof(BrrrGlobals), nameof(BrrrGlobals.GetNearestSafeRoofedCell)),
            AccessTools.Method(typeof(BrrrGlobals), nameof(BrrrGlobals.GenRnd100)),
            AccessTools.Method(typeof(BrrrGlobals), nameof(BrrrGlobals.GenNewRRJob)),
            AccessTools.Method(typeof(JobGiver_Ooh), "TryFindOohCell"),
            AccessTools.Method(typeof(Toils_BrrrLayDown), nameof(Toils_BrrrLayDown.BrrrLayDown))
        };
        foreach (var methodInfo in array)
        {
            FixRNG(methodInfo);
        }
    }

    private static void FixRNG(MethodInfo method)
    {
        harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), nameof(FixRNGPre)),
            new HarmonyMethod(typeof(MultiplayerSupport), nameof(FixRNGPos)));
    }

    private static void FixRNGPre()
    {
        Rand.PushState(Find.TickManager.TicksAbs);
    }

    private static void FixRNGPos()
    {
        Rand.PopState();
    }
}
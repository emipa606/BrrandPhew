using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Brrr;

public class JobDriver_OohSkygaze : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        var gaze = new Toil
        {
            initAction = delegate { pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp; },
            tickAction = delegate { BrrrGlobals.JoyTickOohCheckEnd(pawn); },
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = job.def.joyDuration
        };
        gaze.FailOn(() => pawn.Position.Roofed(pawn.Map));
        gaze.AddFailCondition(delegate
        {
            var actor = gaze.actor;
            var needs = actor.needs;
            if (needs?.food == null)
            {
                return false;
            }

            var needs2 = actor.needs;
            var num = needs2 != null ? new float?(needs2.food.CurLevelPercentage) : null;
            var percentageThreshHungry = actor.needs.food.PercentageThreshHungry;
            return num.GetValueOrDefault() < percentageThreshHungry && num != null;
        });
        yield return gaze;
    }

    public override string GetReport()
    {
        if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
        {
            return "Brrr.OohEclipse".Translate();
        }

        if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Aurora))
        {
            return "Brrr.OohAurora".Translate();
        }

        var num = GenCelestial.CurCelestialSunGlow(Map);
        switch (num)
        {
            case < 0.1f:
                return "Brrr.OohStarGazing".Translate();
            case >= 0.65f:
                return "Brrr.OohClouds".Translate();
            default:
                return GenLocalDate.DayPercent(pawn) < 0.5f
                    ? "Brrr.OohSunrise".Translate()
                    : "Brrr.OohSunset".Translate();
        }
    }
}
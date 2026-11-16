using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Brrr;

public class JobDriver_PhewRecovery : JobDriver
{
    private const TargetIndex BedOrRestSpotIndex = TargetIndex.A;
    private const TargetIndex SwimSpotIndex = TargetIndex.B;
    private const float SwimCoolingFactor = 0.1f;
    private const float SwimCoolingMin = 0.01f;
    private const float SwimCoolingMax = 0.05f;

    private Building_Bed Bed => job.GetTarget(BedOrRestSpotIndex).Thing as Building_Bed;

    private bool IsSwimming => job.GetTarget(SwimSpotIndex).IsValid;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!job.GetTarget(BedOrRestSpotIndex).HasThing && !IsSwimming)
        {
            return true;
        }

        if (IsSwimming)
        {
            return pawn.Reserve(job.GetTarget(SwimSpotIndex), job, errorOnFailed: errorOnFailed);
        }

        var localPawn = pawn;
        LocalTargetInfo target = Bed;
        var localJob = job;
        var sleepingSlotsCount = Bed.SleepingSlotsCount;
        var stackCount = 0;
        return localPawn.Reserve(target, localJob, sleepingSlotsCount, stackCount, null, errorOnFailed);
    }

    public override bool CanBeginNowWhileLyingDown()
    {
        return !IsSwimming && JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(BedOrRestSpotIndex));
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        if (IsSwimming)
        {
            var gotoToil = Toils_Goto.GotoCell(SwimSpotIndex, PathEndMode.OnCell);
            gotoToil.tickIntervalAction = _ => CheckForSwimmingPose();
            gotoToil.AddFinishAction(CheckForSwimmingPose);
            yield return gotoToil;
            yield return MakeSwimToil();
        }
        else
        {
            var hasBed = job.GetTarget(BedOrRestSpotIndex).HasThing;
            if (hasBed)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(BedOrRestSpotIndex);
                yield return Toils_Bed.GotoBed(BedOrRestSpotIndex);
            }
            else
            {
                yield return Toils_Goto.GotoCell(BedOrRestSpotIndex, PathEndMode.OnCell);
            }

            yield return Toils_BrrrLayDown.BrrrLayDown(BedOrRestSpotIndex, hasBed);
        }
    }

    public override string GetReport()
    {
        if (IsSwimming)
        {
            return "Brrr.PhewRecoverSwimming".Translate();
        }

        return asleep ? "Brrr.PhewRecoverSleeping".Translate() : "Brrr.PhewRecoverResting".Translate();
    }

    private Toil MakeSwimToil()
    {
        var swimToil = ToilMaker.MakeToil("PhewSwim");
        swimToil.initAction = CheckForSwimmingPose;
        swimToil.tickIntervalAction = delta =>
        {
            CheckForSwimmingPose();
            if (!job.swimming)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            pawn.mindState.lastSwamTick = GenTicks.TicksGame;
            if (!CoolDownWhileSwimming(delta))
            {
                EndJobWith(JobCondition.Succeeded);
            }
        };
        swimToil.defaultCompleteMode = ToilCompleteMode.Never;
        swimToil.AddFailCondition(() =>
        {
            var needs = pawn.needs;
            if (needs?.food != null && needs.food.CurLevelPercentage < needs.food.PercentageThreshHungry)
            {
                return true;
            }

            return needs?.joy != null && needs.joy.CurLevelPercentage < Settings.JoySev / 100f &&
                   Settings.AllowJoy;
        });
        swimToil.AddFinishAction(() => job.swimming = false);
        return swimToil;
    }

    /// <summary>
    ///     Reduces heatstroke faster than vanilla comfortable-temperature recovery
    ///     (severity * 0.027, clamped 0.0015-0.015 per 60 ticks).
    /// </summary>
    /// <returns>True while the pawn still has heatstroke.</returns>
    private bool CoolDownWhileSwimming(int delta)
    {
        var heatstroke = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);
        if (heatstroke == null)
        {
            return false;
        }

        var reduction = Mathf.Clamp(heatstroke.Severity * SwimCoolingFactor, SwimCoolingMin, SwimCoolingMax) *
                        (delta / 60f);
        if (heatstroke.Severity - reduction <= 0f)
        {
            pawn.health.RemoveHediff(heatstroke);
            return false;
        }

        heatstroke.Severity -= reduction;
        return true;
    }

    private void CheckForSwimmingPose()
    {
        job.swimming = pawn.Position.GetTerrain(pawn.Map).IsWater;
    }
}
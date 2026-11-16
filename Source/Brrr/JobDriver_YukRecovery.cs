using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Brrr;

public class JobDriver_YukRecovery : JobDriver
{
    private const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

    private Building_Bed Bed => (Building_Bed)job.GetTarget(BedOrRestSpotIndex).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!job.GetTarget(BedOrRestSpotIndex).HasThing)
        {
            return true;
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
        return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(BedOrRestSpotIndex));
    }

    protected override IEnumerable<Toil> MakeNewToils()
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

    public override string GetReport()
    {
        return asleep ? "Brrr.YukRecoverSleeping".Translate() : "Brrr.YukRecoverResting".Translate();
    }
}
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Brrr;

public static class BrrrGlobals
{
    public static Region BrrrClosestRegionWithinTemperatureRange(IntVec3 root, Map map, FloatRange tempRange,
        TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
    {
        var region = root.GetRegion(map, traversableRegionTypes);
        if (region == null)
        {
            return null;
        }

        Region foundReg = null;

        RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
        return foundReg;

        bool entryCondition(Region from, Region r)
        {
            return r.Allows(traverseParms, false);
        }

        bool regionProcessor(Region r)
        {
            if (r.IsDoorway)
            {
                return false;
            }

            if (!tempRange.Includes(r.Room.Temperature))
            {
                return false;
            }

            if (!Settings.AllowUnsafeAreas && !r.Cells.Any(vec3 => vec3.InAllowedArea(traverseParms.pawn)))
            {
                return false;
            }

            foundReg = r;
            return true;
        }
    }

    public static IntVec3 GetNearestSafeRoofedCell(Pawn pawn, IntVec3 root, Map map, FloatRange tempRange)
    {
        RCellFinder.TryFindRandomCellNearWith(root, baseValidator, map, out var cell);
        return cell;

        bool baseValidator(IntVec3 chkcell)
        {
            if (!chkcell.Standable(map))
            {
                return false;
            }

            if (chkcell.GetDangerFor(pawn, map) != Danger.None)
            {
                return false;
            }

            if (!chkcell.Roofed(map))
            {
                return false;
            }

            if (chkcell.IsPolluted(map))
            {
                return false;
            }

            if (!tempRange.Includes(chkcell.GetRoom(map).Temperature))
            {
                return false;
            }

            if (!Settings.AllowUnsafeAreas && !chkcell.InAllowedArea(pawn))
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(chkcell, PathEndMode.OnCell, Danger.None))
            {
                return false;
            }

            var room = chkcell.GetRoom(map);
            return pawn.IsPrisoner == room is { IsPrisonCell: true };
        }
    }

    public static void JoyTickOohCheckEnd(Pawn pawn,
        JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f)
    {
        var curJob = pawn.CurJob;
        if (curJob.def.joyKind == null)
        {
            Log.Warning("This method can only be called for jobs with joyKind.");
            return;
        }

        pawn.needs.joy?.GainJoy(extraJoyGainFactor * curJob.def.joyGainRate * 0.36f / 2500f, curJob.def.joyKind);
        if (curJob.def.joySkill != null)
        {
            pawn.skills?.GetSkill(curJob.def.joySkill).Learn(curJob.def.joyXpPerTick);
        }

        if (!curJob.ignoreJoyTimeAssignment && !pawn.GetTimeAssignment().allowJoy)
        {
            pawn.jobs?.curDriver.EndJobWith(JobCondition.InterruptForced);
        }

        if (!(pawn.needs.outdoors.CurLevelPercentage >= 0.75f))
        {
            return;
        }

        if (fullJoyAction == JoyTickFullJoyAction.EndJob)
        {
            pawn.jobs?.curDriver.EndJobWith(JobCondition.Succeeded);
            return;
        }

        if (fullJoyAction != JoyTickFullJoyAction.GoToNextToil)
        {
            return;
        }

        pawn.jobs?.curDriver.ReadyForNextToil();
    }

    public static bool BrrrAnimalIsFollowing(Pawn pawn)
    {
        return pawn.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted &&
               pawn.training.HasLearned(TrainableDefOf.Obedience) &&
               ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn);
    }

    public static int GenRnd100()
    {
        return Rand.Range(1, 100);
    }

    public static Job GenNewRRJob(JobDef def, Region reg)
    {
        return new Job(def, reg.RandomCell);
    }
}
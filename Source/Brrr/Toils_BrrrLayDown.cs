using RimWorld;
using Verse;
using Verse.AI;

namespace Brrr;

public static class Toils_BrrrLayDown
{
    private const int TicksBetweenSleepZs = 100;

    private const float GroundRestEffectiveness = 0.8f;

    private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

    public static Toil BrrrLayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs = false,
        bool canSleep = true, bool gainRestAndHealth = true)
    {
        var layDown = new Toil();
        layDown.initAction = delegate
        {
            var actor3 = layDown.actor;
            actor3.pather.StopDead();
            var curDriver3 = actor3.jobs.curDriver;
            if (hasBed)
            {
                if (!((Building_Bed)actor3.CurJob.GetTarget(bedOrRestSpotIndex).Thing).OccupiedRect()
                    .Contains(actor3.Position))
                {
                    Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor3);
                    actor3.jobs.EndCurrentJob(JobCondition.Errored);
                    return;
                }

                actor3.jobs.posture = PawnPosture.LayingInBed;
            }
            else
            {
                actor3.jobs.posture = PawnPosture.LayingOnGroundNormal;
            }

            curDriver3.asleep = false;
            if (actor3.mindState.applyBedThoughtsTick == 0)
            {
                actor3.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
                actor3.mindState.applyBedThoughtsOnLeave = false;
            }

            if (actor3.ownership != null && actor3.CurrentBed() != actor3.ownership.OwnedBed)
            {
                ThoughtUtility.RemovePositiveBedroomThoughts(actor3);
            }
        };
        layDown.tickIntervalAction = delegate(int delta)
        {
            var actor2 = layDown.actor;
            var curJob = actor2.CurJob;
            var curDriver2 = actor2.jobs.curDriver;
            var buildingBed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
            actor2.GainComfortFromCellIfPossible(delta);
            if (!curDriver2.asleep)
            {
                if (canSleep && (actor2.needs.rest != null && RestUtility.CanFallAsleep(actor2) || curJob.forceSleep))
                {
                    curDriver2.asleep = true;
                }
            }
            else if (!canSleep ||
                     (actor2.needs.rest == null || !RestUtility.CanFallAsleep(actor2)) && !curJob.forceSleep)
            {
                curDriver2.asleep = false;
            }

            if (curDriver2.asleep && gainRestAndHealth && actor2.needs.rest != null)
            {
                var restEffectiveness =
                    buildingBed == null ||
                    !buildingBed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)
                        ? GroundRestEffectiveness
                        : buildingBed.GetStatValue(StatDefOf.BedRestEffectiveness);
                for (var i = 0; i < delta; i++)
                {
                    actor2.needs.rest.TickResting(restEffectiveness);
                }
            }

            if (actor2.mindState.applyBedThoughtsTick != 0 &&
                actor2.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
            {
                applyBedThoughts(actor2);
                actor2.mindState.applyBedThoughtsTick += 60000;
                actor2.mindState.applyBedThoughtsOnLeave = true;
            }

            if (actor2.IsHashIntervalTick(TicksBetweenSleepZs, delta) && !actor2.Position.Fogged(actor2.Map))
            {
                if (curDriver2.asleep)
                {
                    FleckMaker.ThrowMetaIcon(actor2.Position, actor2.Map, FleckDefOf.SleepZ);
                }

                if (gainRestAndHealth && actor2.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
                {
                    FleckMaker.ThrowMetaIcon(actor2.Position, actor2.Map, FleckDefOf.HealingCross);
                }
            }

            if (actor2.ownership != null && buildingBed is { Medical: false } &&
                !buildingBed.OwnersForReading.Contains(actor2))
            {
                if (actor2.Downed)
                {
                    actor2.Position = CellFinder.RandomClosewalkCellNear(actor2.Position, actor2.Map, 1);
                }

                actor2.jobs.EndCurrentJob(JobCondition.Incompletable);
                return;
            }

            if (lookForOtherJobs && actor2.IsHashIntervalTick(GetUpOrStartJobWhileInBedCheckInterval, delta))
            {
                actor2.jobs.CheckForJobOverride();
            }
        };
        layDown.defaultCompleteMode = ToilCompleteMode.Never;
        if (hasBed)
        {
            layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
        }

        layDown.AddFailCondition(delegate
        {
            var actor = layDown.actor;
            var needs = actor.needs;
            if (needs?.food != null &&
                actor.needs.food.CurLevelPercentage < actor.needs.food.PercentageThreshHungry)
            {
                return true;
            }

            var needs2 = actor.needs;
            return needs2?.joy != null && !actor.RaceProps.Animal &&
                   actor.needs.joy.CurLevelPercentage < Settings.JoySev / 100f && Settings.AllowJoy;
        });
        layDown.AddFailCondition(delegate
        {
            var actor = layDown.actor;
            var hypoHed = actor.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
            var heatHed = actor.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);
            var toxHed = actor.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            var redHed = actor.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodRage);
            var breathHed =
                actor.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamed("OxygenStarvation", false));
            var exposureHed =
                actor.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamed("VacuumExposure", false));
            var ceSmokeHed =
                actor.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamed("BrrrSmokeInhalation", false));
            return hypoHed == null && heatHed == null && toxHed == null && breathHed == null && exposureHed == null &&
                   redHed == null && ceSmokeHed == null;
        });
        layDown.AddFinishAction(delegate
        {
            var actor = layDown.actor;
            var curDriver = actor.jobs.curDriver;
            if (actor.mindState.applyBedThoughtsOnLeave)
            {
                applyBedThoughts(actor);
            }

            curDriver.asleep = false;
        });
        return layDown;
    }

    private static void applyBedThoughts(Pawn actor)
    {
        if (actor.needs.mood == null)
        {
            return;
        }

        var buildingBed = actor.CurrentBed();
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
        if (actor.GetRoom().PsychologicallyOutdoors)
        {
            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside);
        }

        if (buildingBed == null || buildingBed.CostListAdjusted().Count == 0)
        {
            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround);
        }

        if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
        {
            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold);
        }

        if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
        {
            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat);
        }

        if (buildingBed == null || buildingBed != actor.ownership.OwnedBed || buildingBed.ForPrisoners ||
            actor.story.traits.HasTrait(TraitDefOf.Ascetic))
        {
            return;
        }

        ThoughtDef thoughtDef = null;
        if (buildingBed.GetRoom().Role == RoomRoleDefOf.Bedroom)
        {
            thoughtDef = ThoughtDefOf.SleptInBedroom;
        }
        else if (buildingBed.GetRoom().Role == RoomRoleDefOf.Barracks)
        {
            thoughtDef = ThoughtDefOf.SleptInBarracks;
        }

        if (thoughtDef == null)
        {
            return;
        }

        var scoreStageIndex =
            RoomStatDefOf.Impressiveness.GetScoreStageIndex(buildingBed.GetRoom()
                .GetStat(RoomStatDefOf.Impressiveness));
        if (thoughtDef.stages[scoreStageIndex] != null)
        {
            actor.needs.mood.thoughts.memories.TryGainMemory(
                ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
        }
    }
}
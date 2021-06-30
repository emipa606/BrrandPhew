﻿using RimWorld;
using Verse;
using Verse.AI;

namespace Brrr
{
    // Token: 0x02000011 RID: 17
    public class JobGiver_AnimalGasp : ThinkNode_JobGiver
    {
        // Token: 0x06000038 RID: 56 RVA: 0x000031FC File Offset: 0x000013FC
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!Settings.UseGasp || !Settings.ApplyAnimals)
            {
                return null;
            }

            if (pawn?.Map == null)
            {
                return null;
            }

            if (!pawn.AnimalOrWildMan() || pawn.IsWildMan())
            {
                return null;
            }

            if (pawn.Faction == null)
            {
                return null;
            }

            if (pawn.Faction != Faction.OfPlayer)
            {
                return null;
            }

            if (pawn.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted &&
                pawn.training.HasLearned(TrainableDefOf.Obedience) &&
                ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn))
            {
                return null;
            }

            if (pawn.InMentalState)
            {
                return null;
            }

            var needs = pawn.needs;
            if (needs?.food != null && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry)
            {
                return null;
            }

            if (RestUtility.DisturbancePreventsLyingDown(pawn))
            {
                return null;
            }

            if (pawn.CurJobDef == BrrrJobDef.Brrr_BrrrRecovery || pawn.CurJobDef == BrrrJobDef.Brrr_GaspRecovery ||
                pawn.CurJobDef == BrrrJobDef.Brrr_PhewRecovery || pawn.CurJobDef == BrrrJobDef.Brrr_YukRecovery)
            {
                return null;
            }

            var OxStarve = DefDatabase<HediffDef>.GetNamed("OxygenStarvation", false);
            if (OxStarve == null || !pawn.health.hediffSet.HasHediff(OxStarve))
            {
                return null;
            }

            var HedBreath = pawn.health.hediffSet.GetFirstHediffOfDef(OxStarve);
            if (HedBreath == null || HedBreath.Severity < Settings.UnsafeGaspSev / 100f)
            {
                return null;
            }

            Thing BrrrBed = null;
            var FindBed = RestUtility.FindBedFor(pawn, pawn, false, true);
            if (FindBed != null && FindBed.Position.Roofed(pawn.Map) &&
                pawn.ComfortableTemperatureRange().Includes(FindBed.GetRoom().Temperature))
            {
                BrrrBed = FindBed;
            }

            if (BrrrBed != null)
            {
                return new Job(BrrrJobDef.Brrr_GaspRecovery, BrrrBed);
            }

            var tempRange = pawn.ComfortableTemperatureRange();
            var SafeCell = BrrrGlobals.GetNearestSafeRoofedCell(pawn, pawn.Position, pawn.Map, tempRange);
            return new Job(BrrrJobDef.Brrr_GaspRecovery, SafeCell);
        }

        // Token: 0x0200002D RID: 45
        [DefOf]
        public static class BrrrJobDef
        {
            // Token: 0x04000054 RID: 84
            public static JobDef Brrr_BrrrRecovery;

            // Token: 0x04000055 RID: 85
            public static JobDef Brrr_PhewRecovery;

            // Token: 0x04000056 RID: 86
            public static JobDef Brrr_YukRecovery;

            // Token: 0x04000057 RID: 87
            public static JobDef Brrr_GaspRecovery;
        }
    }
}
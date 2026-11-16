.github/copilot-instructions.md

# GitHub Copilot Instructions for Brrr and Phew (Continued) Mod

## Mod Overview and Purpose
Brrr and Phew (Continued) is an update to the original Brrr mod, enhancing the colony AI to help pawns avoid negative effects of hypothermia, heatstroke, toxic buildup, and cabin fever. The mod allows for early responses to these conditions, enabling pawns to take preventive measures to avoid severe aspects.

## Key Features and Systems
- **Brrr (Hypothermia and Heatstroke)**: Pawns react to initial buildup of hypothermia and heatstroke, seeking shelter or resting in a comfortable temperature range.
- **Phew (Toxic Buildup)**: Pawns take steps to avoid toxic buildup, with a longer recovery period.
- **Phew swimming (Heatstroke, Odyssey DLC)**: Colonists with heatstroke may swim in nearby safe (non-toxic, reachable) water; while swimming, `JobDriver_PhewRecovery` reduces heatstroke faster than vanilla resting and ends the job once it is gone.
- **Ooh (Cabin Fever)**: Pawns take forced breaks, either wandering outside or sky watching, to alleviate cabin fever.
- **Animal Behavior**: Colony animals also mirror reactive behavior to relevant environmental situations.
- **Mod Options**: Allows enabling/disabling AI behavior, setting recovery initiation levels, and controlling joy activities during recovery periods.

## Coding Patterns and Conventions
- **Harmony Patching**: The mod uses Harmony for patching vanilla RimWorld methods to integrate new behaviors and reactions.
- **XML Integration**: XML files define new job definitions and integrate them into the game.
- **C# Classes and Methods**: C# code implements job drivers, global settings, and utility methods for managing pawns' behaviors.
- **Naming Conventions**: Use descriptive names for classes, methods, and variables to improve readability and maintainability.
- **Code Comments**: Add comments to explain complex logic and reasoning behind specific implementations.

## XML Integration
- **Job Definitions**: XML files (e.g., `Defs_Brrr.xml`, `Defs_Gasp.xml`) define new jobs that pawns can perform, such as seeking shelter or resting.
- **Def Injection**: Ensure that XML definitions are correctly injected into the game's data structure using RimWorld's def injection mechanism.

## Harmony Patching
- **Patch Points**: Identify appropriate vanilla methods to patch for adding new behaviors, such as `JobDriver_BeginTryDoJob` or `Pawn_JobTracker_StartJob`.
- **Patch Logic**: Implement logic in the patch to check conditions and trigger new jobs or behaviors.
- **Patch Order**: Ensure that patches are applied in the correct order to avoid conflicts with other mods or vanilla game logic.

## Suggestions for Copilot
- **Job Drivers**: Use Copilot to generate job drivers for new behaviors, such as `JobDriver_OohGoForWalk` or `JobDriver_BrrrRecovery`.
- **XML Definitions**: Use Copilot to help create or modify XML job definitions, ensuring they integrate correctly with the game.
- **Utility Methods**: Use Copilot to generate utility methods for common tasks, such as finding nearest safe cells or checking pawn conditions.
- **Harmony Patches**: Use Copilot to assist in creating Harmony patches, focusing on identifying and implementing the necessary patch points.
- **Documentation**: Use Copilot to generate or assist in documenting new classes, methods, and XML definitions to improve code maintainability.

## Example Copilot Usage
- **Creating a New Job Driver**:
  csharp
  using RimWorld;
  using Verse;

  namespace Brrr
  {
      public class JobDriver_OohGoForWalk : JobDriver
      {
          public override JobState CanBeginNowWhileLyingDown()
          {
              return false;
          }

          public override void MakeNewToils()
          {
              Toil toil = ToilMaker.MakeToil("MoveToCell");
              toil.AddFinishAction(delegate
              {
                  // Implement walk behavior
              });
              this.AddToil(toil);
          }

          public override bool TryMakePreToilReservations(ReservationManager resMan)
          {
              // Implement reservation logic
              return true;
          }
      }
  }
  
- **Creating a Harmony Patch**:
  csharp
  using HarmonyLib;
  using RimWorld;
  using Verse;

  namespace Brrr
  {
      [HarmonyPatch(typeof(JobDriver_BeginTryDoJob), nameof(JobDriver_BeginTryDoJob.StartJob))]
      public static class HarmonyPatch_BeginTryDoJob_StartJob
      {
          public static void Postfix(ref JobDriver __instance, ref Pawn __state)
          {
              // Implement patch logic
          }
      }
  }
  

## Conclusion
By following these coding patterns and conventions, and utilizing GitHub Copilot effectively, you can enhance the Brrr and Phew (Continued) mod to provide robust and intuitive behaviors for pawns in RimWorld. Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.


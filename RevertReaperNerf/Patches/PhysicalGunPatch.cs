using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace AetharNet.Mods.ZumbiBlocks2.RevertReaperNerf.Patches;

[HarmonyPatch(typeof(PhysicalGun))]
public static class PhysicalGunPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PhysicalGun.SetShotDamage))]
    public static IEnumerable<CodeInstruction> RevertStaggerChance(IEnumerable<CodeInstruction> instructions)
    {
        var playerDamageConstructor = AccessTools.Constructor(typeof(PlayerDamage), [typeof(Vector3), typeof(float), typeof(bool), typeof(float)]);
        return new CodeMatcher(instructions)
            .MatchStartForward([
                new CodeMatch(OpCodes.Ldc_R4, 0.5f),
                new CodeMatch(OpCodes.Newobj, playerDamageConstructor)
            ])
            .RemoveInstruction()
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldarg_0),
                Transpilers.EmitDelegate(GetStaggerChance)
            ])
            .InstructionEnumeration();
    }

    private static float GetStaggerChance(PhysicalGun physicalGun)
    {
        // The change to stagger chance actually affects all gun-wielding bosses,
        // but this mod specifically reverts changes to the Reaper
        return physicalGun.name == "ReaperAr15" ? 1f : 0.5f;
    }
}

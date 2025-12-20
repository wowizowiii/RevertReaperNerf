using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AetharNet.Mods.ZumbiBlocks2.RevertReaperNerf.Patches;

[HarmonyPatch(typeof(ReaperBehaviour))]
public static class ReaperBehaviourPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(MethodType.Constructor, typeof(Zombie))]
    public static IEnumerable<CodeInstruction> RestoreBossAmmo(IEnumerable<CodeInstruction> instructions)
    {
        var bossAmmoConstructor = AccessTools.Constructor(typeof(BossAmmo), [typeof(int)]);
        return new CodeMatcher(instructions)
            .MatchStartForward([
                OpCodes.Ldc_I4_4,
                new CodeMatch(OpCodes.Newobj, bossAmmoConstructor)
            ])
            .SetOpcodeAndAdvance(OpCodes.Ldc_I4_5)
            .MatchStartForward(OpCodes.Ret)
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldarg_0),
                Transpilers.EmitDelegate(RestoreGunDamage)
            ])
            .InstructionEnumeration();
    }

    private static void RestoreGunDamage(ReaperBehaviour reaperBehaviour)
    {
        if (reaperBehaviour.zombie.obj.customObj is ReaperBossObject reaperBossObject)
        {
            reaperBossObject.gunHandling.databaseGun.dmg = 4f;
        }
    }
    
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ReaperBehaviour.TryAttack))]
    public static IEnumerable<CodeInstruction> RestoreAttackCooldown(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            // These are load-bearing method calls, apparently:
            // If you remove either `.Advance()` or `.ThrowIfNotMatchForward()`,
            // the transpiler will run into an error and fail to patch.
            // Why? I have no clue!
            .Advance(1)
            .ThrowIfNotMatchForward("I can't believe I have to do this..", [
                OpCodes.Ldarg_0,
                OpCodes.Ldloc_2,
                OpCodes.Ldc_R4,
                OpCodes.Ldc_R4,
            ])
            // Usually, I'd attempt to patch out the branch,
            // but with the above issue existing,
            // I really can't be bothered to tinker with this some more;
            // I've spent far too long debugging this specific patch
            .Repeat(m => m
                .MatchStartForward([
                    OpCodes.Ldarg_0,
                    OpCodes.Ldloc_2,
                    OpCodes.Ldc_R4,
                    OpCodes.Ldc_R4,
                ])
                .Advance(2)
                .SetOperandAndAdvance(0.1f)
                .SetOperandAndAdvance(0.2f))
            .InstructionEnumeration();
    }
}

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AetharNet.Mods.ZumbiBlocks2.RevertReaperNerf;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class RevertReaperNerf : BaseUnityPlugin
{
    public const string PluginGUID = "AetharNet.Mods.ZumbiBlocks2.RevertReaperNerf";
    public const string PluginAuthor = "wowi";
    public const string PluginName = "RevertReaperNerf";
    public const string PluginVersion = "0.1.1";

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Harmony.CreateAndPatchAll(typeof(RevertReaperNerf).Assembly, PluginGUID);
    }
}

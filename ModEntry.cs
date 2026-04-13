using MegaCrit.Sts2.Core.Modding;
using HarmonyLib;
using AlwaysClone.Patches;

[ModInitializer("Initialize")] 
public class ModEntry { 
    const string HarmonyId = "AlwaysClone.patch";

    public static void Initialize() { 
        var harmony = new Harmony(HarmonyId);

        try {
            ModLog.Info("Initializing and applying Harmony patches");
            var patched = AlwaysClonePatches.Register(harmony);
            ModLog.Info($"Harmony patches applied via explicit registration. count={patched}");
        } catch (Exception ex) {
            ModLog.Info($"Initialization failed: {ex}");
            TryRollbackAllPatches();

            throw new Exception("AlwaysClone failed to initialize. See log for details.", ex);
        }
    } 

    static void TryRollbackAllPatches() {
        try {
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            ModLog.Info("Rolled back Harmony patches");
        } catch (Exception) {
        }
    }
}
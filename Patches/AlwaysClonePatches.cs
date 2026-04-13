using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace AlwaysClone.Patches;

internal static class AlwaysClonePatches {
    static readonly string[] PreferredAncientMarkers = { "pael" };
    static readonly string[] PreferredOptionMarkers = { "paels_growth", "paelsgrowth" };

    public static int Register(Harmony harmony) {
        var patched = 0;
        patched += PatchPostfix(harmony, typeof(ActModel), "PullAncient", nameof(PullAncient_Postfix));
        patched += PatchPostfix(harmony, typeof(ActModel), "get_Ancient", nameof(ActModel_GetAncient_Postfix));
        patched += PatchPostfix(harmony, typeof(Pael), "GenerateInitialOptions", nameof(Pael_GenerateInitialOptions_Postfix));
        return patched;
    }

    static int PatchPostfix(Harmony harmony, Type targetType, string targetName, string patchMethodName, params Type[]? args) {
        var target = AccessTools.Method(targetType, targetName, args);
        var patchMethod = AccessTools.Method(typeof(AlwaysClonePatches), patchMethodName);

        if (target == null || patchMethod == null) {
            ModLog.Info($"Patch registration skipped: {targetType.FullName}.{targetName}");
            return 0;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(patchMethod));
        return 1;
    }

    static void PullAncient_Postfix(ActModel __instance, ref EventModel __result) {
        if (!IsAct2(__instance)) {
            return;
        }

        var preferred = FindPreferredAncient(__instance);
        if (preferred != null) {
            __result = preferred;
        }
    }

    static void ActModel_GetAncient_Postfix(ActModel __instance, ref AncientEventModel __result) {
        if (!IsAct2(__instance)) {
            return;
        }

        var preferred = FindPreferredAncient(__instance);
        if (preferred != null) {
            __result = preferred;
        }
    }

    static void Pael_GenerateInitialOptions_Postfix(Pael __instance, ref IReadOnlyList<EventOption> __result) {
        __result = MovePreferredOptionToFirstOrReplaceFirst(__result, __instance.AllPossibleOptions);
    }

    static AncientEventModel? FindPreferredAncient(ActModel actModel) {
        var ancients = actModel.AllAncients ?? Enumerable.Empty<AncientEventModel>();
        return ancients.FirstOrDefault(IsPreferredAncient);
    }

    static bool IsPreferredAncient(AncientEventModel model) {
        var typeName = model.GetType().Name;
        var fullName = model.GetType().FullName;
        var display = model.ToString();

        foreach (var marker in PreferredAncientMarkers) {
            if (ContainsIgnoreCase(typeName, marker)
                || ContainsIgnoreCase(fullName, marker)
                || ContainsIgnoreCase(display, marker)) {
                return true;
            }
        }

        return false;
    }

    static bool IsAct2(ActModel actModel) {
        var typeName = actModel.GetType().Name;
        var fullName = actModel.GetType().FullName;

        return ContainsIgnoreCase(typeName, "Hive")
               || ContainsIgnoreCase(fullName, "Hive")
               || ContainsIgnoreCase(typeName, "Act2")
               || ContainsIgnoreCase(fullName, "Act2");
    }

    static IReadOnlyList<EventOption> MovePreferredOptionToFirstOrReplaceFirst(
        IReadOnlyList<EventOption>? options,
        IEnumerable<EventOption>? allPossibleOptions) {
        if (options == null || options.Count == 0) {
            return options ?? Array.Empty<EventOption>();
        }

        var current = options.ToList();
        var preferredIndex = current.FindIndex(IsPreferredOption);

        if (preferredIndex == 0) {
            return options;
        }

        if (preferredIndex > 0) {
            var preferred = current[preferredIndex];
            current.RemoveAt(preferredIndex);
            current.Insert(0, preferred);
            return current;
        }

        var fallback = allPossibleOptions?.FirstOrDefault(IsPreferredOption);
        if (fallback != null) {
            current[0] = fallback;
            return current;
        }

        return options;
    }

    static bool IsPreferredOption(EventOption option) {
        if (option == null) {
            return false;
        }

        return ContainsPreferredOption(option.TextKey)
               || ContainsPreferredOption(option.Title?.ToString())
               || ContainsPreferredOption(option.Description?.ToString())
               || ContainsPreferredOption(option.Relic?.GetType().FullName)
               || ContainsPreferredOption(option.Relic?.ToString());
    }

    static bool ContainsPreferredOption(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return false;
        }

        foreach (var marker in PreferredOptionMarkers) {
            if (value.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
        }

        return false;
    }

    static bool ContainsIgnoreCase(string? value, string keyword) {
        return !string.IsNullOrWhiteSpace(value)
               && value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

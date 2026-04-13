using MegaCrit.Sts2.Core.Logging;

internal static class ModLog {
    const string Prefix = "[AlwaysClone]: ";

    public static void Info(string message) {
        Log.Info($"{Prefix}{message ?? string.Empty}");
    }
}

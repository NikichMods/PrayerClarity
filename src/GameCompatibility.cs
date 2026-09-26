using System;
using System.Reflection;

namespace PrayerClarity
{
    internal sealed class GameCompatibilityStatus
    {
        internal readonly Guid GameMvid;
        internal readonly bool Verified;

        internal GameCompatibilityStatus(Guid gameMvid, bool verified)
        {
            GameMvid = gameMvid;
            Verified = verified;
        }

        internal string Mode
        {
            get { return Verified ? "verified" : "unverified"; }
        }
    }

    internal static class GameCompatibility
    {
        internal const string VerifiedTarget = "Graveyard Keeper 1.407";
        private static readonly Guid VerifiedGameMvid =
            new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");

        internal static GameCompatibilityStatus Inspect(Assembly gameAssembly)
        {
            if (gameAssembly == null) throw new ArgumentNullException(nameof(gameAssembly));
            Guid actual = gameAssembly.ManifestModule.ModuleVersionId;
            return new GameCompatibilityStatus(actual, actual == VerifiedGameMvid);
        }
    }
}

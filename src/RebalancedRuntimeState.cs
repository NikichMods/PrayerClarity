namespace PrayerClarity
{
    internal static class RebalancedRuntimeState
    {
        private enum ProjectionState
        {
            Pending,
            Ready,
            Disabled
        }

        private static ProjectionState _state = ProjectionState.Pending;

        internal static bool IsReady
        {
            get { return _state == ProjectionState.Ready; }
        }

        internal static void MarkPending()
        {
            _state = ProjectionState.Pending;
        }

        internal static void MarkReady()
        {
            _state = ProjectionState.Ready;
        }

        internal static void Disable()
        {
            _state = ProjectionState.Disabled;
        }
    }
}

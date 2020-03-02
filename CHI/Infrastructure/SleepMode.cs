using System.Runtime.InteropServices;

namespace CHI.Infrastructure
{
    public static class SleepMode
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(ExecutionState esFlags);

        private enum ExecutionState : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        public static void Deny()
        {
            SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_CONTINUOUS);
        }
        public static void Allow()
        {
            SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
        }
    }
}

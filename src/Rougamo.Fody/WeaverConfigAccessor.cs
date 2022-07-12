using Fody;
using System;
using System.Linq;

namespace Rougamo.Fody
{
    public static class WeaverConfigAccessor
    {
        public static bool ConfigEnabled(this BaseModuleWeaver weaver)
        {
            var enabled = weaver.GetConfigValue("enabled", "true");
            return "true".Equals(enabled, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ConfigRecordingIteratorReturnValue(this BaseModuleWeaver weaver)
        {
#if DEBUG
            return true;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            var recording = weaver.GetConfigValue("enumerable-returns", "false");
            return "true".Equals(recording, StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static bool ConfigReverseCallEnding(this BaseModuleWeaver weaver)
        {
            var reverseCall = weaver.GetConfigValue("reverse-call-ending", "true");
            return "true".Equals(reverseCall, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetConfigValue(this BaseModuleWeaver weaver, string configKey, string defaultValue)
        {
            if (weaver.Config == null) return defaultValue;

            var configAttribute = weaver.Config.Attributes(configKey).SingleOrDefault();
            return configAttribute == null ? defaultValue : configAttribute.Value;
        }
    }
}

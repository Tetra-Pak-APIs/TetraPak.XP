using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Extends the <see cref="RuntimeEnvironment"/> enum.
    /// </summary>
    public static class RuntimeEnvironmentExtensions
    {
        /// <summary>
        ///   Attempts parsing a textual representation of a <see cref="RuntimeEnvironment"/> value.
        /// </summary>
        /// <param name="self">
        ///   The textual value to be parsed.
        /// </param>
        /// <param name="runtimeEnvironment">
        ///   Passes back the (successfully) parsed <see cref="RuntimeEnvironment"/> value.
        /// </param>
        /// <returns>
        ///   The parsed <see cref="RuntimeEnvironment"/> value
        /// </returns>
        public static bool TryParseAsRuntimeEnvironment(this string? self, out RuntimeEnvironment runtimeEnvironment)
        {
            self = self?.Trim();
            if (string.IsNullOrWhiteSpace(self)) throw new ArgumentNullException(nameof(self));
            if (self!.Length != 3 && self.Length != 4)
                return Enum.TryParse(self, out runtimeEnvironment);

            if (self.Equals("DEV", StringComparison.OrdinalIgnoreCase)||
                self.Equals("DEVL", StringComparison.OrdinalIgnoreCase))
            {
                runtimeEnvironment = RuntimeEnvironment.Development;
                return true;
            }

            if (self.Equals("MIG", StringComparison.OrdinalIgnoreCase) ||
                self.Equals("MIGR", StringComparison.OrdinalIgnoreCase))
            {
                runtimeEnvironment = RuntimeEnvironment.Migration;
                return true;
            }

            if (self.Equals("PRD", StringComparison.OrdinalIgnoreCase) ||
                self.Equals("PROD", StringComparison.OrdinalIgnoreCase))
            {
                runtimeEnvironment = RuntimeEnvironment.Production;
                return true;
            }

            if (self.Equals("TST", StringComparison.OrdinalIgnoreCase) ||
                self.Equals("TEST", StringComparison.OrdinalIgnoreCase))
            {
                runtimeEnvironment = RuntimeEnvironment.Test;
                return true;
            }

            runtimeEnvironment = default;
            return false;
        }
    }
}
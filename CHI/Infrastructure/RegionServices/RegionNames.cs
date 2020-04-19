using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Названия регионов
    /// </summary>
    public static class RegionNames
    {
        public static string MainRegion { get; } = nameof(MainRegion);
        public static string ProgressBarRegion { get; } = nameof(ProgressBarRegion);
        public static string DialogRegion { get; } = nameof(DialogRegion);
        public static string AttachedPatientsSettingsRegion { get; } = nameof(AttachedPatientsSettingsRegion);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Helpers
{
    public class TimeZoneHelper
    {
        private static readonly IDictionary<string, string> IanaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> WindowsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> RailsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, IList<string>> InverseRailsMap = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

        static TimeZoneHelper()
        {
            DataLoader.Populate(IanaMap, WindowsMap, RailsMap, InverseRailsMap);

            KnownIanaTimeZoneNames = IanaMap.Keys;
            KnownWindowsTimeZoneIds = WindowsMap.Keys.Select(x => x.Split('|')[1]).Distinct().ToArray();
            KnownRailsTimeZoneNames = RailsMap.Keys;
        }

        /// <summary>
        /// Gets a collection of all IANA time zone names known to this library.
        /// </summary>
        public static ICollection<string> KnownIanaTimeZoneNames { get; }

        /// <summary>
        /// Gets a collection of all Windows time zone IDs known to this library.
        /// </summary>
        public static ICollection<string> KnownWindowsTimeZoneIds { get; }

        /// <summary>
        /// Gets a collection of all Rails time zone names known to this library.
        /// </summary>
        public static ICollection<string> KnownRailsTimeZoneNames { get; }

        /// <summary>
        /// Converts an IANA time zone name to the equivalent Windows time zone ID.
        /// </summary>
        /// <param name="ianaTimeZoneName">The IANA time zone name to convert.</param>
        /// <returns>A Windows time zone ID.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent Windows zone.</exception>
        public static string IanaToWindows(string ianaTimeZoneName)
        {
            if (IanaMap.TryGetValue(ianaTimeZoneName, out var windowsTimeZoneId))
                return windowsTimeZoneId;

            throw new InvalidTimeZoneException($"\"{ianaTimeZoneName}\" was not recognized as a valid IANA time zone name, or has no equivalant Windows time zone.");
        }

        /// <summary>
        /// Converts a Windows time zone ID to an equivalent IANA time zone name.
        /// </summary>
        /// <param name="windowsTimeZoneId">The Windows time zone ID to convert.</param>
        /// <param name="territoryCode">
        /// An optional two-letter ISO Country/Region code, used to get a a specific mapping.
        /// Defaults to "001" if not specified, which means to get the "golden zone" - the one that is most prevalent.
        /// </param>
        /// <returns>An IANA time zone name.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent IANA zone.</exception>
        public static string WindowsToIana(string windowsTimeZoneId, string territoryCode = "001")
        {
            var key = $"{territoryCode}|{windowsTimeZoneId}";
            if (WindowsMap.TryGetValue(key, out var ianaTimeZoneName))
                return ianaTimeZoneName;

            if (territoryCode != "001")
            {
                // use the golden zone when not found with a particular region
                return WindowsToIana(windowsTimeZoneId);
            }

            throw new InvalidTimeZoneException($"\"{windowsTimeZoneId}\" was not recognized as a valid Windows time zone ID.");
        }
        
        /// <summary>
        /// Retrieves a <see cref="TimeZoneInfo"/> object given a valid Windows or IANA time zone idenfifier,
        /// regardless of which platform the application is running on.
        /// </summary>
        /// <param name="windowsOrIanaTimeZoneId">A valid Windows or IANA time zone identifier.</param>
        /// <returns>A <see cref="TimeZoneInfo"/> object.</returns>
        public static TimeZoneInfo GetTimeZoneInfo(string windowsOrIanaTimeZoneId)
        {
            try
            {
                // Try a direct approach first
                return TimeZoneInfo.FindSystemTimeZoneById(windowsOrIanaTimeZoneId);
            }
            catch(Exception ex)
            {
                ex.ThrowIfNecessary();
                // We have to convert to the opposite platform
                var tzid = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? IanaToWindows(windowsOrIanaTimeZoneId)
                    : WindowsToIana(windowsOrIanaTimeZoneId);

                // Try with the converted ID
                return TimeZoneInfo.FindSystemTimeZoneById(tzid);
            }
        }

        /// <summary>
        /// Converts an IANA time zone name to one or more equivalent Rails time zone names.
        /// </summary>
        /// <param name="ianaTimeZoneName">The IANA time zone name to convert.</param>
        /// <returns>One or more equivalent Rails time zone names.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent Rails zone.</exception>
        public static IList<string> IanaToRails(string ianaTimeZoneName)
        {
            // try directly first
            if (InverseRailsMap.TryGetValue(ianaTimeZoneName, out var railsTimeZoneNames))
                return railsTimeZoneNames;

            // try again with the Windows golden zone
            try
            {
                var goldenZone = WindowsToIana(IanaToWindows(ianaTimeZoneName));
                if (InverseRailsMap.TryGetValue(goldenZone, out railsTimeZoneNames))
                    return railsTimeZoneNames;
            }
            catch (InvalidTimeZoneException) { }

            throw new InvalidTimeZoneException($"\"{ianaTimeZoneName}\" was not recognized as a valid IANA time zone name, or has no equivalant Rails time zone.");
        }

        /// <summary>
        /// Converts a Rails time zone name to an equivalent IANA time zone name.
        /// </summary>
        /// <param name="railsTimeZoneName">The Rails time zone name to convert.</param>
        /// <returns>An IANA time zone name.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent IANA zone.</exception>
        public static string RailsToIana(string railsTimeZoneName)
        {
            if (RailsMap.TryGetValue(railsTimeZoneName, out var ianaTimeZoneName))
                return ianaTimeZoneName;

            throw new InvalidTimeZoneException($"\"{railsTimeZoneName}\" was not recognized as a valid Rails time zone name.");
        }

        /// <summary>
        /// Converts a Rails time zone name to an equivalent Windows time zone ID.
        /// </summary>
        /// <param name="railsTimeZoneName">The Rails time zone name to convert.</param>
        /// <returns>A Windows time zone ID.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent Windows zone.</exception>
        public static string RailsToWindows(string railsTimeZoneName)
        {
            var ianaZoneName = RailsToIana(railsTimeZoneName);
            var windowsZoneId = IanaToWindows(ianaZoneName);
            return windowsZoneId;
        }

        /// <summary>
        /// Converts a Windows time zone ID to one ore more equivalent Rails time zone names.
        /// </summary>
        /// <param name="windowsTimeZoneId">The Windows time zone ID to convert.</param>
        /// <param name="territoryCode">
        /// An optional two-letter ISO Country/Region code, used to get a a specific mapping.
        /// Defaults to "001" if not specified, which means to get the "golden zone" - the one that is most prevalent.
        /// </param>
        /// <returns>One or more equivalent Rails time zone names.</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string was not recognized or has no equivalent Rails zone.</exception>
        public static IList<string> WindowsToRails(string windowsTimeZoneId, string territoryCode = "001")
        {
            var ianaTimeZoneName = WindowsToIana(windowsTimeZoneId, territoryCode);
            var railsTimeZoneNames = IanaToRails(ianaTimeZoneName);
            return railsTimeZoneNames;
        }

        internal static class DataLoader
        {
            public static void Populate(IDictionary<string, string> ianaMap, IDictionary<string, string> windowsMap, IDictionary<string, string> railsMap, IDictionary<string, IList<string>> inverseRailsMap)
            {
                var mapping = GetEmbeddedData("Schubert.Framework._Infrastructure.Helpers.TimeZoneMappings.Mapping.csv");
                var aliases = GetEmbeddedData("Schubert.Framework._Infrastructure.Helpers.TimeZoneMappings.Aliases.csv");
                var railsMapping = GetEmbeddedData("Schubert.Framework._Infrastructure.Helpers.TimeZoneMappings.RailsMapping.csv");

                var links = new Dictionary<string, string>();
                foreach (var link in aliases)
                {
                    var parts = link.Split(',');
                    var value = parts[0];
                    foreach (var key in parts[1].Split())
                        links.Add(key, value);
                }

                var similarIanaZones = new Dictionary<string, IList<string>>();
                foreach (var item in mapping)
                {
                    var parts = item.Split(',');
                    var windowsZone = parts[0];
                    var territory = parts[1];
                    var ianaZones = parts[2].Split();

                    // Create the Windows map entry
                    if (!links.TryGetValue(ianaZones[0], out var value))
                        value = ianaZones[0];

                    var key = $"{territory}|{windowsZone}";
                    windowsMap.Add(key, value);

                    // Create the IANA map entries
                    foreach (var ianaZone in ianaZones)
                    {
                        if (!ianaMap.ContainsKey(ianaZone))
                            ianaMap.Add(ianaZone, windowsZone);
                    }

                    if (ianaZones.Length > 1)
                    {
                        foreach (var ianaZone in ianaZones)
                            similarIanaZones.Add(ianaZone, ianaZones.Except(new[] { ianaZone }).ToArray());
                    }
                }

                // Expand the IANA map to include all links
                foreach (var link in links)
                {
                    if (ianaMap.ContainsKey(link.Key))
                        continue;

                    ianaMap.Add(link.Key, ianaMap[link.Value]);
                }

                foreach (var item in railsMapping)
                {
                    var parts = item.Split(',');
                    var railsZone = parts[0].Trim('"');
                    var ianaZone = parts[1].Trim('"');
                    railsMap.Add(railsZone, ianaZone);
                }

                foreach (var grouping in railsMap.GroupBy(x => x.Value, x => x.Key))
                {
                    inverseRailsMap.Add(grouping.Key, grouping.ToList());
                }

                // Expand the Inverse Rails map to include similar IANA zones
                foreach (var ianaZone in ianaMap.Keys)
                {
                    if (inverseRailsMap.ContainsKey(ianaZone) || links.ContainsKey(ianaZone))
                        continue;

                    if (similarIanaZones.TryGetValue(ianaZone, out var similarZones))
                    {
                        foreach (var otherZone in similarZones)
                        {
                            if (inverseRailsMap.TryGetValue(otherZone, out var railsZones))
                            {
                                inverseRailsMap.Add(ianaZone, railsZones);
                                break;
                            }
                        }
                    }
                }

                // Expand the Inverse Rails map to include links
                foreach (var link in links)
                {
                    if (inverseRailsMap.ContainsKey(link.Key))
                        continue;

                    if (inverseRailsMap.TryGetValue(link.Value, out var railsZone))
                        inverseRailsMap.Add(link.Key, railsZone);
                }


            }

            private static IEnumerable<string> GetEmbeddedData(string resourceName)
            {
                var assembly = typeof(DataLoader).GetTypeInfo().Assembly;
                using (var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new MissingManifestResourceException())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            yield return line;
                        }
                    }
                }
            }
        }
    }
}

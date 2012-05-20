﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Contains informations about the assembly.
    /// </summary>
    public static class Signature
    {
        /// <summary>
        /// Gets the name of the software.
        /// </summary>
        /// <value>The software name.</value>
        public static string Software { get; private set; }

        /// <summary>
        /// Gets the version number of the executing assembly.
        /// </summary>
        /// <value>The software version.</value>
        public static string Version { get; private set; }

        /// <summary>
        /// Gets the formatted version number of the executing assembly.
        /// </summary>
        /// <value>The formatted software version.</value>
        public static string VersionFormatted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a nightly build.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this is a nightly build; otherwise, <c>false</c>.
        /// </value>
        public static bool IsNightly
        {
            get
            {
                return
#if NIGHTLY
                    true
#else
                    false
#endif
                ;
            }
        }

        /// <summary>
        /// Gets the date and time when the executing assembly was compiled.
        /// </summary>
        /// <value>The compile time.</value>
        public static DateTime CompileTime { get; private set; }

        /// <summary>
        /// Gets the full path to the executing assembly.
        /// </summary>
        /// <value>The full path.</value>
        public static string FullPath { get; private set; }

        /// <summary>
        /// This number is used for various purposes where a non-random unique number is required.
        /// </summary>
        public static long MagicNumber
        {
            get
            {
                return 0xFEEDFACEC0FFEE;
            }
        }
        
        /// <summary>
        /// Initializes static members of the <see cref="Signature"/> class. 
        /// </summary>
        static Signature()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            Software         = "RS TV Show Tracker";
            Version          = ver.Major + "." + ver.Minor + (ver.Build != 0 ? "." + ver.Build : string.Empty);
            VersionFormatted = "v" + ver.Major + "." + ver.Minor + (ver.Build != 0 ? " build " + ver.Build : string.Empty) + (IsNightly ? " nightly" : string.Empty);
            CompileTime      = RetrieveLinkerTimestamp();
            try { FullPath   = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar; } catch (ArgumentException) { }
        }

        /// <summary>
        /// Gets the numbers. This is an easter egg. ;)
        /// </summary>
        /// <returns>
        /// The numbers.
        /// </returns>
        public static IEnumerable<int> GetNumbers()
        {
            for (var x = 1; x != 6; x++)
            {
                yield return (int)(60 + 4.25 * Math.Pow(x * x, 2) + 91.75 * x * x - 29.375 * x * Math.Pow(x, 2) - 0.22499999 * x * Math.Pow(x, 2) * Math.Pow(x, 2) - 122.4 * x);
            }
        }

        /// <summary>
        /// Retrieves the linker timestamp from the PE header embedded in the executable file.
        /// </summary>
        /// <returns>
        /// Compilation date.
        /// </returns>
        private static DateTime RetrieveLinkerTimestamp()
        {
            try
            {
                var pe = new byte[2048];

                using (var fs = new FileStream(Assembly.GetExecutingAssembly().Location, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(pe, 0, 2048);
                }

                return Extensions.GetUnixTimestamp(BitConverter.ToInt32(pe, BitConverter.ToInt32(pe, 60) + 8));
            }
            catch
            {
                return Utils.UnixEpoch;
            }
        }
    }
}

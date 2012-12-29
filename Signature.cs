﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

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
        /// Gets a value indicating whether a donation key has been entered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a donation key has been entered; otherwise, <c>false</c>.
        /// </value>
        public static bool IsActivated
        {
            get
            {
#if ACTIVATE_WITHOUT_DONATION
                return true;
#else
                // TODO check for activation.
                return true;
#endif
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
        /// Gets the Aperture Science Turret Testing Facility Access Codes.
        /// </summary>
        private static BigInteger[] Moduluses =
            {
                new BigInteger(15246487771235781107)*(17971768237329038849),
                new BigInteger(16455720859197541667)*(16693946381056259363),
                new BigInteger(14486207375898700283)*(16083913764276947789),
                new BigInteger(14283561927021492269)*(15127792701305930963),
                new BigInteger(14370260276245322459)*(16496639490075584219),
                new BigInteger(15914860067061659267)*(16579739426608955183),
                new BigInteger(14143463530261769267)*(14816180372324778737),
                new BigInteger(14726713035460975247)*(14922266034305327459),
                new BigInteger(15973096875095683133)*(17437565442450587429),
                new BigInteger(15028130840778560783)*(17277204033912866243),
            };

        /// <summary>
        /// Initializes static members of the <see cref="Signature"/> class. 
        /// </summary>
        static Signature()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            Software = "RS TV Show Tracker";
            Version = ver.Major + "." + ver.Minor + (ver.Build != 0 ? "." + ver.Build : string.Empty);
            VersionFormatted = "v" + ver.Major + "." + ver.Minor + (ver.Build != 0 ? " build " + ver.Build : string.Empty) + (IsNightly ? " nightly" : string.Empty);
            CompileTime = RetrieveLinkerTimestamp();
            try { FullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar; } catch (ArgumentException) { }
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

        /// <summary>
        /// Determines whether the specified activation data is valid.
        /// </summary>
        /// <param name="user">The email address of the user.</param>
        /// <param name="key">The corresponding donation key.</param>
        /// <returns>
        ///   <c>true</c> if the activation was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool Activate(string user, string key)
        {
            var a = Encoding.UTF8.GetBytes(user.ToLower().Trim());
            var b = new BigInteger(new HMACSHA512(MD5.Create().ComputeHash(a)).ComputeHash(a).Truncate(16).Reverse().ToArray());
            var c = key.Trim().Replace("-", string.Empty).Substring(2).Reverse().ToList();
            var d = (c.Aggregate(int.Parse(key.TrimStart().Substring(0, 2), NumberStyles.HexNumber), (i, x) => i - x) - 5 * 45) & byte.MaxValue;
            var e = Enumerable.Range('0', '9' - '0' + 1).Concat(Enumerable.Range('a', 'z' - 'a' + 1)).Concat(Enumerable.Range('A', 'Z' - 'A' + 1)).Select(x => (char)x).ToList();
            var f = c.Aggregate(BigInteger.Zero, (g, i, x) => BigInteger.Add(g, BigInteger.Multiply(e.IndexOf(x), BigInteger.Pow(e.Count, i))));
            return Moduluses.Any(m => BigInteger.Subtract(b, BigInteger.ModPow(f, 0x010001, m)).IsZero) && 0 == d;
        }
    }
}

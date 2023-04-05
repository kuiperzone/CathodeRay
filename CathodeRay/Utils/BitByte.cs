// -----------------------------------------------------------------------------
// PROJECT   : CathodeRay
// COPYRIGHT : Andy Thomas (C) 2023
// LICENSE   : LGPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/CathodeRay
//
// This file is part of CathodeRay.
//
// CathodeRay is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
// Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// CathodeRay is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for
// more details.
//
// You should have received a copy of the GNU Lesser General Public License along with CathodeRay.
// If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

namespace KuiperZone.CathodeRay.Utils
{
    /// <summary>
    /// A static utility class with miscellaneous methods relating to computer size (i.e. KB) values.
    /// </summary>
    public static class BitByte
    {
        /// <summary>
        /// Utility which converts a byte count to a friendly string.
        /// I.e. 2050 gives "2.0 KB".
        /// </summary>
        public static string ToByteString(long count, bool verbose = false)
        {
            return ToBitByteString(count, false, verbose);
        }

        /// <summary>
        /// Utility which converts a bit count to a friendly string.
        /// I.e. 2050 gives "2.0 Kb".
        /// </summary>
        public static string ToBitString(long count, bool verbose = false)
        {
            return ToBitByteString(count, true, verbose);
        }

        private static string ToBitByteString(long count, bool bits, bool verbose)
        {
            if (count > 0)
            {
                long sz = count;
                double dz = count;

                int idx = 0;
                var mags = new string[] { " bytes", " KB", " MB", " GB", " TB", " PB", " EB" };

                if (bits)
                {
                    mags = new string[] { " bits", " Kb", " Mb", " Gb", " Tb", " Pb", " Eb" };
                }

                while (sz >= 1024 && idx < mags.Length - 1)
                {
                    ++idx;
                    sz /= 1024;
                    dz /= 1024;
                }

                string rslt;

                if (idx > 0)
                {
                    // KB or greater
                    rslt = dz.ToString("0.0") + mags[idx];

                    if (verbose)
                    {
                        rslt += " (" + count.ToString() + ")";
                    }
                }
                else
                {
                    // Small byte number
                    rslt = sz.ToString() + mags[idx];
                }

                return rslt;
            }

            return count.ToString();
        }

    }
}

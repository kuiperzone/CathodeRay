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

using System.Diagnostics;
using System.Text;

namespace KuiperZone.CathodeRay.Internal
{
    /// <summary>
    /// A static utility class with miscellaneous methods files.
    /// </summary>
    internal static class FileUtils
    {
        /// <summary>
        /// Determines the probable encoding of the stream by looking for a byte order mark (BOM) in
        /// the first instance, and then trying a limiting number of possible encodings. For streams
        /// containing only ASCII, the result will be Encoding.UTF8. If the stream contains binary
        /// (non-text) data or the encoding could not be determined, the result is null. Possible
        /// return values are restricted to: null, UTF7, UTF8, Unicode (UTF16-LE), BigEndianUnicode,
        /// UTF32 and an encoder for ISO-8859-1 (Latin). On return, the file position will be left
        /// at the stream start. The stream must be seekable, otherwise throws NotSupportedException.
        /// </summary>
        public static Encoding? GetEncoding(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            if (stream.Length == 0)
            {
                return null;
            }

            try
            {
                var enc = GetFileBom(stream);
                if (enc != null) return enc;

                // The most (86.3 %) used encoding.
                // This will include ASCII.
                stream.Seek(0, SeekOrigin.Begin);
                enc = Encoding.UTF8;
                if (TryEncoding(stream, enc)) return enc;

                // Look for a zero which indicate binary file
                int rb = 0;
                int count = 0;
                stream.Seek(0, SeekOrigin.Begin);

                while (rb > -1 && count++ < 32 * 1024)
                {
                    rb = stream.ReadByte();

                    // Probable binary
                    if (rb == 0) return null;
                }

                try
                {
                    // Supported on .NET Core
                    // ISO-8859-1/1252 is 6.7% used encoding.
                    stream.Seek(0, SeekOrigin.Begin);
                    enc = Encoding.GetEncoding("ISO-8859-1");
                    if (TryEncoding(stream, enc)) return enc;
                }
                catch (ArgumentException)
                {
                    // Not available
                    // Strangely, for example, 1252 is not supported on .NET Core.
                }

                // UTF-16 LE
                stream.Seek(0, SeekOrigin.Begin);
                enc = Encoding.Unicode;
                if (TryEncoding(stream, enc)) return enc;

                // Unknown / binary
                return null;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// The file must exist. See: <see cref="GetEncoding(Stream)"/>
        /// </summary>
        public static Encoding? GetEncoding(string filename)
        {
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return GetEncoding(file);

            }
        }

        /// <summary>
        /// Gets the uppercase of enc.WebName. If enc is null, returns "Binary".
        /// </summary>
        public static string GetEncodingName(Encoding? enc)
        {
            if (enc != null)
            {
                return enc.WebName.ToUpperInvariant();
            }

            return "Binary";
        }

        /// <summary>
        /// Opens the file or process.
        /// </summary>
        public static bool OpenRun(string filename, bool shell, string? args = null, bool wait = false)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = filename;
                proc.StartInfo.UseShellExecute = shell;
                proc.StartInfo.Arguments = args;


                if (proc.Start())
                {
                    if (wait)
                    {
                        proc.WaitForExit();
                    }

                    return true;
                }

                return false;
            }
        }

        private static Encoding? GetFileBom(Stream stream)
        {
            // Read the BOM
            // https://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding
            var bom = new byte[4];
            stream.Read(bom, 0, 4);

            // Analyze the BOM
            // if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; // UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; // UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;

            return null;
        }

        private static bool TryEncoding(Stream stream, Encoding enc, int maxLines = 250)
        {
            // https://stackoverflow.com/questions/90838/how-can-i-detect-the-encoding-codepage-of-a-text-file
            var verifier = Encoding.GetEncoding(enc.CodePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());

            try
            {
                using (var reader = new StreamReader(stream, verifier, false, 1024, true))
                {
                    int count = 0;
                    while (count++ < maxLines && reader.ReadLine() != null) ;

                    return true;
                }
            }
            catch (DecoderFallbackException)
            {
            }

            return false;
        }

    }
}

//--------------------------------------------------------------------------------------------------
// PROJECT      : TRAL
// COPYRIGHT    : Andy Thomas
// LICENSE      : Proprietary - All rights reserved
//--------------------------------------------------------------------------------------------------

using System.Text;
using KuiperZone.CathodeRay.Utils;

namespace KuiperZone.CathodeRay.Internal
{
    /// <summary>
    /// Similar to the DriveInfo class, but comprises readonly information initialised at
    /// construction. Size values are -1 if undefined.
    /// </summary>
    public class DriveQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriveQuery"/> class.
        /// </summary>
        public DriveQuery(DriveInfo drive)
        {
            try
            {
                Name = !string.IsNullOrEmpty(drive.Name) ? drive.Name : Name;

                if (drive.IsReady)
                {

                    VolumeLabel = !string.IsNullOrEmpty(drive.VolumeLabel) ? drive.VolumeLabel : VolumeLabel;

                    AvailableFreeSpace = drive.AvailableFreeSpace;
                    TotalFreeSpace = drive.TotalFreeSpace;
                    TotalSize = drive.TotalSize;

                    DriveType = drive.DriveType;
                    RootDirectory = drive.RootDirectory;

                    DriveFormat = !string.IsNullOrEmpty(drive.DriveFormat) ? drive.DriveFormat : DriveFormat;
                    IsValid = true;

                    if (TotalSize > 0)
                    {
                        long used = TotalSize - TotalFreeSpace;
                        PercentUsed = 100.0 * used / TotalSize;
                    }
                    else
                    {
                        PercentUsed = 100;
                    }
                }
            }
            catch
            {
                // Log?
            }
        }

        /// <summary>
        /// Indicates the amount of available free space on a drive, in bytes, taking into
        /// account disk quotas.
        /// </summary>
        public long AvailableFreeSpace { get; } = -1;

        /// <summary>
        /// Gets the name of the file system, such as NTFS or FAT32.
        /// </summary>
        public string DriveFormat { get; } = "unknown";

        /// <summary>
        /// Gets the drive type, such as CD-ROM, removable, network, or fixed.
        /// </summary>
        public DriveType DriveType { get; } = DriveType.Unknown;

        /// <summary>
        /// Gets the name of a drive, such as "C:\". The value is an empty string if undefined.
        /// </summary>
        public string Name { get; } = "";

        /// <summary>
        /// Gets whether the drive information was queried successfully and has valid results.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the root directory of a drive. Null if undefined.
        /// </summary>
        public DirectoryInfo? RootDirectory { get; }

        /// <summary>
        /// This property indicates the total amount of free space available on the drive,
        /// not just what is available to the current user.
        /// </summary>
        public long TotalFreeSpace { get; } = -1;

        /// <summary>
        /// Gets the total size of storage space on a drive, in bytes.
        /// </summary>
        public long TotalSize { get; } = -1;

        /// <summary>
        /// Total used as a percentage.
        /// </summary>
        public double PercentUsed { get; } = -1;

        /// <summary>
        /// Gets the volume label of a drive. The value is an empty string if undefined.
        /// </summary>
        public string VolumeLabel { get; } = "";

        /// <summary>
        /// Returns a list of DriveQuery instances matching the DriveType value.
        /// </summary>
        public static IList<DriveQuery> GetDrives(DriveType type)
        {
            var result = new List<DriveQuery>();

            try
            {
                foreach (var info in DriveInfo.GetDrives())
                {
                    var drive = new DriveQuery(info);

                    if (drive.IsValid && drive.DriveType == type)
                    {
                        result.Add(drive);
                    }
                }
            }
            catch
            {
                // Log
            }

            return result;
        }

        /// <summary>
        /// Examines drives matching the DriveType value and returns first drive instance
        /// with a usage exceeding the threshold percentage. If no drives exceed the threshold,
        /// the result is null.
        /// </summary>
        public static DriveQuery? UsageExceeds(DriveType type, double thresholdPerc)
        {
            if (thresholdPerc > 0)
            {
                foreach (var drive in GetDrives(type))
                {
                    if (drive.PercentUsed > thresholdPerc)
                    {
                        return drive;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns summary information.
        /// </summary>
        public override string ToString()
        {
            if (IsValid)
            {
                bool hasSep = false;
                bool hasVol = false;
                var sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(VolumeLabel))
                {
                    hasVol = true;
                    sb.Append(VolumeLabel);
                }

                if (!string.Equals(Name, VolumeLabel))
                {
                    if (hasVol)
                    {
                        hasSep = true;
                        sb.Append(" : ");
                        sb.Append(Name);
                    }
                    else
                    {
                        sb.Append(Name);
                    }
                }

                if (!hasSep)
                {
                    sb.Append(" :");
                }

                sb.Append(" [");
                sb.Append(DriveFormat);
                sb.Append("]");

                if (TotalFreeSpace >= 0 && TotalSize > 0)
                {
                    sb.Append(" ");

                    sb.Append(PercentUsed.ToString("0.#"));
                    sb.Append("% used: ");
                    sb.Append(BitByte.ToByteString(TotalSize - TotalFreeSpace));
                    sb.Append(" of ");
                    sb.Append(BitByte.ToByteString(TotalSize));
                }

                return sb.ToString();
            }

            return Name + " : (unavailable)";
        }

    }
}
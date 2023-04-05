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

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Option flags for use with <see cref="FileBrowserPage.Styling"/>.
    /// </summary>
    [Flags]
    public enum BrowserStyles
    {
        /// <summary>
        /// Shows nothing.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Show regular files.
        /// </summary>
        ShowFiles = 0x00000001,

        /// <summary>
        /// Show all files, including hidden and system files.
        /// </summary>
        ShowAllFiles = ShowFiles | 0x00000002,

        /// <summary>
        /// Show subdirectories. Note that specifying this flag without an accompanying
        /// <see cref="SubNavigation"/> flag will merely show the directories, but not allow
        /// the user to select them.
        /// </summary>
        ShowDirectories = 0x00000004,

        /// <summary>
        /// Show file details such as modification time and file size.
        /// </summary>
        DetailedView = 0x00000008,

        /// <summary>
        /// Shows the directory name in the content section. The directory name is the directory
        /// leaf name plus any subdirectory navigation part. It is the fully qualified path if
        /// <see cref="RootNavigation"/> is specified. It would not be typical to specify this flag
        /// with <see cref="ShowDirectoryAsPageTitle"/>, as the directory name will be shown twice.
        /// </summary>
        ShowDirectoryName = 0x00000010,

        /// <summary>
        /// If specified, the directory name is always fully qualified.
        /// </summary>
        ShowFullyQualifiedName = 0x00000020,

        /// <summary>
        /// Displays the directory name for the page title. It would not be typical to specify
        /// this flag with<see cref="ShowDirectoryName"/>, as the directory name will be
        /// shown twice.
        /// </summary>
        ShowDirectoryAsPageTitle = 0x00000040,

        /// <summary>
        /// Shows sub-section headers for files and subdirectories.
        /// </summary>
        ShowSectionHeaders = 0x00000080,

        /// <summary>
        /// Hide file extensions.
        /// </summary>
        HideExtension = 0x00000100,

        /// <summary>
        /// Items are sorted on name by default. This option reverse sorts on execution.
        /// </summary>
        Reversed = 0x00000200,

        /// <summary>
        /// Show dates and times as UTC.
        /// </summary>
        UtcTimes = 0x00000400,

        /// <summary>
        /// Show an option allowing the user to quit the browser, short-cutting multiple page back
        /// navigations. This flag should normally be specified.
        /// </summary>
        ShowQuitOption = 0x00000800,

        /// <summary>
        /// Provides the user with common options, such as the ability to switch the detailed view
        /// and change the sort order.
        /// </summary>
        ShowViewOptions = 0x00001000,

        /// <summary>
        /// Allow user to rename and delete individual files. Does nothing unless
        /// <see cref="ShowFiles"/> is specified.
        /// </summary>
        RenameDeleteFiles = 0x00002000,

        /// <summary>
        /// Allow user to delete multiple/all files in a directory. This option, by itself, does not
        /// allow file renaming or the deletion of individual file. Does nothing unless <see
        /// cref="ShowFiles"/> is specified.
        /// </summary>
        DeleteMultiFiles = 0x00004000,

        /// <summary>
        /// Allow user to rename and delete the current directory. Does nothing unless
        /// <see cref="ShowDirectories"/> is specified.
        /// </summary>
        RenameDeleteDirectory = 0x00008000,

        /// <summary>
        /// Allow user to rename and delete the current directory, individual files and all files.
        /// </summary>
        FullRenameDelete = RenameDeleteDirectory | RenameDeleteFiles | DeleteMultiFiles,

        /// <summary>
        /// Allow the user to view the file content within the page shell using a built in viewer.
        /// </summary>
        FileViewContent = 0x00010000,

        /// <summary>
        /// Allow the user to open (launch) the file in the shell on the local machine.
        /// </summary>
        FileOpenRun = 0x00020000,

        /// <summary>
        /// When the file is selected, if <see cref="FileViewContent"/> is specified, the user
        /// navigates directly to the file content view rather than a file summary page. If <see
        /// cref="FileViewContent"/> is not specified, but <see cref="FileOpenRun"/> is specified,
        /// the file is opened (launched) on the local machine. <see cref="DirectToFile"/> does
        /// nothing unless used with either <see cref="FileViewContent"/> or <see cref="FileOpenRun"/>.
        /// Note the use of this may deny certain options, such as <see cref="RenameDeleteFiles"/>.
        /// </summary>
        DirectToFile = 0x00040000,

        /// <summary>
        /// If <see cref="ShowFiles"/> is specified, then selecting a file item will cause the
        /// browser to return. The caller can retrieve the selected item value via the
        /// <see cref="FileBrowserPage.SelectedItem"/> property. If <see cref="ShowFiles"/> is
        /// not specified, but <see cref="ShowDirectories"/> is specified, then an option is
        /// shown to allow the user to select the current directory. This allows the
        /// the <see cref="FileBrowserPage"/> to serve a "browse for directory" function. This flag
        /// takes precedence over <see cref="DirectToFile"/> and note its use may deny certain
        /// other options, such as <see cref="FileViewContent"/> and <see cref="RenameDeleteFiles"/>.
        /// </summary>
        SelectItemToReturn = 0x00080000,

        /// <summary>
        /// Requests confirmation if the user selects an existing item. It is appropriate where
        /// the selected item may be overwritten or deleted by the caller. Specifying this
        /// flag includes <see cref="SelectItemToReturn"/>.
        /// </summary>
        ConfirmBeforeSelectReturn = SelectItemToReturn | 0x00100000,

        /// <summary>
        /// Allows the user to enter a new filename that does not already exist. This will not
        /// create the file itself, but will set <see cref="FileBrowserPage.SelectedItem"/> giving
        /// the fully qualified new file path thus allowing the caller to create the file. Does
        /// nothing unless both <see cref="ShowFiles"/> and <see cref="SelectItemToReturn"/> are
        /// specified.
        /// </summary>
        AllowSelectNewFile = 0x00200000,

        /// <summary>
        /// Allows the user to navigate sub-directories of the starting directory.
        /// Forces <see cref="ShowDirectories"/>.
        /// </summary>
        SubNavigation = 0x00400000 | ShowDirectories,

        /// <summary>
        /// Allows the user to navigate directories above the starting directory up to the root
        /// of the starting directory.
        /// </summary>
        RootNavigation = 0x00800000 | SubNavigation,

        /// <summary>
        /// Allows the user to navigate beyond the root directory to fixed drives.
        /// </summary>
        FixedDriveNavigation = 0x01000000 | RootNavigation,

        /// <summary>
        /// Allows the user to navigate beyond the root directory to both fixed and network drives.
        /// </summary>
        NetworkDriveNavigation = 0x02000000 | FixedDriveNavigation,

        /// <summary>
        /// Same as <see cref="NetworkDriveNavigation"/> but allows the use to navigate all drives,
        /// including removable drives (but excludes special drives such as Ram).
        /// </summary>
        GlobalDriveNavigation = 0x04000000 | NetworkDriveNavigation,

        /// <summary>
        /// A flag combination which provides a file browser allowing the user to navigate
        /// sub-directories of the starting <see cref="FileBrowserPage.DirectoryPath"/> only.
        /// The user can view file information, but cannot modify file contents. Combine with other
        /// flags to enable as required.
        /// </summary>
        SubFileBrowser = ShowFiles | ShowDirectories | ShowDirectoryName | ShowSectionHeaders
            | ShowViewOptions | ShowQuitOption | SubNavigation,

        /// <summary>
        /// Same as <see cref="SubFileBrowser"/> but has <see cref="RootNavigation"/>, allowing the
        /// user to navigate up to the root of the <see cref="FileBrowserPage.DirectoryPath"/>
        /// drive.
        /// </summary>
        RootFileBrowser = SubFileBrowser | RootNavigation,

        /// <summary>
        /// Same as <see cref="SubFileBrowser"/> but has <see cref="GlobalDriveNavigation"/>, allowing
        /// the user to navigate between drives accessible by the machine.
        /// </summary>
        GlobalFileBrowser = SubFileBrowser | GlobalDriveNavigation,

        /// <summary>
        /// A flag combination which allows the user to select an existing file from the
        /// <see cref="FileBrowserPage.DirectoryPath"/> directory or any of its sub-directories.
        /// This option is suitable for an "open file" operation. The selected file path
        /// can be retrieved using <see cref="FileBrowserPage.SelectedItem"/>.
        /// </summary>
        SubFileSelector = SubFileBrowser | SelectItemToReturn,

        /// <summary>
        /// Same as <see cref="SubFileSelector"/> but has <see cref="RootNavigation"/>, allowing the
        /// user to select any file on the same drive as the <see cref="FileBrowserPage.DirectoryPath"/>
        /// directory.
        /// </summary>
        RootFileSelector = SubFileSelector | RootNavigation,

        /// <summary>
        /// Same as <see cref="SubFileSelector"/> but has <see cref="GlobalDriveNavigation"/>, allowing the
        /// user to select any file accessible by the machine.
        /// </summary>
        GlobalFileSelector = SubFileSelector | GlobalDriveNavigation,

        /// <summary>
        /// A flag combination which allows the user to select any existing file, or specify a new
        /// one, within the <see cref="FileBrowserPage.DirectoryPath"/> directory or any of its
        /// sub-directories. The user is requested to confirm the selection of an existing file
        /// before return. This option is suitable for a "save file as" operation. The selected
        /// file path can be retrieved using <see cref="FileBrowserPage.SelectedItem"/>.
        /// </summary>
        SubSaveAs = SubFileSelector | AllowSelectNewFile | ConfirmBeforeSelectReturn,

        /// <summary>
        /// Same as <see cref="SubSaveAs"/> but has <see cref="RootNavigation"/>, allowing the
        /// user to select any file on the same drive as the <see cref="FileBrowserPage.DirectoryPath"/>
        /// directory.
        /// </summary>
        RootSaveAs = RootFileSelector | AllowSelectNewFile | ConfirmBeforeSelectReturn,

        /// <summary>
        /// Same as <see cref="SubSaveAs"/> but has <see cref="GlobalDriveNavigation"/>, allowing the
        /// user to select any file accessible by the machine.
        /// </summary>
        GlobalSaveAs = GlobalFileSelector | AllowSelectNewFile | ConfirmBeforeSelectReturn,

        /// <summary>
        /// A flag combination which provides a directory browser allowing the user to select a
        /// sub-directory within the starting <see cref="FileBrowserPage.DirectoryPath"/> only.
        /// Files are not shown. This option is suitable for a "browse for directory" operation.
        /// The selected directory path can be retrieved using <see cref="FileBrowserPage.SelectedItem"/>.
        /// </summary>
        SubDirectorySelector = ShowDirectories | ShowDirectoryName | ShowSectionHeaders
            | ShowViewOptions | ShowQuitOption | SubNavigation | SelectItemToReturn,

        /// <summary>
        /// Same as <see cref="SubDirectorySelector"/> but has <see cref="RootNavigation"/>, allowing
        /// the user to select any directory on the same drive as the
        /// <see cref="FileBrowserPage.DirectoryPath"/> directory.
        /// </summary>
        RootDirectorySelector = SubDirectorySelector | RootNavigation,

        /// <summary>
        /// Same as <see cref="SubDirectorySelector"/> but has <see cref="GlobalDriveNavigation"/>,
        /// allowing the user to select any directory accessible by the machine.
        /// </summary>
        GlobalDirectorySelector = SubDirectorySelector | GlobalDriveNavigation,
    }
}
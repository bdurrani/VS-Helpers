// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace BD.VSHelpers
{
    static class PkgCmdIDList
    {
        public const uint cmdidCopyWithContext = 0x100;
        public const uint cmdidStartWithoutDebug = 0x101;
        /// <summary>
        /// Id for the root menu that is the placeholder for the build exes
        /// </summary>
        public const uint cmdIdBuildExe = 0x200;
    };
}
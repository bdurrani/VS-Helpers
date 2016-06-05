// Guids.cs
// MUST match guids.h
using System;

namespace BD.VSHelpers
{
    static class GuidList
    {
        public const string guidVSHelpersPkgString = "a5a713b9-14a4-4376-ba18-107c20af20ec";
        public const string guidVSHelpersCmdSetString = "b739221f-3e50-4b98-a34f-5e2daa2ec2db";

        public static readonly Guid guidVSHelpersCmdSet = new Guid(guidVSHelpersCmdSetString);
    };
}
﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RunTests
{
    internal struct ProcDumpInfo
    {
        private const string KeyProcDumpFilePath = "ProcDumpFilePath";
        private const string KeyProcDumpDirectory = "ProcDumpOutputPath";

        internal string ProcDumpFilePath { get; }
        internal string DumpDirectory { get; }

        internal ProcDumpInfo(string procDumpFilePath, string dumpDirectory)
        {
            Debug.Assert(Path.IsPathRooted(procDumpFilePath));
            Debug.Assert(Path.IsPathRooted(dumpDirectory));
            ProcDumpFilePath = procDumpFilePath;
            DumpDirectory = dumpDirectory;
        }

        internal void WriteEnvironmentVariables(Dictionary<string, string> environment)
        {
            environment[KeyProcDumpFilePath] = ProcDumpFilePath;
            environment[KeyProcDumpDirectory] = DumpDirectory;
        }

        internal static ProcDumpInfo? ReadFromEnvironment()
        {
            bool validate(string s) => !string.IsNullOrEmpty(s) && Path.IsPathRooted(s);

            var procDumpFilePath = Environment.GetEnvironmentVariable(KeyProcDumpFilePath);
            var dumpDirectory = Environment.GetEnvironmentVariable(KeyProcDumpDirectory);

            if (!validate(procDumpFilePath) || !validate(dumpDirectory))
            {
                return null;
            }

            return new ProcDumpInfo(procDumpFilePath, dumpDirectory);
        }
    }

    internal static class ProcDumpUtil
    {
        internal static Process AttachProcDump(ProcDumpInfo procDumpInfo, int processId)
        {
            return AttachProcDump(procDumpInfo.ProcDumpFilePath, processId, procDumpInfo.DumpDirectory);
        }

        /// <summary>
        /// Attaches a new procdump.exe against the specified process.
        /// </summary>
        /// <param name="procDumpFilePath">The path to the procdump executable</param>
        /// <param name="processId">process id</param>
        /// <param name="dumpDirectory">destination directory for dumps</param>
        internal static Process AttachProcDump(string procDumpFilePath, int processId, string dumpDirectory)
        {
            // /accepteula command line option to automatically accept the Sysinternals license agreement.
            // -ma	Write a 'Full' dump file. Includes All the Image, Mapped and Private memory.
            // -e	Write a dump when the process encounters an unhandled exception. Include the 1 to create dump on first chance exceptions.
            // -t	Write a dump when the process terminates.
            const string procDumpSwitches = "/accepteula -ma -e -t";
            Directory.CreateDirectory(dumpDirectory);
            dumpDirectory = dumpDirectory.TrimEnd('\\');

            return Process.Start(procDumpFilePath, $" {procDumpSwitches} {processId} \"{dumpDirectory}\"");
        }
    }
}

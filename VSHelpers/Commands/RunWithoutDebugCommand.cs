using BD.VSHelpers.WMI.Win32;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BD.VSHelpers.Commands
{
    internal class RunWithoutDebugCommand : BaseCommand
    {
        private ProcessWatcher _procWatcher;

        internal RunWithoutDebugCommand(VSHelpersPackage package)
            : base(package, new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdidStartWithoutDebug))
        { }

        protected override void OnExecute()
        {
            var dte = Package.GetDTE();
            SolutionBuild2 sb = (SolutionBuild2)dte.Solution.SolutionBuild;

            // get the name of the active project
            string startupProjectUniqueName = (string)((Array)sb.StartupProjects).GetValue(0);

            var results = new List<Project>();
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                ProjectHelpers.GetAllProjectsFromProject(project, results);
            }

            results.ToDebugPrint();

            // find the start up project
            var startupProject = results.Where(x => !string.IsNullOrEmpty(x.Name) && string.Equals(x.UniqueName, startupProjectUniqueName));
            if (!startupProject.Any())
            {
                // no startup project found
                return;
            }

            // try to figure out the build outputs of the project
            var fileUrls = ProjectHelpers.GetBuildOutputs(startupProject.First());
            var executables = fileUrls.Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

            if (!executables.Any())
            {
                return;
            }

            string activeProjectExePath = new Uri(executables.First(), UriKind.Absolute).LocalPath;
            string activeExePath = Path.GetFileName(activeProjectExePath);

            CleanupProcWatcher();

            _procWatcher = new ProcessWatcher(activeExePath);
            _procWatcher.ProcessCreated += ProcWatcher_ProcessCreated;
            _procWatcher.ProcessDeleted += ProcWatcher_ProcessDeleted;
            _procWatcher.Start();

            dte.ExecuteCommand("Debug.StartWithoutDebugging");

            // this will get all the build output paths for the active project
            var outputFolders = new List<string>();
            foreach (var strUri in fileUrls)
            {
                var uri = new Uri(strUri, UriKind.Absolute);
                var filePath = uri.LocalPath;
                var folderPath = Path.GetDirectoryName(filePath);
                outputFolders.Add(folderPath.ToLower());
            }
        }

        protected override void OnBeforeQueryStatus()
        {
            // command is enabled only if solution is loaded.
            var dte = Package.GetDTE();
            Solution solution = (Solution)dte.Solution;
            bool isSolutionOpen = solution.IsOpen;
            this.Visible = isSolutionOpen;
            this.Enabled = isSolutionOpen;
        }

        void ProcWatcher_ProcessDeleted(Win32_Process process)
        {
            Debug.WriteLine("process deleted");
        }

        void ProcWatcher_ProcessCreated(Win32_Process process)
        {
            Debug.WriteLine("process created");
            string name = process.Name;
            AttachToProcess(name);
        }

        private void CleanupProcWatcher()
        {
            if (_procWatcher != null)
            {
                _procWatcher.ProcessCreated -= ProcWatcher_ProcessCreated;
                _procWatcher.ProcessDeleted -= ProcWatcher_ProcessDeleted;
                _procWatcher.Dispose();
                _procWatcher = null;
            }
        }

        private void AttachToProcess(string processName)
        {
            var dte = Package.GetDTE();
            Processes processes = dte.Debugger.LocalProcesses;
            var process = processes.Cast<EnvDTE.Process>().Where(proc => proc.Name.Contains(processName));
            if (!process.Any())
            {
                return;
            }

            process.First().Attach();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            CleanupProcWatcher();
            base.Dispose();
        }
    }
}

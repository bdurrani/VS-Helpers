using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.VSHelpers.Commands
{
    internal class RunBuildsCommand: BaseCommand
    {
        internal RunBuildsCommand(VSHelpersPackage package)
            : base(package, new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdIdBuildExe))
        {

        }

        protected override void OnExecute()
        {
            var dte = Package.GetDTE();
            var result = ProjectHelpers.GetBuildExecutablesForSolution(dte);
            //SolutionBuild2 sb = (SolutionBuild2)dte.Solution.SolutionBuild;

            //// get the name of the active project
            ////string startupProjectUniqueName = (string)((Array)sb.StartupProjects).GetValue(0);

            //var results = new List<Project>();
            //foreach (EnvDTE.Project project in dte.Solution.Projects)
            //{
            //    ProjectHelpers.GetAllProjectsFromProject(project, results);
            //}
            //List<string> exes = new List<string>();
            //foreach (var project in results)
            //{
            //    project.ToDebugPrint();
            //    // try to figure out the build outputs of the project
            //    var fileUrls = ProjectHelpers.GetBuildOutputs(project);
            //    var executables = fileUrls.Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            //    exes.AddRange(executables);
            //}

            return;
        }
    }
}

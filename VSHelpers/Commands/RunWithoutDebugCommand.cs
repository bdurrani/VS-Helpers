using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.VSHelpers.Commands
{
    internal class RunWithoutDebugCommand : BaseCommand
    {
        internal RunWithoutDebugCommand(VSHelpersPackage package)
            : base(package, new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdidStartWithoutDebug))
        { 
        }

        protected override void OnExecute()
        {
            return;
        }

        protected override void OnBeforeQueryStatus()
        {
            var dte = Package.GetDTE();
            Solution solution = (Solution)dte.Solution;
            bool isSolutionOpen = solution.IsOpen;
            this.Visible = isSolutionOpen;
            this.Enabled = isSolutionOpen;
        }
    }
}

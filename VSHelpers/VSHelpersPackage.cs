using BD.VSHelpers.WMI.Win32;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BD.VSHelpers
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // auto-load the extension instead on on-demand: http://www.mztools.com/articles/2013/MZ2013027.aspx
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [Guid(GuidList.guidVSHelpersPkgString)]
    [ProvideOptionPage(typeof(OptionPageGrid), "VS Helpers", OptionPageGrid.CategoryName, 0, 0, false)]
    public sealed class VSHelpersPackage : Package
    { 
        private DTEEvents _packageEvents;
        private ProcessWatcher _procWatcher;
        private List<Project> projects = new List<Project>();

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VSHelpersPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                return;
            }

            var dte = GetDTE();
            _packageEvents = dte.Events.DTEEvents;
            _packageEvents.OnBeginShutdown += PackageEvents_OnBeginShutdown;

            AddCopyWithContextMenu(mcs);
            AddStartWithoutDebugging(mcs);
        }

        void PackageEvents_OnBeginShutdown()
        {
            CleanupProcWatcher();
        }

        private void AddStartWithoutDebugging(OleMenuCommandService mcs)
        {
            CommandID menuCommandID = new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdidStartWithoutDebug);
            var menuItem = new OleMenuCommand(StartWithoutDebugging_MenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += MenuCommand_BeforeQueryStatus;
            mcs.AddCommand(menuItem);
        }

        private void AddCopyWithContextMenu(OleMenuCommandService mcs)
        {
            // Create the command for the menu item.
            CommandID menuCommandID = new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdidCopyWithContext);
            var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += MenuCommand_BeforeQueryStatus;
            mcs.AddCommand(menuItem);
        }

        void MenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                var dte = GetDTE();
                Solution solution = (Solution)dte.Solution;
                bool isSolutionOpen = solution.IsOpen;
                menuCommand.Visible = isSolutionOpen;
                menuCommand.Enabled = isSolutionOpen;
            }
        }

        #endregion

        /// <summary>
        /// Gets the DTE
        /// </summary>
        /// <returns>DTE2</returns>
        private EnvDTE80.DTE2 GetDTE()
        {
            return (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
        }

        private void StartWithoutDebugging_MenuItemCallback(object sender, EventArgs e)
        {
            var dte = GetDTE();
            //dte.ExecuteCommand("Debug.StartWithoutDebugging");

            SolutionBuild2 sb = (SolutionBuild2)dte.Solution.SolutionBuild;

            // get the name of the active project
            string startupProjectUniqueName = (string)((Array)sb.StartupProjects).GetValue(0);

            //IVsSolutionBuildManager solutionBuildManager = base.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            //IVsHierarchy ppHierarchy;
            //solutionBuildManager.get_StartupProject(out ppHierarchy);
            //if(ppHierarchy != null)
            //{
            //    var xproj = ppHierarchy as IVsProject3;
            //    var d = xproj.ToString();
            //}

            projects.Clear();
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                GetAllProjectsFromProject(project);
            }

            projects.ToDebugPrint();

            // find the start up project
            var startupProject = projects.Where(x => !string.IsNullOrEmpty(x.Name) && string.Equals(x.UniqueName, startupProjectUniqueName));
            if (!startupProject.Any())
            {
                // no startup project found
                return;
            }

            // try to figure out the build outputs of the project
            var outputGroups = startupProject.First().ConfigurationManager.ActiveConfiguration.OutputGroups.OfType<EnvDTE.OutputGroup>();
            //foreach (OutputGroup group in outputGroups)
            //{
            //    group.ToDebugPrint();
            //}

            var builtGroup = outputGroups.First(x => x.CanonicalName == "Built");

            var fileUrls = ((object[])builtGroup.FileURLs).OfType<string>();
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

            // this will get all the build output paths for the active project
            var outputFolders = new List<string>();
            foreach (var strUri in fileUrls)
            {
                var uri = new Uri(strUri, UriKind.Absolute);
                var filePath = uri.LocalPath;
                var folderPath = Path.GetDirectoryName(filePath);
                outputFolders.Add(folderPath.ToLower());
            }
            return;
        }

        private void AttachToProcess(string processName)
        {
            var dte = GetDTE();
            Processes processes = dte.Debugger.LocalProcesses;
            var process = processes.Cast<EnvDTE.Process>().Where(proc => proc.Name.Contains(processName));
            if(!process.Any())
            {
                return;
            }

            process.First().Attach(); 
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

        void ProcWatcher_ProcessDeleted(Win32_Process process)
        {
            Debug.WriteLine("process deleted");
        }

        void ProcWatcher_ProcessCreated(Win32_Process process)
        {
            string name = process.Name;
            Debug.WriteLine("process created");
            AttachToProcess(name);
        }


        private void GetAllProjectsFromProject(Project project)
        {
            if (project == null)
            {
                return;
            }

            projects.Add(project);

            if (project.ProjectItems != null)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item != null)
                    {
                        GetAllProjectsFromProject(item.Object as Project);
                    }
                }
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary> 
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = GetDTE();
            CopyCurrentMethod(dte);
        }

        private TextSelection GetSelection(DTE2 dte)
        {
            var activeDoc = dte.ActiveDocument;
            Debug.WriteLine(activeDoc.ToDebugPrint());
            return activeDoc.Selection as TextSelection;
        }

        private void CopyCurrentMethod(DTE2 dte)
        {
            var selection = GetSelection(dte);
            if (selection == null)
            {
                return;
            }

            var selectionText = selection.Text.Trim();
            var txtPoint = GetCursorTextPoint(dte);
            string location = GetCodeElementNonRecursively(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements, txtPoint);
            OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
            if (page.ClipboardFormat == OptionPageGrid.CopyFormat.Slack)
            {
                string result = string.Format("```\n{0}\n```\n{2} Line {1}", selectionText, selection.CurrentLine, location);
                System.Windows.Clipboard.SetText(result);
            }

            if (page.ClipboardFormat == OptionPageGrid.CopyFormat.RTF)
            {
                var rtf = new RichTextBox();
                rtf.Font = new System.Drawing.Font("Consolas", 10);
                rtf.Text = selectionText;
                rtf.Font = new System.Drawing.Font("Calibri", 11);
                rtf.AppendText(string.Format("\n{0}, Line {1}", location, selection.CurrentLine));
                System.Windows.Clipboard.SetText(rtf.Rtf, System.Windows.TextDataFormat.Rtf);
            }
        }

        private TextPoint GetCursorTextPoint(DTE2 dte)
        {
            try
            {
                var selection = dte.ActiveDocument.Selection as TextSelection;
                return selection.ActivePoint;
            }
            catch (Exception)
            {
            }
            return null;
        }

        private string GetCodeElementNonRecursively(CodeElements codeElements, TextPoint txtPoint)
        {
            if (codeElements == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            while (codeElements != null)
            {
                CodeElement foundElement = FindMatch(txtPoint, codeElements);
                if (foundElement != null)
                {
                    switch (foundElement.Kind)
                    {
                        case vsCMElement.vsCMElementNamespace:
                        case vsCMElement.vsCMElementClass:
                        case vsCMElement.vsCMElementEnum:
                            sb.Append(foundElement.Name + ".");
                            break;
                        default:
                            break;
                    }
                }
                codeElements = GetCodeElementMembers(foundElement);
            }
            return sb.ToString().TrimEnd('.');
        }

        private CodeElement FindMatch(TextPoint txtPoint, CodeElements codeElements)
        {
            if (codeElements == null)
            {
                return null;
            }

            foreach (CodeElement item in codeElements)
            {
                if (item.StartPoint.LessThan(txtPoint) && item.EndPoint.GreaterThan(txtPoint))
                {
                    Debug.WriteLine(item.ToDebugPrint());
                    return item;
                }
            }
            return null;
        }

        private CodeElements GetCodeElementMembers(CodeElement codeElement)
        {
            if (codeElement is CodeNamespace)
            {
                return (codeElement as CodeNamespace).Members;
            }
            else if (codeElement is CodeType)
            {
                return (codeElement as CodeType).Members;
            }
            else if (codeElement is CodeFunction)
            {
                return (codeElement as CodeFunction).Parameters;
            }
            return null;
        }

    }
}

using BD.VSHelpers.Commands;
using BD.VSHelpers.Options;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;

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
        private List<BaseCommand> _commands = new List<BaseCommand>();
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

            _commands.Add(new RunWithoutDebugCommand(this));
            _commands.Add(new CopyWithContextCommand(this));
            _commands.Add(new RunBuildsCommand(this));
            foreach (var command in _commands)
            {
                mcs.AddCommand(command);
            }
            //InitMRUMenu(mcs);
        }

        public RegistryKey RegistryKey
        {
            get
            {
                return UserRegistryRoot;
            }
        }

        void PackageEvents_OnBeginShutdown()
        {
            // clean up any commands
            foreach (var command in _commands)
            {
                command.Dispose();
            }
            _commands.Clear();
        }

        /// <summary>
        /// Options menu
        /// </summary>
        public OptionPageGrid Options
        {
            get
            {
                return (OptionPageGrid)this.GetDialogPage(typeof(OptionPageGrid));
            }
        }

        #endregion

        /// <summary>
        /// Gets the DTE
        /// </summary>
        /// <returns>DTE2</returns>
        public EnvDTE80.DTE2 GetDTE()
        {
            return (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
        }

        private int numMRUItems = 4;
        private int baseMRUID = (int)PkgCmdIDList.cmdIdBuildExe;
        private List<string> mruList = new List<string>();

        private void InitMRUMenu(OleMenuCommandService mcs)
        {
            for (int i = 0; i < this.numMRUItems; i++)
            {
                var cmdID = new CommandID(GuidList.guidVSHelpersCmdSet, this.baseMRUID + i);
                var mc = new OleMenuCommand(new EventHandler(OnMRUExec), cmdID);
                mc.BeforeQueryStatus += new EventHandler(OnMRUQueryStatus);
                mcs.AddCommand(mc);
                mruList.Add(i.ToString());
            }
        }

        private void OnMRUExec(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (null != menuCommand)
            {
                int MRUItemIndex = menuCommand.CommandID.ID - this.baseMRUID;
                if (MRUItemIndex >= 0 && MRUItemIndex < this.mruList.Count)
                {
                    string selection = this.mruList[MRUItemIndex] as string;
                    for (int i = MRUItemIndex; i > 0; i--)
                    {
                        this.mruList[i] = this.mruList[i - 1];
                    }
                    this.mruList[0] = selection;
                    System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "Selected {0}", selection));
                }
            }
        }

        private void OnMRUQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (null != menuCommand)
            {
                int MRUItemIndex = menuCommand.CommandID.ID - this.baseMRUID;
                if (MRUItemIndex >= 0 && MRUItemIndex < this.mruList.Count)
                {
                    menuCommand.Text = this.mruList[MRUItemIndex] as string;
                }
            }
        }
    }
}

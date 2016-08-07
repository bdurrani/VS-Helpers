using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Windows.Input;

namespace BD.VSHelpers.Options
{
    /// <summary>
    /// Format for the text being copied
    /// </summary>
    [Description("Copy format option")]
    public enum CopyFormat
    {
        /// <summary>
        /// Use Slack's markdown format
        /// </summary>
        Slack,
        /// <summary>
        /// Use Rich Text
        /// </summary>
        RTF
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class OptionPageGrid : DialogPage
    {
        public const string CategoryName = "General";
        private CopyFormat optionValue = CopyFormat.Slack;

        private ICommand _refreshCommand;

        [Category(CategoryName)]
        [DisplayName("Copy format")]
        [Description("Specify the format for the copied text.")]
        public CopyFormat ClipboardFormat
        {
            get { return optionValue; }
            set { optionValue = value; }
        }

        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                var control = new OptionsPageControl(this);
                return control;
            }
        }

        #region Command

        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new CommandHandler(() => RefreshAction(), true));
            }
        }

        public void RefreshAction()
        {
            return;
        }

        #endregion
    }

    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public event EventHandler CanExecuteChanged;

        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        } 

        public void Execute(object parameter)
        {
            _action();
        }
    }
}

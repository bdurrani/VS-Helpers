using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;


namespace BD.VSHelpers
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class OptionPageGrid : DialogPage
    {
        public const string CategoryName = "General";
        private CopyFormat optionValue = CopyFormat.Slack;

        public enum CopyFormat
        {
            Slack,
            Skype
        }

        [Category(CategoryName)]
        [DisplayName("Copy format")]
        [Description("Specify the target application where the copied text will be used.")]
        public CopyFormat ClipboardFormat
        {
            get { return optionValue; }
            set { optionValue = value; }
        }
    }
}

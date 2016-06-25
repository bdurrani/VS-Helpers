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

        [Category(CategoryName)]
        [DisplayName("Copy format")]
        [Description("Specify the format for the copied text.")]
        public CopyFormat ClipboardFormat
        {
            get { return optionValue; }
            set { optionValue = value; }
        }
    }
}

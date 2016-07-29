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

        /// <summary>
        /// Additional property for binding the enum
        /// </summary>
        public IEnumerable<CopyFormat> CopyFormatEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(CopyFormat)).Cast<CopyFormat>();
            }
        }

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
        #endregion
    }
}

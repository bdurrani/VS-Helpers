using BD.VSHelpers.Options;
using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BD.VSHelpers.Commands
{
    internal class CopyWithContextCommand : BaseCommand
    {
        internal CopyWithContextCommand(VSHelpersPackage package)
            : base(package, new CommandID(GuidList.guidVSHelpersCmdSet, (int)PkgCmdIDList.cmdidCopyWithContext))
        { }

        protected override void OnBeforeQueryStatus()
        {
            var dte = Package.GetDTE();
            Solution solution = (Solution)dte.Solution;
            bool isSolutionOpen = solution.IsOpen;
            this.Visible = isSolutionOpen;
            this.Enabled = isSolutionOpen;
        }

        protected override void OnExecute()
        {
            CopyCurrentMethod();
        }

        private void CopyCurrentMethod()
        {
            var dte = Package.GetDTE();
            var selection = GetSelection(dte);
            if (selection == null)
            {
                return;
            }

            var selectionText = selection.Text.Trim();
            var txtPoint = GetCursorTextPoint(dte);
            string location = GetCodeElementNonRecursively(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements, txtPoint);
            OptionPageGrid page = Package.Options;
            if (page.ClipboardFormat == OptionPageGrid.CopyFormat.Slack)
            {
                string result = string.Format("```\n{0}\n```\n{2} Line {1}", selectionText, selection.CurrentLine, location);
                System.Windows.Forms.Clipboard.SetDataObject(result, false, 5, 300);
            }
            else if (page.ClipboardFormat == OptionPageGrid.CopyFormat.RTF)
            {
                var rtf = new RichTextBox();
                rtf.Font = new System.Drawing.Font("Consolas", 10);
                rtf.Text = selectionText;
                rtf.Font = new System.Drawing.Font("Calibri", 11);
                rtf.AppendText(string.Format("\n{0}, Line {1}", location, selection.CurrentLine));
                var dataObject = new DataObject();
                dataObject.SetData(DataFormats.Rtf, rtf.Rtf);
                System.Windows.Forms.Clipboard.SetDataObject(dataObject, false, 5, 300);
            }
        }

        private TextSelection GetSelection(DTE2 dte)
        {
            var activeDoc = dte.ActiveDocument;
            activeDoc.ToDebugPrint();
            return activeDoc.Selection as TextSelection;
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
                    item.ToDebugPrint();
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

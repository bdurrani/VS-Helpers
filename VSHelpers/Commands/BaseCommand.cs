using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.VSHelpers.Commands
{
    internal abstract class BaseCommand : OleMenuCommand
    {
        protected VSHelpersPackage Package { get; private set; }

        /// <summary>
        /// Base implementation of a command
        /// </summary>
        /// <param name="package">the package</param>
        /// <param name="id">command id</param>
        protected BaseCommand(VSHelpersPackage package, CommandID id)
            : base(BaseCommand_Execute, id)
        {
            Package = package;
            this.BeforeQueryStatus += BaseCommand_BeforeQueryStatus;
        }

        void BaseCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var baseCommand = sender as BaseCommand;
            if(baseCommand != null)
            {
                baseCommand.OnBeforeQueryStatus();
            }
        }

        /// <summary>
        /// Handles the Execute event of the BaseCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void BaseCommand_Execute(object sender, EventArgs e)
        {
            BaseCommand command = sender as BaseCommand;
            if (command != null)
            {
                command.OnExecute();
            }
        }

        /// <summary>
        /// Called to update the current status of the command.
        /// </summary>
        protected virtual void OnBeforeQueryStatus()
        {
            // By default, commands are always enabled.
            Enabled = true;
        }

        /// <summary>
        /// Called to execute the command.
        /// </summary>
        protected abstract void OnExecute();
    }

}

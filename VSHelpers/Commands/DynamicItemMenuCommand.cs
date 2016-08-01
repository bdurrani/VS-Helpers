﻿using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace BD.VSHelpers.Commands
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/bb166492(v=vs.120).aspx
    /// </summary>
    class DynamicItemMenuCommand: OleMenuCommand
    {
        private Predicate<int> matches;
        public DynamicItemMenuCommand(CommandID rootId, Predicate<int> matches, EventHandler invokeHandler, EventHandler beforeQueryStatusHandler)
            : base(invokeHandler, null /*changeHandler*/, beforeQueryStatusHandler, rootId)
        {
            if (matches == null)
            {
                throw new ArgumentNullException("matches");
            }

            this.matches = matches;
        }

        public override bool DynamicItemMatch(int cmdId)
        {
            // Call the supplied predicate to test whether the given cmdId is a match.
            // If it is, store the command id in MatchedCommandid 
            // for use by any BeforeQueryStatus handlers, and then return that it is a match.
            // Otherwise clear any previously stored matched cmdId and return that it is not a match.
            if (this.matches(cmdId))
            {
                this.MatchedCommandId = cmdId;
                return true;
            }

            this.MatchedCommandId = 0;
            return false;
        }
    }
}

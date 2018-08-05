using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace MrHideyUnloady
{
    internal sealed class UnloadedProjectFilterCommand
    {
        public const int CommandId = 0x0100;

        public const string CommandSetString = "79b1c195-c11a-41c3-82eb-4712a192fece";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnloadedProjectFilterCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private UnloadedProjectFilterCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
        }
    }
}

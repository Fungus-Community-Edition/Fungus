using System;

namespace Amanita.VScripting
{
    /// <summary>
    /// Attribute class for Fungus commands.
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandInfoAttribute : Attribute
    {
        /// <summary>
        /// Metadata atribute for the Command class. 
        /// </summary>
        /// <param name="category">The category to place this command in.</param>
        /// <param name="commandName">The display name of the command.</param>
        /// <param name="helpText">Help information to display in the inspector.</param>
        /// <param name="priority">If two command classes have the same name, the one with highest priority is listed. Negative priority removess the command from the list.</param>///
        public CommandInfoAttribute(string category, string commandName, string helpText, int priority = 0)
        {
            this.Category = category;
            this.CommandName = commandName;
            this.HelpText = helpText;
            this.Priority = priority;
        }

        public string Category { get; set; }
        public string CommandName { get; set; }
        public string HelpText { get; set; }
        public int Priority { get; set; }
    }

}
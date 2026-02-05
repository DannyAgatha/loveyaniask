using System;

namespace WingsAPI.Scripting.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptEventAttribute : System.Attribute
    {
        public ScriptEventAttribute(string name, bool isRemovedOnTrigger)
        {
            IsRemovedOnTrigger = isRemovedOnTrigger;
            Name = name;
        }

        public string Name { get; }

        public bool IsRemovedOnTrigger { get; }
    }
}
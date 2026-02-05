using System;

namespace WingsAPI.Scripting
{
    public class InvalidScriptException : Exception
    {
        public InvalidScriptException(string message) : base(message)
        {
        }
    }
}
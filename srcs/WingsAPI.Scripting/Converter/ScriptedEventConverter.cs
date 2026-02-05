using System;
using PhoenixLib.Events;
using WingsAPI.Scripting.Event;

namespace WingsAPI.Scripting.Converter
{
    public interface IScriptedEventConverter
    {
        Type EventType { get; }
        IAsyncEvent Convert(SEvent e);
    }

    public abstract class ScriptedEventConverter<T> : IScriptedEventConverter where T : SEvent
    {
        public Type EventType { get; } = typeof(T);
        public IAsyncEvent Convert(SEvent e) => Convert((T)e);
        protected abstract IAsyncEvent Convert(T e);
    }
}
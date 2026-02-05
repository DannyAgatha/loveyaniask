using System.Collections.Generic;
using FluentValidation;
using WingsAPI.Scripting.Event;

namespace WingsAPI.Scripting.Validator.Common
{
    public class SEventsValidator : AbstractValidator<IDictionary<string, IEnumerable<SEvent>>>
    {
    }
}
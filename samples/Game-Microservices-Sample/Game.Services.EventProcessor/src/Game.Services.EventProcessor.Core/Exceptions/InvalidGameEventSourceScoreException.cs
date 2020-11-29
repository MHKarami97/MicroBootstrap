using System;
using System.Runtime.Serialization;

namespace Game.Services.EventProcessor.Core.Entities.Exceptions
{
    public class InvalidGameEventSourceScoreException : DomainException
    {
        public InvalidGameEventSourceScoreException():base ("Invalid GameEventSource Score.")
        {
            
        }
    }
}
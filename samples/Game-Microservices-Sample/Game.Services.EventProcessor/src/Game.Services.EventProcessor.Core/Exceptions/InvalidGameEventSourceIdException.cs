using System;
using System.Runtime.Serialization;

namespace Game.Services.EventProcessor.Core.Entities.Exceptions
{
    public class InvalidGameEventSourceIdException : DomainException
    {
        public InvalidGameEventSourceIdException():base ("Invalid GameEventSource Id.")
        {
            
        }
    }
}
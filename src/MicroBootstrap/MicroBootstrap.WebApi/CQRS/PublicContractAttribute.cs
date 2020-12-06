using System;

namespace MicroBootstrap.WebApi.CQRS
{
    //Marker
    [AttributeUsage(AttributeTargets.Class)]
    public class PublicContractAttribute : Attribute
    {
    }
}
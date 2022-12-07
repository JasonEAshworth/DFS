using System;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Used to link a service to a data interface type
    /// </summary>
    public interface IHaveDataInterfaceType : IDisposable
    {
        /// <summary>
        /// Type of data interface for which the service is implemented
        /// </summary>
        string InterfaceType { get; }
    }
}
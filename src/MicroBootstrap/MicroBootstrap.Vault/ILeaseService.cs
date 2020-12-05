using System.Collections.Generic;

namespace MicroBootstrap.Vault
{
    public interface ILeaseService
    {
        IReadOnlyDictionary<string, LeaseData> All { get; }
        LeaseData Get(string key);
        void Set(string key, LeaseData data);
    }
}
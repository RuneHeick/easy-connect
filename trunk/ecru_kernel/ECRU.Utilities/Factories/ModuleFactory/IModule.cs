using System;
using Microsoft.SPOT;

namespace ECRU.Utilities.Factories.ModuleFactory
{
    public interface IModule
    {
        void LoadConfig(string configFilePath);
        void Start();
        void Stop();
        void Reset();
    }

}

using System;
using System.Collections.Generic;

namespace iBMSApp.Services
{
    public static class DIContainer
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void RegisterSingleton<TInterface, TImplementation>() where TImplementation : new()
        {
            _services[typeof(TInterface)] = new TImplementation();
        }

        public static void RegisterSingleton<T>(Func<T> factory)
        {
            _services[typeof(T)] = factory();
        }

        public static void RegisterLogger<T>()
        {
            _services[typeof(ILogger<T>)] = new UnityLogger<T>();
        }

        public static T Resolve<T>()
        {
            return (T)_services[typeof(T)];
        }

        public static bool Has<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

    }
}

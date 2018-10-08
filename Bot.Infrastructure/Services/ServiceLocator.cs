using System;
using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Bot.Infrastructure.Services
{
    public static class ServiceLocator
    {
        private static IResolutionRoot _resolutionRoot;

        public static void SetRoot(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public static T Get<T>(params IParameter[] parameters)
        {
            return _resolutionRoot.Get<T>(parameters);
        }

        public static object Get(Type serviceType)
        {
            return _resolutionRoot.Get(serviceType);
        }

        public static object Get(Type serviceType, params IParameter[] parameters)
        {
            return _resolutionRoot.Get(serviceType, parameters);
        }
    }
}

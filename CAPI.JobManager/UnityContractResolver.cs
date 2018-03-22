using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Unity;

namespace CAPI.JobManager
{
    public class UnityContractResolver : DefaultContractResolver
    {
        private readonly UnityContainer _container;

        public UnityContractResolver(UnityContainer container)
        {
            _container = container;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            if (_container.IsRegistered(objectType))
            {
                JsonObjectContract contract = ResolveContact(objectType);
                contract.DefaultCreator = () => _container.Resolve(objectType);

                return contract;
            }
            return base.CreateObjectContract(objectType);
        }

        private JsonObjectContract ResolveContact(Type objectType)
        {
            // attempt to create the contact from the resolved type
            var registration = _container.Registrations.FirstOrDefault(r => r.RegisteredType == objectType);
            if (registration != null)
            {
                //Type viewType = (registration.MappedToType as ReflectionActivator)?.LimitType;
                //if (viewType != null)
                //{
                //    return base.CreateObjectContract(viewType);
                //}
            }

            // fall back to using the registered type
            return base.CreateObjectContract(objectType);
        }
    }
}

// Copyright 2007-2019 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Registration
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Courier;
    using Definition;
    using Internals.Extensions;
    using Util;


    public static class ExecuteActivityDefinitionRegistrationCache
    {
        static CachedRegistration GetOrAdd(Type type)
        {
            if (!type.HasInterface(typeof(IExecuteActivityDefinition<,>)))
                throw new ArgumentException($"The type is not an activity definition: {TypeMetadataCache.GetShortName(type)}", nameof(type));

            var argumentTypes = type.GetClosingArguments(typeof(IExecuteActivityDefinition<,>)).ToArray();
            var genericType = typeof(CachedRegistration<,,>).MakeGenericType(type, argumentTypes[0], argumentTypes[1]);

            return Cached.Instance.GetOrAdd(type, _ => (CachedRegistration)Activator.CreateInstance(genericType));
        }

        public static void Register(Type sagaDefinitionType, IContainerRegistrar registrar)
        {
            GetOrAdd(sagaDefinitionType).Register(registrar);
        }


        static class Cached
        {
            internal static readonly ConcurrentDictionary<Type, CachedRegistration> Instance = new ConcurrentDictionary<Type, CachedRegistration>();
        }


        interface CachedRegistration
        {
            void Register(IContainerRegistrar registrar);
        }


        class CachedRegistration<TDefinition, TActivity, TArguments> :
            CachedRegistration
            where TDefinition : class, IExecuteActivityDefinition<TActivity, TArguments>
            where TActivity : class, ExecuteActivity<TArguments>
            where TArguments : class
        {
            public void Register(IContainerRegistrar registrar)
            {
                registrar.RegisterExecuteActivityDefinition<TDefinition, TActivity, TArguments>();
            }
        }
    }
}

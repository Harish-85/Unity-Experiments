using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute:Attribute
    {
            public InjectAttribute()
            {
                
            }
    }
    
    public sealed class ProvideAttribute:Attribute
    {
        public ProvideAttribute()
        {
        }
    }


    public class Injector:MonoBehaviour
    {
        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        readonly Dictionary<Type,object> registery = new();

        public static Injector Instance;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            var providers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID)
                .OfType<IDependencyProvider>();

            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
            
            var injectables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID)
                .Where(IsInjectable);
            
            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        void Inject(object instance)
        {
            var type = instance.GetType();
            var injectableFields = type.GetFields(flags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var field in injectableFields)
            {
                Type fieldType = field.FieldType;
                
                var resolved = Resolve(fieldType);
                
                if (resolved == null)
                {
                    throw new Exception($"Failed to resolve {fieldType.Name} for field {field.Name} in {type.Name}");
                }
                else
                {
                    field.SetValue(instance,resolved);
                    Debug.Log($"Injected {fieldType.Name} into {type.Name}.{field.Name}");
                }
            }
            
        }

        object Resolve(Type type)
        {
            registery.TryGetValue(type,out var result);
            return result;
        }
        
        static bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(flags);
            return members.Any( m => Attribute.IsDefined(m,typeof(InjectAttribute)));
        }
        
        void RegisterProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(flags);

            foreach (var meth in methods)
            {
                if(!Attribute.IsDefined(meth,typeof(ProvideAttribute)))
                    continue;
                
                var returnType = meth.ReturnType;
                var providerInstance = meth.Invoke(provider,null);

                if (providerInstance == null)
                {
                    throw new Exception($"Provider {provider.GetType().Name} returned null for method {meth.Name}");
                }
                else
                {
                    registery.Add( returnType,providerInstance);   
                    Debug.Log($"Registered {returnType.Name} with provider {provider.GetType().Name}");
                }
                
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
using System;
using UnityEngine;

namespace SerializableInterface
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface, "Type must be an interface");
            InterfaceType = interfaceType;
        }
    
    }

    public class InterfaceReference<TInterface, TObject> where TInterface : class where TObject : class
    {
        [SerializeField ,HideInInspector] TObject underlyingValue;

        public TInterface Value
        {
            get => underlyingValue switch
            {
                null => null,
                TInterface value => value,
                _ => throw new InvalidCastException($"Underlying value is not of type {typeof(TInterface)}")
            };
            set => underlyingValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new InvalidCastException($"Value is not of type {typeof(TObject)}")
            };
        }
        
        public TObject UnderlyingValue{ get => underlyingValue; set => underlyingValue = value; }
        
    
        
    }
}

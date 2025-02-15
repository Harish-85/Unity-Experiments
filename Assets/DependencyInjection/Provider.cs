using UnityEngine;

namespace DependencyInjection
{
    public class Provider: MonoBehaviour , IDependencyProvider
    {
        [Provide]
        public ServiceA ProviceServiceA()
        {
            return new ServiceA();
        }
        [Provide]
        public ServiceB ProvideFloat()
        {
            return new ServiceB();
        }
        [Provide]
        public string ProvideString()
        {
            return "Hello World";
        }
    }

    public class ServiceA
    {
        public void Initialize()
        {
            Debug.Log("ServiceA initialized");
        }
    }
    
    public class ServiceB
    {
        public void Initialize()
        {
            Debug.Log("ServiceB initialized");
        }
    }
    
}
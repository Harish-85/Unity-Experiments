using UnityEngine;

namespace DependencyInjection
{
    public class ClassA : MonoBehaviour
    {
        ServiceA serviceA;
        
        [Inject]
        public void InjectServiceA(ServiceA serviceA)
        {
            this.serviceA = serviceA;
        }
    }
}
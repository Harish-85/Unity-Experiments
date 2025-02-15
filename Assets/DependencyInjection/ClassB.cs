
using UnityEngine;

namespace DependencyInjection
{
    public class ClassB : MonoBehaviour
    {
        [Inject]
        public ServiceA serviceA;
        
        [Inject]
        public ServiceB ServiceB;
        
        [Inject]
        public string stringField;
        
    }
}
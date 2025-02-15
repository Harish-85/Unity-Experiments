using UnityEngine;

namespace Ascii
{
    [ExecuteAlways]
    public class ImageEffectRenderer : MonoBehaviour
    {
        public Material material;
        
        public bool useEffect = true;
        
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!useEffect)
            {
                Graphics.Blit(src, dest);
                return;
            }
            Graphics.Blit(src, dest, material);
        }
    }
}
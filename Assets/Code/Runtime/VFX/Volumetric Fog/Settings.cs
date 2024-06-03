using System;
using UnityEngine;

namespace SwapChains.Runtime.VFX.VolumetricFog
{
    [Serializable]
    public class Settings
    {
        [Range(1, 8)] public int fogDownsampleLevel = 4;
        public Material fogMaterial;
        public Material depthMaterial;
        public Material compositeMaterial;
        public string compositeMaterialColourTextureName = "_ColourTexture";
        public string compositeMaterialFogTextureName = "_FogTexture";
        public string compositeMaterialDepthTextureName = "_DepthTexture";
        public RenderTextureQuality renderTextureQuality = RenderTextureQuality.Medium;
    }
}

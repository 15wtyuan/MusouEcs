using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance;

        [SerializeField] public Texture2D[] ordinaryTextures;
        public Material uniMaterial;
        [SerializeField] public Mesh quadMesh;

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void Awake()
        {
            Instance = this;
            CreateTextureArray();
        }

        private void CreateTextureArray()
        {
            // Create Texture2DArray
            var texture2DArray = new
                Texture2DArray(ordinaryTextures[0].width,
                    ordinaryTextures[0].height, ordinaryTextures.Length,
                    TextureFormat.RGBA32, true, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Repeat // Loop through ordinary textures and copy pixels to the
                }; // Apply settings
            // Texture2DArray
            for (var i = 0; i < ordinaryTextures.Length; i++)
            {
                texture2DArray.SetPixels(ordinaryTextures[i].GetPixels(0),
                    i, 0);
            } // Apply our changes

            texture2DArray.Apply(); // Set the texture to a material
            uniMaterial.SetTexture(MainTex, texture2DArray);
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
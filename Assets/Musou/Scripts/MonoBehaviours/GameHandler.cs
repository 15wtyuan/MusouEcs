using System;
using UnityEngine;

namespace MusouEcs
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance;
        [SerializeField] public Material unitMaterial;
        [SerializeField] public Mesh quadMesh;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
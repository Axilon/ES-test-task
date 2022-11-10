using System;
using UnityEngine;

namespace ApplicationContext
{
    public class ApplicationContext : MonoBehaviour
    {
        [SerializeField] private LightningGenerator.LightningGenerator LightningGenerator;
        [SerializeField] private LightningManager.LightningManager LightningManager;

        private void Awake()
        {
            LightningGenerator.Init();
            LightningManager.Init();
        }
        
        private void Start()
        {
            LightningGenerator.InitComplete();
            LightningManager.InitComplete();
        }

        private void OnDestroy()
        {
            LightningGenerator.Dispose();
            LightningManager.Dispose();
        }
    }
}
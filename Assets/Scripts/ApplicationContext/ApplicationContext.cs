using System;
using UnityEngine;
using VideoRecorder;

namespace ApplicationContext
{
    public class ApplicationContext : MonoBehaviour
    {
        [SerializeField] private LightningGenerator.LightningGenerator _lightningGenerator;
        [SerializeField] private LightningManager.LightningManager _lightningManager;
        [SerializeField] private VideoRecorderManager _videoRecorderManager;

        private void Awake()
        {
            _videoRecorderManager.Init();
            _lightningGenerator.Init();
            _lightningManager.Init();
        }
        
        private void Start()
        {
            _videoRecorderManager.InitComplete();
            _lightningGenerator.InitComplete();
            _lightningManager.InitComplete();
        }

        private void OnDestroy()
        {
            _videoRecorderManager.Dispose();
            _lightningGenerator.Dispose();
            _lightningManager.Dispose();
        }
    }
}
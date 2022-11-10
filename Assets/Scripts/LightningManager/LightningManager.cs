using System.Collections.Generic;
using Data;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LightningManager
{
    public class LightningManager : BaseApplicationContextComponent
    {
        [SerializeField] private LightningGenerator.LightningGenerator _generator;
        [SerializeField] private Range<int> _flashesRange;
        [SerializeField] private Range<float> _flashesTimeRange;
        [SerializeField] private Range<float> _spawnTimeRage;
        
        [Range(0,20)]
        
        [SerializeField] private ushort _ligtsToSpawn;
        
        private GameObject[] _lightnings;
        private List<Sequence> _activeAnimationSequences;

        public override void Init()
        {
            _lightnings = new GameObject[_ligtsToSpawn];
            _activeAnimationSequences = new List<Sequence>(_ligtsToSpawn);
        }

        public override void InitComplete()
        {
            SpawnLightning();
        }

        private async void SpawnLightning()
        {
            for (int i = 0; i < _ligtsToSpawn; i++)
            {
                var go = await _generator.GenerateLightning();
                RunFlashAnimation(go);
            }
        }

        private void RunFlashAnimation(GameObject lightning)
        {
            var flashes = Random.Range(_flashesRange.Min, _flashesRange.Max);
            
            var canvasGroupComp = lightning.GetComponent<CanvasGroup>();
            var sequence = DOTween.Sequence();
            
            sequence.OnStart(() => canvasGroupComp.alpha = 0);
            sequence.AppendInterval(Random.Range(_spawnTimeRage.Min,_spawnTimeRage.Max));
            
            for (int i = 0; i < flashes; i++)
            {
                sequence.Append(canvasGroupComp.DOFade(1, Random.Range(_flashesTimeRange.Min, _flashesTimeRange.Max)).SetEase(Ease.InOutBounce));
                sequence.Append(canvasGroupComp.DOFade(0, 0.1f));
            }

            sequence.OnComplete(() =>
            {
                _activeAnimationSequences.Remove(sequence);
                RunFlashAnimation(lightning);
            });
            sequence.Play();
            
            _activeAnimationSequences.Add(sequence);
        }
        
        public override void Dispose()
        {
            foreach (var sequence in _activeAnimationSequences)
            {
                sequence?.Kill();
            }
            _activeAnimationSequences = null;
        }
    }
}
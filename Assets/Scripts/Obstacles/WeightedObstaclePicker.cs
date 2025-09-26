using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryDogs.Obstacles
{
    /// <summary>
    /// Deterministic weighted picker that avoids per-frame allocations.
    /// </summary>
    public sealed class WeightedObstaclePicker
    {
        private readonly List<float> _cumulativeWeights = new();
        private readonly List<ObstacleManager.ObstacleDefinition> _definitions = new();
        private float _totalWeight;

        public int Count => _definitions.Count;

        public void Configure(IReadOnlyList<ObstacleManager.ObstacleDefinition> definitions)
        {
            _definitions.Clear();
            _cumulativeWeights.Clear();
            _totalWeight = 0f;

            if (definitions == null)
            {
                return;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || definition.weight <= 0f || definition.obstaclePrefab == null)
                {
                    continue;
                }

                _totalWeight += definition.weight;
                _definitions.Add(definition);
                _cumulativeWeights.Add(_totalWeight);
            }
        }

        public ObstacleManager.ObstacleDefinition Pick(float sample)
        {
            if (_definitions.Count == 0)
            {
                return null;
            }

            var clamped = Mathf.Clamp01(sample);
            var target = clamped * _totalWeight;
            for (var i = 0; i < _cumulativeWeights.Count; i++)
            {
                if (target <= _cumulativeWeights[i])
                {
                    return _definitions[i];
                }
            }

            return _definitions[^1];
        }
    }
}

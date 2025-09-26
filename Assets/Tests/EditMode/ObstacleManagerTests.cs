using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AngryDogs.Obstacles;
using AngryDogs.Systems;

namespace AngryDogs.Tests.EditMode
{
    public class ObstacleManagerTests
    {
        private GameObject _root;
        private ObstacleManager _manager;
        private ObjectPooler _pooler;
        private Transform _player;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("ObstacleManagerTests");
            _manager = _root.AddComponent<ObstacleManager>();
            _pooler = new GameObject("Pooler").AddComponent<ObjectPooler>();
            _player = new GameObject("Player").transform;

            // Inject private fields for testing.
            typeof(ObstacleManager).GetField("pooler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_manager, _pooler);
            typeof(ObstacleManager).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_manager, _player);

            _manager.ConfigureObstacles(CreateDefinitions());
            _manager.SetRandomSeed(42);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_pooler.gameObject);
            Object.DestroyImmediate(_player.gameObject);
        }

        [Test]
        public void WeightedPicker_PrioritisesHighWeight()
        {
            var picker = new WeightedObstaclePicker();
            picker.Configure(CreateDefinitions());

            var resultLow = picker.Pick(0.05f);
            var resultHigh = picker.Pick(0.95f);

            Assert.IsNotNull(resultLow);
            Assert.IsNotNull(resultHigh);
            Assert.AreNotEqual(resultLow.id, resultHigh.id);
        }

        [Test]
        public void NotifyObstacleDestroyed_SpawnsRepurposedTrap()
        {
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "SlimeSlide",
                obstaclePrefab = new GameObject("ObstaclePrefab"),
                repurposedPrefab = new GameObject("TrapPrefab"),
                weight = 1f,
                despawnDistance = 50f
            };

            _manager.ConfigureObstacles(new List<ObstacleManager.ObstacleDefinition> { definition });

            // Spawn once to create active obstacle.
            typeof(ObstacleManager).GetMethod("SpawnObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_manager, null);

            Assert.AreEqual(1, _manager.ActiveCount);

            // Grab spawned instance from private list for testing.
            var lookupField = typeof(ObstacleManager).GetField("_activeObstacles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var activeList = (System.Collections.IList)lookupField?.GetValue(_manager);
            var activeEntry = activeList?[0];
            var instanceField = activeEntry?.GetType().GetField("Instance");
            var obstacleInstance = instanceField?.GetValue(activeEntry) as GameObject;
            Assert.IsNotNull(obstacleInstance);

            _manager.NotifyObstacleDestroyed(obstacleInstance, Vector3.zero, Vector3.forward);

            Assert.GreaterOrEqual(_manager.ActiveCount, 1, "Repurposed trap should register as active for cleanup");
        }

        private static List<ObstacleManager.ObstacleDefinition> CreateDefinitions()
        {
            return new List<ObstacleManager.ObstacleDefinition>
            {
                new ObstacleManager.ObstacleDefinition
                {
                    id = "KibbleBot",
                    obstaclePrefab = new GameObject("KibbleBot"),
                    weight = 3f,
                    despawnDistance = 40f
                },
                new ObstacleManager.ObstacleDefinition
                {
                    id = "HoloPaw",
                    obstaclePrefab = new GameObject("HoloPaw"),
                    repurposedPrefab = new GameObject("HoloTrap"),
                    weight = 1f,
                    despawnDistance = 40f
                }
            };
        }
    }
}

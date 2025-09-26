using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AngryDogs.Obstacles;
using AngryDogs.Systems;

namespace AngryDogs.Tests.EditMode
{
    public class ObstacleInteractionTests
    {
        private GameObject _root;
        private ObstacleManager _manager;
        private ObjectPooler _pooler;
        private Transform _player;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("ObstacleInteractionTests");
            _manager = _root.AddComponent<ObstacleManager>();
            _pooler = new GameObject("Pooler").AddComponent<ObjectPooler>();
            _player = new GameObject("Player").transform;

            // Inject private fields for testing
            typeof(ObstacleManager).GetField("pooler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_manager, _pooler);
            typeof(ObstacleManager).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_manager, _player);

            _manager.ConfigureObstacles(CreateTestDefinitions());
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
        public void KibbleVendingBot_ActivatesStickyPellets()
        {
            // Riley: "Time to test the kibble vending bot!"
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "KibbleVendingBot",
                obstaclePrefab = new GameObject("KibbleBot"),
                obstacleType = ObstacleManager.ObstacleType.KibbleVendingBot,
                effectDuration = 5f,
                effectRadius = 3f,
                weight = 1f,
                despawnDistance = 50f
            };

            _manager.ConfigureObstacles(new List<ObstacleManager.ObstacleDefinition> { definition });

            // Spawn the obstacle
            typeof(ObstacleManager).GetMethod("SpawnObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_manager, null);

            Assert.AreEqual(1, _manager.ActiveCount);

            // Get the spawned obstacle
            var activeObstacles = GetActiveObstacles();
            var obstacle = activeObstacles[0];

            // Activate the kibble vending bot
            _manager.HandleObstacleInteraction(obstacle, Vector3.zero, Vector3.forward);

            // The original obstacle should be removed
            Assert.AreEqual(0, _manager.ActiveCount, "Original obstacle should be removed after activation");
        }

        [Test]
        public void HoloPawPrint_CreatesDecoyTrail()
        {
            // Nibble: "Bark! (Translation: Test my holographic decoys!)"
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "HoloPawPrint",
                obstaclePrefab = new GameObject("HoloPaw"),
                obstacleType = ObstacleManager.ObstacleType.HoloPawPrint,
                effectDuration = 5f,
                effectRadius = 3f,
                weight = 1f,
                despawnDistance = 50f
            };

            _manager.ConfigureObstacles(new List<ObstacleManager.ObstacleDefinition> { definition });

            // Spawn the obstacle
            typeof(ObstacleManager).GetMethod("SpawnObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_manager, null);

            Assert.AreEqual(1, _manager.ActiveCount);

            // Get the spawned obstacle
            var activeObstacles = GetActiveObstacles();
            var obstacle = activeObstacles[0];

            // Activate the holo paw print
            _manager.HandleObstacleInteraction(obstacle, Vector3.zero, Vector3.forward);

            // The original obstacle should be removed
            Assert.AreEqual(0, _manager.ActiveCount, "Original obstacle should be removed after activation");
        }

        [Test]
        public void GetActiveObstaclesOfType_ReturnsCorrectObstacles()
        {
            // Riley: "Need to test obstacle type filtering!"
            var definitions = new List<ObstacleManager.ObstacleDefinition>
            {
                new ObstacleManager.ObstacleDefinition
                {
                    id = "KibbleBot1",
                    obstaclePrefab = new GameObject("KibbleBot1"),
                    obstacleType = ObstacleManager.ObstacleType.KibbleVendingBot,
                    weight = 1f,
                    despawnDistance = 50f
                },
                new ObstacleManager.ObstacleDefinition
                {
                    id = "HoloPaw1",
                    obstaclePrefab = new GameObject("HoloPaw1"),
                    obstacleType = ObstacleManager.ObstacleType.HoloPawPrint,
                    weight = 1f,
                    despawnDistance = 50f
                },
                new ObstacleManager.ObstacleDefinition
                {
                    id = "Standard1",
                    obstaclePrefab = new GameObject("Standard1"),
                    obstacleType = ObstacleManager.ObstacleType.Standard,
                    weight = 1f,
                    despawnDistance = 50f
                }
            };

            _manager.ConfigureObstacles(definitions);

            // Spawn one of each type
            for (int i = 0; i < 3; i++)
            {
                typeof(ObstacleManager).GetMethod("SpawnObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_manager, null);
            }

            Assert.AreEqual(3, _manager.ActiveCount);

            // Test filtering by type
            var kibbleBots = _manager.GetActiveObstaclesOfType(ObstacleManager.ObstacleType.KibbleVendingBot);
            var holoPaws = _manager.GetActiveObstaclesOfType(ObstacleManager.ObstacleType.HoloPawPrint);
            var standards = _manager.GetActiveObstaclesOfType(ObstacleManager.ObstacleType.Standard);

            Assert.AreEqual(1, kibbleBots.Count, "Should have 1 Kibble Vending Bot");
            Assert.AreEqual(1, holoPaws.Count, "Should have 1 Holo Paw Print");
            Assert.AreEqual(1, standards.Count, "Should have 1 Standard obstacle");
        }

        [Test]
        public void IsPositionAffectedByObstacle_DetectsNearbyObstacles()
        {
            // Nibble: "Bark! (Translation: Test if I can detect sticky spots!)"
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "KibbleBot",
                obstaclePrefab = new GameObject("KibbleBot"),
                obstacleType = ObstacleManager.ObstacleType.KibbleVendingBot,
                effectRadius = 2f,
                weight = 1f,
                despawnDistance = 50f
            };

            _manager.ConfigureObstacles(new List<ObstacleManager.ObstacleDefinition> { definition });

            // Spawn obstacle at origin
            typeof(ObstacleManager).GetMethod("SpawnObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_manager, null);

            // Test positions within and outside effect radius
            Assert.IsTrue(_manager.IsPositionAffectedByObstacle(Vector3.zero, ObstacleManager.ObstacleType.KibbleVendingBot, 2f), 
                "Position at origin should be affected");
            Assert.IsTrue(_manager.IsPositionAffectedByObstacle(Vector3.one, ObstacleManager.ObstacleType.KibbleVendingBot, 2f), 
                "Position within radius should be affected");
            Assert.IsFalse(_manager.IsPositionAffectedByObstacle(Vector3.one * 5f, ObstacleManager.ObstacleType.KibbleVendingBot, 2f), 
                "Position outside radius should not be affected");
        }

        [UnityTest]
        public IEnumerator StickyPelletComponent_DeactivatesAfterDuration()
        {
            // Riley: "Test if sticky pellets disappear after their duration!"
            var pellet = new GameObject("StickyPellet");
            var component = pellet.AddComponent<StickyPelletComponent>();
            
            component.Initialize(0.1f); // Short duration for testing
            
            Assert.IsTrue(pellet.activeInHierarchy, "Pellet should be active initially");
            
            yield return new WaitForSeconds(0.15f);
            
            Assert.IsFalse(pellet.activeInHierarchy, "Pellet should be deactivated after duration");
            
            Object.DestroyImmediate(pellet);
        }

        [UnityTest]
        public IEnumerator HoloPawPrintComponent_DeactivatesAfterDuration()
        {
            // Nibble: "Bark! (Translation: Test if my holograms fade away!)"
            var pawPrint = new GameObject("HoloPawPrint");
            var component = pawPrint.AddComponent<HoloPawPrintComponent>();
            
            component.Initialize(0.1f, 1); // Short duration for testing
            
            Assert.IsTrue(pawPrint.activeInHierarchy, "Paw print should be active initially");
            
            yield return new WaitForSeconds(0.15f);
            
            Assert.IsFalse(pawPrint.activeInHierarchy, "Paw print should be deactivated after duration");
            
            Object.DestroyImmediate(pawPrint);
        }

        private List<GameObject> GetActiveObstacles()
        {
            var result = new List<GameObject>();
            var activeObstaclesField = typeof(ObstacleManager).GetField("_activeObstacles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var activeObstacles = (System.Collections.IList)activeObstaclesField?.GetValue(_manager);
            
            if (activeObstacles != null)
            {
                foreach (var entry in activeObstacles)
                {
                    var instanceField = entry.GetType().GetField("Instance");
                    var instance = instanceField?.GetValue(entry) as GameObject;
                    if (instance != null)
                    {
                        result.Add(instance);
                    }
                }
            }
            
            return result;
        }

        private static List<ObstacleManager.ObstacleDefinition> CreateTestDefinitions()
        {
            return new List<ObstacleManager.ObstacleDefinition>
            {
                new ObstacleManager.ObstacleDefinition
                {
                    id = "TestKibbleBot",
                    obstaclePrefab = new GameObject("TestKibbleBot"),
                    obstacleType = ObstacleManager.ObstacleType.KibbleVendingBot,
                    weight = 1f,
                    despawnDistance = 50f
                },
                new ObstacleManager.ObstacleDefinition
                {
                    id = "TestHoloPaw",
                    obstaclePrefab = new GameObject("TestHoloPaw"),
                    obstacleType = ObstacleManager.ObstacleType.HoloPawPrint,
                    weight = 1f,
                    despawnDistance = 50f
                }
            };
        }
    }
}
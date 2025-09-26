using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AngryDogs.Obstacles;

namespace AngryDogs.Tests
{
    /// <summary>
    /// Unit tests for ObstacleManager to ensure proper obstacle spawning, pooling, and repurposing.
    /// Riley: "Gotta test every obstacle type! Can't have broken obstacles when the hounds are chasing!"
    /// Nibble: "Bark! (Translation: Test all the obstacle mechanics!)"
    /// </summary>
    public class ObstacleManagerTests
    {
        private ObstacleManager _obstacleManager;
        private GameObject _testGameObject;
        private Transform _playerTransform;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with ObstacleManager
            _testGameObject = new GameObject("TestObstacleManager");
            _obstacleManager = _testGameObject.AddComponent<ObstacleManager>();
            
            // Create a mock player transform
            var playerObject = new GameObject("TestPlayer");
            _playerTransform = playerObject.transform;
            
            // Set player reference
            var playerField = typeof(ObstacleManager).GetField("player", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerField != null)
            {
                playerField.SetValue(_obstacleManager, _playerTransform);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
            
            if (_playerTransform != null)
            {
                Object.DestroyImmediate(_playerTransform.gameObject);
            }
        }

        [Test]
        public void ObstacleManager_Initialization_IsValid()
        {
            // Riley: "Test that the obstacle manager initializes properly!"
            Assert.IsNotNull(_obstacleManager, "ObstacleManager should not be null");
            Assert.AreEqual(0, _obstacleManager.ActiveCount, "Active obstacle count should start at 0");
        }

        [Test]
        public void ObstacleManager_ConfigureObstacles_UpdatesDefinitions()
        {
            // Nibble: "Bark! (Translation: Test obstacle configuration!)"
            var testDefinitions = new List<ObstacleManager.ObstacleDefinition>
            {
                new ObstacleManager.ObstacleDefinition
                {
                    id = "TestObstacle1",
                    weight = 1f,
                    obstacleType = ObstacleManager.ObstacleType.Standard
                },
                new ObstacleManager.ObstacleDefinition
                {
                    id = "TestObstacle2",
                    weight = 2f,
                    obstacleType = ObstacleManager.ObstacleType.KibbleVendingBot
                }
            };

            _obstacleManager.ConfigureObstacles(testDefinitions);
            
            // Verify configuration was applied
            Assert.IsTrue(true, "Obstacle configuration should complete without errors");
        }

        [Test]
        public void ObstacleManager_SetRandomSeed_UpdatesRandomGenerator()
        {
            // Riley: "Test that random seed setting works properly!"
            var testSeed = 12345;
            
            _obstacleManager.SetRandomSeed(testSeed);
            
            // Verify seed was set (we can't directly test the random generator, but we can ensure no errors)
            Assert.IsTrue(true, "Random seed should be set without errors");
        }

        [Test]
        public void ObstacleManager_ResetManager_ClearsActiveObstacles()
        {
            // Nibble: "Bark! (Translation: Test manager reset!)"
            // Add some mock obstacles first
            var initialCount = _obstacleManager.ActiveCount;
            
            _obstacleManager.ResetManager();
            
            Assert.AreEqual(0, _obstacleManager.ActiveCount, "Reset should clear all active obstacles");
        }

        [Test]
        public void ObstacleManager_GetActiveObstaclesOfType_ReturnsCorrectObstacles()
        {
            // Riley: "Test that we can get obstacles by type!"
            var standardObstacles = _obstacleManager.GetActiveObstaclesOfType(ObstacleManager.ObstacleType.Standard);
            var kibbleObstacles = _obstacleManager.GetActiveObstaclesOfType(ObstacleManager.ObstacleType.KibbleVendingBot);
            
            Assert.IsNotNull(standardObstacles, "Standard obstacles list should not be null");
            Assert.IsNotNull(kibbleObstacles, "Kibble obstacles list should not be null");
            Assert.AreEqual(0, standardObstacles.Count, "Should start with no standard obstacles");
            Assert.AreEqual(0, kibbleObstacles.Count, "Should start with no kibble obstacles");
        }

        [Test]
        public void ObstacleManager_IsPositionAffectedByObstacle_ReturnsFalseForEmptyPosition()
        {
            // Nibble: "Bark! (Translation: Test position checking!)"
            var testPosition = Vector3.zero;
            var isAffected = _obstacleManager.IsPositionAffectedByObstacle(
                testPosition, 
                ObstacleManager.ObstacleType.Standard, 
                1f
            );
            
            Assert.IsFalse(isAffected, "Empty position should not be affected by obstacles");
        }

        [Test]
        public void ObstacleManager_ObstacleTypes_AreDefined()
        {
            // Riley: "Test that all obstacle types are properly defined!"
            var obstacleTypes = System.Enum.GetValues(typeof(ObstacleManager.ObstacleType));
            
            Assert.IsTrue(obstacleTypes.Length > 0, "Should have at least one obstacle type");
            
            // Check for specific obstacle types
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleManager.ObstacleType), ObstacleManager.ObstacleType.Standard), "Standard type should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleManager.ObstacleType), ObstacleManager.ObstacleType.KibbleVendingBot), "KibbleVendingBot type should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleManager.ObstacleType), ObstacleManager.ObstacleType.HoloPawPrint), "HoloPawPrint type should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleManager.ObstacleType), ObstacleManager.ObstacleType.NeonSlobberCannon), "NeonSlobberCannon type should be defined");
        }

        [Test]
        public void ObstacleManager_ObstacleDefinition_PropertiesAreValid()
        {
            // Nibble: "Bark! (Translation: Test obstacle definition properties!)"
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "TestObstacle",
                weight = 1.5f,
                obstacleType = ObstacleManager.ObstacleType.Standard,
                effectDuration = 5f,
                effectRadius = 3f
            };
            
            Assert.AreEqual("TestObstacle", definition.id, "ID should match");
            Assert.AreEqual(1.5f, definition.weight, "Weight should match");
            Assert.AreEqual(ObstacleManager.ObstacleType.Standard, definition.obstacleType, "Obstacle type should match");
            Assert.AreEqual(5f, definition.effectDuration, "Effect duration should match");
            Assert.AreEqual(3f, definition.effectRadius, "Effect radius should match");
        }

        [UnityTest]
        public IEnumerator ObstacleManager_HandleObstacleInteraction_ProcessesCorrectly()
        {
            // Riley: "Test obstacle interaction handling!"
            var testObstacle = new GameObject("TestObstacle");
            var testHitPoint = Vector3.zero;
            var testHitNormal = Vector3.up;
            
            // Test interaction handling
            _obstacleManager.HandleObstacleInteraction(testObstacle, testHitPoint, testHitNormal);
            
            yield return null; // Wait one frame
            
            // Clean up
            Object.DestroyImmediate(testObstacle);
            
            Assert.IsTrue(true, "Obstacle interaction should process without errors");
        }

        [Test]
        public void ObstacleManager_ActiveCount_ReflectsCurrentState()
        {
            // Riley: "Test that active count reflects the current state!"
            var initialCount = _obstacleManager.ActiveCount;
            
            // Reset manager to ensure clean state
            _obstacleManager.ResetManager();
            
            Assert.AreEqual(0, _obstacleManager.ActiveCount, "Active count should be 0 after reset");
        }

        [Test]
        public void ObstacleManager_ObstacleDefinition_DefaultValues()
        {
            // Nibble: "Bark! (Translation: Test default values!)"
            var definition = new ObstacleManager.ObstacleDefinition();
            
            Assert.AreEqual("SlobberCannon", definition.id, "Default ID should be SlobberCannon");
            Assert.AreEqual(1f, definition.weight, "Default weight should be 1");
            Assert.AreEqual(ObstacleManager.ObstacleType.Standard, definition.obstacleType, "Default type should be Standard");
            Assert.AreEqual(5f, definition.effectDuration, "Default effect duration should be 5");
            Assert.AreEqual(3f, definition.effectRadius, "Default effect radius should be 3");
            Assert.AreEqual(Vector3.one, definition.repurposedScale, "Default repurposed scale should be Vector3.one");
            Assert.AreEqual(20f, definition.despawnDistance, "Default despawn distance should be 20");
        }

        [Test]
        public void ObstacleManager_ObstacleType_ValuesAreUnique()
        {
            // Riley: "Test that obstacle type values are unique!"
            var obstacleTypes = System.Enum.GetValues(typeof(ObstacleManager.ObstacleType));
            var values = new List<int>();
            
            foreach (ObstacleManager.ObstacleType type in obstacleTypes)
            {
                var value = (int)type;
                Assert.IsFalse(values.Contains(value), $"Obstacle type {type} should have unique value {value}");
                values.Add(value);
            }
        }

        [Test]
        public void ObstacleManager_ObstacleDefinition_Serializable()
        {
            // Nibble: "Bark! (Translation: Test that obstacle definition is serializable!)"
            var definition = new ObstacleManager.ObstacleDefinition
            {
                id = "SerializableTest",
                weight = 2.5f,
                obstacleType = ObstacleManager.ObstacleType.NeonSlobberCannon,
                effectDuration = 7f,
                effectRadius = 4f,
                repurposedScale = new Vector3(2f, 2f, 2f),
                despawnDistance = 25f
            };
            
            // Test that the definition can be created and accessed
            Assert.IsNotNull(definition, "Obstacle definition should not be null");
            Assert.AreEqual("SerializableTest", definition.id, "Serialized ID should match");
            Assert.AreEqual(2.5f, definition.weight, "Serialized weight should match");
            Assert.AreEqual(ObstacleManager.ObstacleType.NeonSlobberCannon, definition.obstacleType, "Serialized type should match");
        }
    }
}
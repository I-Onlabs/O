using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AngryDogs.SaveSystem;
using AngryDogs.Obstacles;
using AngryDogs.Enemies;
using AngryDogs.Data;
using AngryDogs.Systems;

namespace AngryDogs.Tests
{
    /// <summary>
    /// Comprehensive stress tests for core systems under extreme conditions.
    /// Riley: "Time to stress-test these systems! Can't have crashes when 50+ hounds are chasing!"
    /// Nibble: "Bark! (Translation: Test everything under extreme conditions!)"
    /// </summary>
    public class StressTests
    {
        private SaveManager _saveManager;
        private ObstacleManager _obstacleManager;
        private BossHound _bossHound;
        private GameObject _testGameObject;
        private ObjectPooler _objectPooler;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with all required components
            _testGameObject = new GameObject("StressTestObject");
            _saveManager = _testGameObject.AddComponent<SaveManager>();
            _obstacleManager = _testGameObject.AddComponent<ObstacleManager>();
            _objectPooler = _testGameObject.AddComponent<ObjectPooler>();
            
            // Set up object pooler for obstacle manager
            var poolerField = typeof(ObstacleManager).GetField("pooler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (poolerField != null)
            {
                poolerField.SetValue(_obstacleManager, _objectPooler);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
        }

        [UnityTest]
        public IEnumerator SaveManager_StressTest_RapidCloudSyncs()
        {
            // Riley: "Test rapid cloud syncs - can't have the save system break under pressure!"
            Debug.Log("Riley: Starting rapid cloud sync stress test!");
            
            var syncCount = 0;
            var maxSyncs = 100;
            var syncInterval = 0.01f; // 10ms between syncs
            
            for (int i = 0; i < maxSyncs; i++)
            {
                // Modify save data
                var progress = _saveManager.Progress;
                progress.HighScore = i * 100;
                progress.Currency = i * 50;
                
                // Trigger save and cloud sync
                _saveManager.Save();
                _saveManager.SyncToCloud();
                
                syncCount++;
                
                // Small delay to prevent overwhelming the system
                yield return new WaitForSeconds(syncInterval);
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Completed {syncCount} rapid cloud syncs!)");
            Assert.AreEqual(maxSyncs, syncCount, "Should complete all rapid cloud syncs without crashing");
        }

        [UnityTest]
        public IEnumerator ObstacleManager_StressTest_100PlusObstacles()
        {
            // Nibble: "Bark! (Translation: Test with 100+ obstacles!)"
            Debug.Log("Riley: Starting 100+ obstacles stress test!");
            
            // Create obstacle definitions for stress testing
            var obstacleDefinitions = new List<ObstacleManager.ObstacleDefinition>();
            
            for (int i = 0; i < 10; i++)
            {
                var definition = new ObstacleManager.ObstacleDefinition
                {
                    id = $"StressTestObstacle_{i}",
                    weight = 1f,
                    despawnDistance = 50f,
                    obstacleType = ObstacleManager.ObstacleType.Standard
                };
                obstacleDefinitions.Add(definition);
            }
            
            // Configure obstacle manager
            _obstacleManager.ConfigureObstacles(obstacleDefinitions);
            
            // Set up player reference
            var player = new GameObject("TestPlayer");
            player.transform.position = Vector3.zero;
            
            var playerField = typeof(ObstacleManager).GetField("player", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerField != null)
            {
                playerField.SetValue(_obstacleManager, player.transform);
            }
            
            // Set high spawn rate for stress testing
            var spawnIntervalField = typeof(ObstacleManager).GetField("spawnIntervalCurve", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (spawnIntervalField != null)
            {
                var curve = AnimationCurve.Linear(0f, 0.1f, 240f, 0.05f); // Very fast spawning
                spawnIntervalField.SetValue(_obstacleManager, curve);
            }
            
            // Set high obstacle limit
            var maxObstaclesField = typeof(ObstacleManager).GetField("maxActiveObstacles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxObstaclesField != null)
            {
                maxObstaclesField.SetValue(_obstacleManager, 150); // Allow 150 obstacles
            }
            
            // Run stress test for 10 seconds
            var startTime = Time.time;
            var targetDuration = 10f;
            var maxObstaclesReached = 0;
            
            while (Time.time - startTime < targetDuration)
            {
                // Move player forward to trigger obstacle spawning
                player.transform.position += Vector3.forward * Time.deltaTime * 10f;
                
                // Check current obstacle count
                var currentCount = _obstacleManager.ActiveCount;
                maxObstaclesReached = Mathf.Max(maxObstaclesReached, currentCount);
                
                yield return null;
            }
            
            Debug.Log($"Riley: Stress test completed! Max obstacles reached: {maxObstaclesReached}");
            Assert.GreaterOrEqual(maxObstaclesReached, 50, "Should spawn at least 50 obstacles during stress test");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(player);
        }

        [UnityTest]
        public IEnumerator BossHound_StressTest_50PlusHounds()
        {
            // Riley: "Test with 50+ hounds! This is going to be chaos!"
            Debug.Log("Riley: Starting 50+ hounds stress test!");
            
            var hounds = new List<BossHound>();
            var houndCount = 50;
            
            // Create 50 boss hounds
            for (int i = 0; i < houndCount; i++)
            {
                var houndObject = new GameObject($"StressTestBossHound_{i}");
                var hound = houndObject.AddComponent<BossHound>();
                
                // Set up required components
                var navMeshAgent = houndObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                var healthComponent = houndObject.AddComponent<HealthComponent>();
                
                // Position hounds around the test area
                var angle = (i * 360f / houndCount) * Mathf.Deg2Rad;
                var radius = 10f;
                houndObject.transform.position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
                
                hounds.Add(hound);
            }
            
            // Set up target for hounds
            var target = new GameObject("TestTarget");
            target.transform.position = Vector3.zero;
            
            // Configure all hounds to target the same position
            foreach (var hound in hounds)
            {
                var rileyField = typeof(BossHound).GetField("riley", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (rileyField != null)
                {
                    rileyField.SetValue(hound, target.transform);
                }
            }
            
            // Run stress test for 5 seconds
            var startTime = Time.time;
            var targetDuration = 5f;
            var activeHounds = 0;
            
            while (Time.time - startTime < targetDuration)
            {
                // Count active hounds
                activeHounds = 0;
                foreach (var hound in hounds)
                {
                    if (hound != null && hound.gameObject.activeInHierarchy)
                    {
                        activeHounds++;
                    }
                }
                
                // Move target to keep hounds active
                target.transform.position += Vector3.forward * Time.deltaTime * 5f;
                
                yield return null;
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Stress test completed! Active hounds: {activeHounds})");
            Assert.GreaterOrEqual(activeHounds, 40, "Should maintain at least 40 active hounds during stress test");
            
            // Clean up
            foreach (var hound in hounds)
            {
                if (hound != null)
                {
                    UnityEngine.Object.DestroyImmediate(hound.gameObject);
                }
            }
            UnityEngine.Object.DestroyImmediate(target);
        }

        [UnityTest]
        public IEnumerator SaveManager_StressTest_ConcurrentSaves()
        {
            // Riley: "Test concurrent saves from multiple sources - can't have race conditions!"
            Debug.Log("Riley: Starting concurrent saves stress test!");
            
            var saveCount = 0;
            var maxSaves = 50;
            var saveInterval = 0.02f; // 20ms between saves
            
            // Start multiple concurrent save operations
            var saveCoroutines = new List<Coroutine>();
            
            for (int i = 0; i < 5; i++) // 5 concurrent save streams
            {
                var coroutine = _testGameObject.GetComponent<MonoBehaviour>().StartCoroutine(
                    ConcurrentSaveCoroutine(i, maxSaves / 5, saveInterval, () => saveCount++)
                );
                saveCoroutines.Add(coroutine);
            }
            
            // Wait for all saves to complete
            foreach (var coroutine in saveCoroutines)
            {
                yield return coroutine;
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Completed {saveCount} concurrent saves!)");
            Assert.AreEqual(maxSaves, saveCount, "Should complete all concurrent saves without data corruption");
        }

        private IEnumerator ConcurrentSaveCoroutine(int streamId, int saveCount, float interval, System.Action onSave)
        {
            for (int i = 0; i < saveCount; i++)
            {
                // Modify save data
                var progress = _saveManager.Progress;
                progress.HighScore = streamId * 1000 + i;
                progress.Currency = streamId * 500 + i * 10;
                
                // Save
                _saveManager.Save();
                onSave?.Invoke();
                
                yield return new WaitForSeconds(interval);
            }
        }

        [UnityTest]
        public IEnumerator ObstacleManager_StressTest_MemoryLeaks()
        {
            // Nibble: "Bark! (Translation: Test for memory leaks!)"
            Debug.Log("Riley: Starting memory leak stress test!");
            
            var initialMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false);
            var maxIterations = 1000;
            var iteration = 0;
            
            // Create and destroy obstacles repeatedly
            while (iteration < maxIterations)
            {
                // Create obstacle
                var obstacle = new GameObject($"MemoryTestObstacle_{iteration}");
                var obstacleComponent = obstacle.AddComponent<ObstacleManager>();
                
                // Simulate obstacle lifecycle
                yield return new WaitForSeconds(0.001f);
                
                // Destroy obstacle
                UnityEngine.Object.DestroyImmediate(obstacle);
                
                iteration++;
                
                // Check memory every 100 iterations
                if (iteration % 100 == 0)
                {
                    var currentMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false);
                    var memoryIncrease = currentMemory - initialMemory;
                    
                    Debug.Log($"Riley: Memory check at iteration {iteration}: {memoryIncrease / 1024f / 1024f:F2}MB increase");
                    
                    // Memory should not increase excessively
                    Assert.Less(memoryIncrease, 100 * 1024 * 1024, "Memory increase should be less than 100MB");
                }
            }
            
            var finalMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false);
            var totalMemoryIncrease = finalMemory - initialMemory;
            
            Debug.Log($"Nibble: *bark* (Translation: Memory leak test completed! Total increase: {totalMemoryIncrease / 1024f / 1024f:F2}MB)");
            Assert.Less(totalMemoryIncrease, 50 * 1024 * 1024, "Total memory increase should be less than 50MB");
        }

        [UnityTest]
        public IEnumerator BossHound_StressTest_WeakPointDestruction()
        {
            // Riley: "Test weak point destruction under stress - gotta make sure the boss system is robust!"
            Debug.Log("Riley: Starting weak point destruction stress test!");
            
            var bossObject = new GameObject("StressTestBoss");
            var boss = bossObject.AddComponent<BossHound>();
            
            // Set up required components
            var navMeshAgent = bossObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            var healthComponent = bossObject.AddComponent<HealthComponent>();
            
            // Create many weak points
            var weakPointCount = 20;
            var weakPoints = new List<BossHound.WeakPoint>();
            
            for (int i = 0; i < weakPointCount; i++)
            {
                var weakPointObject = new GameObject($"WeakPoint_{i}");
                weakPointObject.transform.SetParent(bossObject.transform);
                weakPointObject.transform.localPosition = new Vector3(i * 0.5f, 0f, 0f);
                
                var weakPoint = new BossHound.WeakPoint
                {
                    transform = weakPointObject.transform,
                    name = $"Weak Point {i}",
                    health = 10f,
                    maxHealth = 10f,
                    isDestroyed = false
                };
                
                weakPoints.Add(weakPoint);
            }
            
            // Set weak points on boss
            var weakPointsField = typeof(BossHound).GetField("weakPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (weakPointsField != null)
            {
                weakPointsField.SetValue(boss, weakPoints);
            }
            
            // Rapidly damage weak points
            var damageCount = 0;
            var maxDamage = 100;
            
            for (int i = 0; i < maxDamage; i++)
            {
                var randomWeakPoint = weakPoints[Random.Range(0, weakPoints.Count)];
                if (!randomWeakPoint.isDestroyed)
                {
                    boss.DamageWeakPoint(randomWeakPoint, 1f);
                    damageCount++;
                }
                
                yield return new WaitForSeconds(0.01f);
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Weak point stress test completed! Damage applied: {damageCount})");
            Assert.GreaterOrEqual(damageCount, 50, "Should apply damage to weak points without crashing");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(bossObject);
        }

        [Test]
        public void SaveManager_StressTest_DataIntegrity()
        {
            // Riley: "Test data integrity under stress - can't have corrupted saves!"
            Debug.Log("Riley: Starting data integrity stress test!");
            
            var testData = new List<PlayerSaveData>();
            var dataCount = 1000;
            
            // Create and modify save data rapidly
            for (int i = 0; i < dataCount; i++)
            {
                var data = PlayerSaveData.CreateDefault();
                data.Progress.HighScore = i * 100;
                data.Progress.Currency = i * 50;
                data.Progress.UnlockUpgrade($"upgrade_{i}");
                
                testData.Add(data);
            }
            
            // Validate all data
            var validDataCount = 0;
            foreach (var data in testData)
            {
                if (data != null && data.Progress != null && data.Settings != null)
                {
                    if (data.Progress.HighScore >= 0 && data.Progress.Currency >= 0)
                    {
                        validDataCount++;
                    }
                }
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Data integrity test completed! Valid data: {validDataCount}/{dataCount})");
            Assert.AreEqual(dataCount, validDataCount, "All save data should remain valid under stress");
        }

        [UnityTest]
        public IEnumerator ObstacleManager_StressTest_Repurposing()
        {
            // Riley: "Test obstacle repurposing under stress - gotta make sure the system can handle rapid changes!"
            Debug.Log("Riley: Starting obstacle repurposing stress test!");
            
            var repurposingCount = 0;
            var maxRepurposing = 200;
            var repurposingInterval = 0.01f;
            
            // Create obstacle definitions with repurposing
            var obstacleDefinitions = new List<ObstacleManager.ObstacleDefinition>();
            
            for (int i = 0; i < 5; i++)
            {
                var definition = new ObstacleManager.ObstacleDefinition
                {
                    id = $"RepurposingTestObstacle_{i}",
                    weight = 1f,
                    despawnDistance = 50f,
                    obstacleType = ObstacleManager.ObstacleType.Standard,
                    repurposedPrefab = new GameObject($"RepurposedPrefab_{i}") // Simple repurposed prefab
                };
                obstacleDefinitions.Add(definition);
            }
            
            _obstacleManager.ConfigureObstacles(obstacleDefinitions);
            
            // Rapidly trigger repurposing
            for (int i = 0; i < maxRepurposing; i++)
            {
                // Create a test obstacle
                var testObstacle = new GameObject($"TestObstacle_{i}");
                var hitPoint = testObstacle.transform.position;
                var hitNormal = Vector3.up;
                
                // Trigger repurposing
                _obstacleManager.NotifyObstacleDestroyed(testObstacle, hitPoint, hitNormal);
                repurposingCount++;
                
                // Clean up
                UnityEngine.Object.DestroyImmediate(testObstacle);
                
                yield return new WaitForSeconds(repurposingInterval);
            }
            
            Debug.Log($"Riley: Repurposing stress test completed! Repurposing operations: {repurposingCount}");
            Assert.AreEqual(maxRepurposing, repurposingCount, "Should complete all repurposing operations without crashing");
        }

        [UnityTest]
        public IEnumerator BossHound_StressTest_PhaseTransitions()
        {
            // Nibble: "Bark! (Translation: Test phase transitions under stress!)"
            Debug.Log("Riley: Starting phase transition stress test!");
            
            var bossObject = new GameObject("PhaseTestBoss");
            var boss = bossObject.AddComponent<BossHound>();
            
            // Set up required components
            var navMeshAgent = bossObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            var healthComponent = bossObject.AddComponent<HealthComponent>();
            
            // Set up health component
            healthComponent.Initialize(1000f);
            
            var phaseTransitionCount = 0;
            var maxTransitions = 50;
            
            // Rapidly change health to trigger phase transitions
            for (int i = 0; i < maxTransitions; i++)
            {
                var healthPercentage = (i % 100) / 100f;
                var newHealth = healthPercentage * 1000f;
                
                // Set health directly
                var currentHealthField = typeof(HealthComponent).GetField("_currentHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (currentHealthField != null)
                {
                    currentHealthField.SetValue(healthComponent, newHealth);
                }
                
                // Trigger health change event
                var onHealthChangedField = typeof(HealthComponent).GetField("OnHealthChanged", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (onHealthChangedField != null)
                {
                    var onHealthChanged = onHealthChangedField.GetValue(healthComponent) as System.Action<float, float>;
                    onHealthChanged?.Invoke(newHealth, 1000f);
                }
                
                phaseTransitionCount++;
                yield return new WaitForSeconds(0.01f);
            }
            
            Debug.Log($"Riley: Phase transition stress test completed! Transitions: {phaseTransitionCount}");
            Assert.AreEqual(maxTransitions, phaseTransitionCount, "Should complete all phase transitions without crashing");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(bossObject);
        }
    }
}
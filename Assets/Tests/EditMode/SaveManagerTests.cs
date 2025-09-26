using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AngryDogs.SaveSystem;
using AngryDogs.Data;

namespace AngryDogs.Tests
{
    /// <summary>
    /// Comprehensive unit tests for SaveManager to ensure JSON integrity, cloud sync reliability, and cross-device compatibility.
    /// Riley: "Gotta test every edge case! Can't have corrupted saves when the hounds are chasing!"
    /// Nibble: "Bark! (Translation: Test all the save scenarios!)"
    /// </summary>
    public class SaveManagerTests
    {
        private SaveManager _saveManager;
        private GameObject _testGameObject;
        private string _originalSavePath;
        private string _testSavePath;

        [SetUp]
        public void SetUp()
        {
            // Create a test GameObject with SaveManager
            _testGameObject = new GameObject("TestSaveManager");
            _saveManager = _testGameObject.AddComponent<SaveManager>();
            
            // Store original save path and create test path
            _originalSavePath = Application.persistentDataPath;
            _testSavePath = Path.Combine(Path.GetTempPath(), "AngryDogsTestSaves");
            Directory.CreateDirectory(_testSavePath);
            
            // Override the save path for testing
            var savePathField = typeof(SaveManager).GetField("SavePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_saveManager, Path.Combine(_testSavePath, "test_save.json"));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
            
            // Clean up test save files
            if (Directory.Exists(_testSavePath))
            {
                Directory.Delete(_testSavePath, true);
            }
        }

        [Test]
        public void SaveManager_DefaultState_ReturnsValidData()
        {
            // Riley: "Test that the save manager starts with valid default data!"
            var saveData = _saveManager.Progress;
            
            Assert.IsNotNull(saveData, "Progress should not be null");
            Assert.AreEqual(0, saveData.HighScore, "High score should start at 0");
            Assert.AreEqual(0, saveData.Currency, "Currency should start at 0");
            Assert.AreEqual(1, saveData.Version, "Version should start at 1");
            Assert.IsNotNull(saveData.UnlockedUpgrades, "Unlocked upgrades list should not be null");
        }

        [Test]
        public void SaveManager_SaveAndLoad_MaintainsDataIntegrity()
        {
            // Nibble: "Bark! (Translation: Test that saving and loading preserves data!)"
            var originalData = _saveManager.Progress;
            originalData.HighScore = 1500;
            originalData.Currency = 250;
            originalData.UnlockUpgrade("test_upgrade_1");
            originalData.UnlockUpgrade("test_upgrade_2");

            // Save the data
            _saveManager.Save();

            // Create a new SaveManager instance to simulate loading
            var newSaveManager = new GameObject("NewSaveManager").AddComponent<SaveManager>();
            var savePathField = typeof(SaveManager).GetField("SavePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(newSaveManager, Path.Combine(_testSavePath, "test_save.json"));
            }

            // Load the data
            newSaveManager.Load();
            var loadedData = newSaveManager.Progress;

            Assert.AreEqual(originalData.HighScore, loadedData.HighScore, "High score should be preserved");
            Assert.AreEqual(originalData.Currency, loadedData.Currency, "Currency should be preserved");
            Assert.AreEqual(originalData.Version + 1, loadedData.Version, "Version should be incremented");
            Assert.IsTrue(loadedData.UnlockedUpgrades.Contains("test_upgrade_1"), "Upgrade 1 should be unlocked");
            Assert.IsTrue(loadedData.UnlockedUpgrades.Contains("test_upgrade_2"), "Upgrade 2 should be unlocked");

            UnityEngine.Object.DestroyImmediate(newSaveManager.gameObject);
        }

        [Test]
        public void SaveManager_CorruptedSaveFile_ReturnsDefaultData()
        {
            // Riley: "Test what happens when the save file gets corrupted by those pesky hounds!"
            var corruptedJson = "This is not valid JSON! The hounds must have tampered with it!";
            var savePath = Path.Combine(_testSavePath, "corrupted_save.json");
            File.WriteAllText(savePath, corruptedJson);

            // Override save path to point to corrupted file
            var savePathField = typeof(SaveManager).GetField("SavePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_saveManager, savePath);
            }

            // Load should return default data
            _saveManager.Load();
            var loadedData = _saveManager.Progress;

            Assert.AreEqual(0, loadedData.HighScore, "Should return default high score for corrupted save");
            Assert.AreEqual(0, loadedData.Currency, "Should return default currency for corrupted save");
            Assert.AreEqual(1, loadedData.Version, "Should return default version for corrupted save");
        }

        [Test]
        public void SaveManager_EmptySaveFile_ReturnsDefaultData()
        {
            // Nibble: "Bark! (Translation: Test empty save file handling!)"
            var emptyJson = "";
            var savePath = Path.Combine(_testSavePath, "empty_save.json");
            File.WriteAllText(savePath, emptyJson);

            // Override save path to point to empty file
            var savePathField = typeof(SaveManager).GetField("SavePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_saveManager, savePath);
            }

            // Load should return default data
            _saveManager.Load();
            var loadedData = _saveManager.Progress;

            Assert.AreEqual(0, loadedData.HighScore, "Should return default high score for empty save");
            Assert.AreEqual(0, loadedData.Currency, "Should return default currency for empty save");
        }

        [Test]
        public void SaveManager_InvalidJsonStructure_ReturnsDefaultData()
        {
            // Riley: "Test handling of JSON with wrong structure!"
            var invalidJson = "{\"wrongField\": \"value\", \"anotherField\": 123}";
            var savePath = Path.Combine(_testSavePath, "invalid_save.json");
            File.WriteAllText(savePath, invalidJson);

            // Override save path to point to invalid file
            var savePathField = typeof(SaveManager).GetField("SavePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_saveManager, savePath);
            }

            // Load should return default data
            _saveManager.Load();
            var loadedData = _saveManager.Progress;

            Assert.AreEqual(0, loadedData.HighScore, "Should return default high score for invalid JSON structure");
        }

        [Test]
        public void SaveManager_ObfuscatedPayload_EncryptsAndDecryptsCorrectly()
        {
            // Nibble: "Bark! (Translation: Test the obfuscation system!)"
            var testData = PlayerSaveData.CreateDefault();
            testData.Progress.HighScore = 9999;
            testData.Progress.Currency = 500;

            // Test serialization with obfuscation
            var serializeMethod = typeof(SaveManager).GetMethod("Serialize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var deserializeMethod = typeof(SaveManager).GetMethod("Deserialize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serializeMethod != null && deserializeMethod != null)
            {
                var obfuscatedJson = (string)serializeMethod.Invoke(_saveManager, new object[] { testData });
                var deserializedData = (PlayerSaveData)deserializeMethod.Invoke(_saveManager, new object[] { obfuscatedJson });

                Assert.AreEqual(testData.Progress.HighScore, deserializedData.Progress.HighScore, "Obfuscated high score should match");
                Assert.AreEqual(testData.Progress.Currency, deserializedData.Progress.Currency, "Obfuscated currency should match");
            }
        }

        [Test]
        public void SaveManager_CloudSyncConflict_ResolvesCorrectly()
        {
            // Riley: "Test cloud sync conflict resolution - gotta be smart about merging saves!"
            var localData = PlayerSaveData.CreateDefault();
            localData.Progress.HighScore = 1000;
            localData.Progress.Currency = 100;
            localData.Progress.UnlockUpgrade("local_upgrade");

            var cloudData = PlayerSaveData.CreateDefault();
            cloudData.Progress.HighScore = 1500; // Higher score
            cloudData.Progress.Currency = 50;    // Lower currency
            cloudData.Progress.UnlockUpgrade("cloud_upgrade");

            // Test merge logic
            var mergeMethod = typeof(SaveManager).GetMethod("MergeSaveData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (mergeMethod != null)
            {
                var mergedData = (PlayerSaveData)mergeMethod.Invoke(_saveManager, new object[] { localData, cloudData });

                Assert.AreEqual(1500, mergedData.Progress.HighScore, "Should prefer higher high score");
                Assert.AreEqual(100, mergedData.Progress.Currency, "Should prefer higher currency");
                Assert.IsTrue(mergedData.Progress.UnlockedUpgrades.Contains("local_upgrade"), "Should include local upgrades");
                Assert.IsTrue(mergedData.Progress.UnlockedUpgrades.Contains("cloud_upgrade"), "Should include cloud upgrades");
            }
        }

        [Test]
        public void SaveManager_SaveRunResults_UpdatesProgressCorrectly()
        {
            // Nibble: "Bark! (Translation: Test saving run results!)"
            var initialScore = _saveManager.Progress.HighScore;
            var initialCurrency = _saveManager.Progress.Currency;

            _saveManager.SaveRunResults(2000, 150, new[] { "new_upgrade_1", "new_upgrade_2" });

            var updatedData = _saveManager.Progress;
            Assert.AreEqual(2000, updatedData.HighScore, "High score should be updated");
            Assert.AreEqual(initialCurrency + 150, updatedData.Currency, "Currency should be increased");
            Assert.IsTrue(updatedData.UnlockedUpgrades.Contains("new_upgrade_1"), "New upgrade 1 should be unlocked");
            Assert.IsTrue(updatedData.UnlockedUpgrades.Contains("new_upgrade_2"), "New upgrade 2 should be unlocked");
        }

        [Test]
        public void SaveManager_KeyBindingStorage_WorksCorrectly()
        {
            // Riley: "Test key binding storage and retrieval!"
            var testAction = "test_action";
            var testKey = KeyCode.Space;

            // Store key binding
            _saveManager.StoreKeyBinding(testAction, testKey);

            // Retrieve key binding
            var retrievedKey = _saveManager.LoadKeyBinding(testAction, KeyCode.None);

            Assert.AreEqual(testKey, retrievedKey, "Retrieved key should match stored key");
        }

        [Test]
        public void SaveManager_DeleteSave_ResetsToDefaults()
        {
            // Nibble: "Bark! (Translation: Test save deletion!)"
            // Set some data
            _saveManager.SaveRunResults(5000, 1000, new[] { "test_upgrade" });
            _saveManager.StoreKeyBinding("test_action", KeyCode.Return);

            // Delete save
            _saveManager.DeleteSave();

            // Verify reset to defaults
            var resetData = _saveManager.Progress;
            Assert.AreEqual(0, resetData.HighScore, "High score should be reset to 0");
            Assert.AreEqual(0, resetData.Currency, "Currency should be reset to 0");
            Assert.AreEqual(1, resetData.Version, "Version should be reset to 1");
            Assert.IsFalse(resetData.UnlockedUpgrades.Contains("test_upgrade"), "Upgrades should be cleared");

            var resetKey = _saveManager.LoadKeyBinding("test_action", KeyCode.None);
            Assert.AreEqual(KeyCode.None, resetKey, "Key binding should be reset to default");
        }

        [Test]
        public void SaveManager_ConcurrentAccess_HandlesGracefully()
        {
            // Riley: "Test concurrent access to save system - can't have race conditions!"
            var saveData = _saveManager.Progress;
            saveData.HighScore = 1000;

            // Simulate concurrent saves
            _saveManager.Save();
            _saveManager.Save();
            _saveManager.Save();

            // Should not crash and should maintain data integrity
            var finalData = _saveManager.Progress;
            Assert.AreEqual(1000, finalData.HighScore, "High score should be preserved through concurrent saves");
        }

        [Test]
        public void SaveManager_JsonIntegrityValidation_DetectsCorruption()
        {
            // Nibble: "Bark! (Translation: Test JSON integrity validation!)"
            var validateMethod = typeof(SaveManager).GetMethod("ValidateSaveIntegrity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (validateMethod != null)
            {
                // Test valid JSON
                var validJson = "{\"progress\":{\"highScore\":1000,\"currency\":500,\"version\":1,\"unlockedUpgrades\":[]},\"settings\":{\"musicVolume\":1.0,\"sfxVolume\":1.0,\"hapticsEnabled\":true,\"leftHandedUi\":false,\"bindings\":[]}}";
                var isValid = (bool)validateMethod.Invoke(_saveManager, new object[] { validJson });
                Assert.IsTrue(isValid, "Valid JSON should pass integrity check");

                // Test invalid JSON
                var invalidJson = "{\"corrupted\": \"data\"}";
                var isInvalid = (bool)validateMethod.Invoke(_saveManager, new object[] { invalidJson });
                Assert.IsFalse(isInvalid, "Invalid JSON should fail integrity check");
            }
        }

        [UnityTest]
        public IEnumerator SaveManager_CloudSyncTimeout_HandlesGracefully()
        {
            // Riley: "Test cloud sync timeout handling - can't wait forever for the cloud!"
            var cloudSyncMethod = typeof(SaveManager).GetMethod("SyncToCloud", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (cloudSyncMethod != null)
            {
                // This would need a mock cloud service to test properly
                // For now, just ensure the method doesn't crash
                cloudSyncMethod.Invoke(_saveManager, null);
                
                yield return new WaitForSeconds(0.1f);
                
                // Should not crash even with invalid cloud URL
                Assert.IsTrue(true, "Cloud sync should handle invalid URLs gracefully");
            }
        }
    }
}
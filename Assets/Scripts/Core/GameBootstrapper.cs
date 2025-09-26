using UnityEngine;
using AngryDogs.Input;
using AngryDogs.Systems;

namespace AngryDogs.Core
{
    /// <summary>
    /// Configurable bootstrapper. Attach to a persistent prefab in the first scene.
    /// Responsible for instantiating core services and keeping them alive across scenes.
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [Header("Service Prefabs")]
        [SerializeField] private PlayerInputHandler inputHandlerPrefab;
        [SerializeField] private ObjectPooler objectPoolerPrefab;

        [Header("Optional Services")]
        [SerializeField] private SaveSystem saveSystemPrefab;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RegisterService(inputHandlerPrefab);
            RegisterService(objectPoolerPrefab);

            if (saveSystemPrefab != null)
            {
                RegisterService(saveSystemPrefab);
            }
        }

        private static void RegisterService<T>(T prefab) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogWarning($"GameBootstrapper is missing a reference to {typeof(T).Name}.");
                return;
            }

            if (FindObjectOfType<T>() != null)
            {
                // Already present in the scene (e.g., manual placement for debugging)
                return;
            }

            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
        }
    }
}

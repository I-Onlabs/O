using UnityEngine;

namespace AngryDogs.Data
{
    /// <summary>
    /// ScriptableObject describing an upgrade purchasable in the shop.
    /// </summary>
    [CreateAssetMenu(menuName = "AngryDogs/Upgrade Definition", fileName = "UpgradeDefinition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private int cost;
        [SerializeField] private Sprite icon;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public string Description => description;
        public int Cost => cost;
        public Sprite Icon => icon;
    }
}

using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Enemies;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "EnemySpawnConfig", menuName = ConfigAssetMenus.EnemySpawn)]
	public sealed class EnemySpawnConfigAsset : ScriptableObject
	{
		[SerializeField] int minPolarBearCount = 1;
		[SerializeField] int maxPolarBearCount = 3;

		public EnemySpawnConfig ToConfig() =>
			new()
			{
				MinPolarBearCount = minPolarBearCount,
				MaxPolarBearCount = maxPolarBearCount
			};

		public void ApplyCodeDefaults()
		{
			var defaults = GameSessionDefaults.Default.Enemies;
			minPolarBearCount = defaults.MinPolarBearCount;
			maxPolarBearCount = defaults.MaxPolarBearCount;
		}
	}
}

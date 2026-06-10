using Game.Systems.Integration.Bootstrap;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "GameSessionProfile", menuName = ConfigAssetMenus.SessionProfile)]
	public sealed class GameSessionProfileAsset : ScriptableObject
	{
		[SerializeField] WorldConfigAsset world;
		[SerializeField] TerrainConfigAsset terrain;
		[SerializeField] PlayerConfigAsset player;
		[SerializeField] EnemySpawnConfigAsset enemies;
		[SerializeField] PolarBearConfigAsset polarBear;
		[SerializeField] TopDownCameraConfigAsset camera;
		[SerializeField] DebugInputConfigAsset debug;

		public WorldConfigAsset World
		{
			get => world;
			set => world = value;
		}

		public TerrainConfigAsset Terrain
		{
			get => terrain;
			set => terrain = value;
		}

		public PlayerConfigAsset Player
		{
			get => player;
			set => player = value;
		}

		public EnemySpawnConfigAsset Enemies
		{
			get => enemies;
			set => enemies = value;
		}

		public PolarBearConfigAsset PolarBear
		{
			get => polarBear;
			set => polarBear = value;
		}

		public TopDownCameraConfigAsset Camera
		{
			get => camera;
			set => camera = value;
		}

		public DebugInputConfigAsset Debug
		{
			get => debug;
			set => debug = value;
		}

		public GameSessionConfig ToSessionConfig()
		{
			if (world == null || terrain == null || player == null ||
			    enemies == null || polarBear == null || camera == null)
			{
				UnityEngine.Debug.LogWarning(
					$"{name} is missing config references. Falling back to {nameof(GameSessionDefaults.Default)} for missing entries.");
			}

			var defaults = GameSessionDefaults.Default;
			return new GameSessionConfig
			{
				World = world != null ? world.ToConfig() : defaults.World,
				Terrain = terrain != null ? terrain.ToConfig() : defaults.Terrain,
				Player = player != null ? player.ToConfig() : defaults.Player,
				Enemies = enemies != null ? enemies.ToConfig() : defaults.Enemies,
				PolarBear = polarBear != null ? polarBear.ToConfig() : defaults.PolarBear,
				Camera = camera != null ? camera.ToConfig() : defaults.Camera
			};
		}

		public DebugInputSettings ToDebugSettings() =>
			debug != null ? debug.ToSettings() : DebugInputSettings.Default;
	}
}

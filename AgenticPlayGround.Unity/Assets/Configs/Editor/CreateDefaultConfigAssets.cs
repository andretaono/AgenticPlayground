#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.UnityBridge.Configs.Editor
{
	public static class CreateDefaultConfigAssets
	{
		private const string ConfigRoot = "Assets/Configs";

		[MenuItem("Game/Configs/Create Default Config Assets")]
		public static void CreateAll()
		{
			EnsureFolder(ConfigRoot);
			EnsureFolder($"{ConfigRoot}/World");
			EnsureFolder($"{ConfigRoot}/Terrain");
			EnsureFolder($"{ConfigRoot}/Player");
			EnsureFolder($"{ConfigRoot}/Enemies");
			EnsureFolder($"{ConfigRoot}/Presentation");
			EnsureFolder($"{ConfigRoot}/Debug");
			EnsureFolder($"{ConfigRoot}/Session");

			var world = CreateAsset<WorldConfigAsset>($"{ConfigRoot}/World/DefaultWorldConfig.asset");
			var terrain = CreateAsset<TerrainConfigAsset>($"{ConfigRoot}/Terrain/DefaultTerrainConfig.asset");
			var player = CreateAsset<PlayerConfigAsset>($"{ConfigRoot}/Player/DefaultPlayerConfig.asset");
			var enemies = CreateAsset<EnemySpawnConfigAsset>($"{ConfigRoot}/Enemies/DefaultEnemySpawnConfig.asset");
			var polarBear = CreateAsset<PolarBearConfigAsset>($"{ConfigRoot}/Enemies/DefaultPolarBearConfig.asset");
			var camera = CreateAsset<TopDownCameraConfigAsset>($"{ConfigRoot}/Presentation/DefaultTopDownCameraConfig.asset");
			var debug = CreateAsset<DebugInputConfigAsset>($"{ConfigRoot}/Debug/DefaultDebugInputConfig.asset");

			var profilePath = $"{ConfigRoot}/Session/DefaultGameSessionProfile.asset";
			var profile = AssetDatabase.LoadAssetAtPath<GameSessionProfileAsset>(profilePath);
			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<GameSessionProfileAsset>();
				AssetDatabase.CreateAsset(profile, profilePath);
			}

			profile.World = world;
			profile.Terrain = terrain;
			profile.Player = player;
			profile.Enemies = enemies;
			profile.PolarBear = polarBear;
			profile.Camera = camera;
			profile.Debug = debug;

			EditorUtility.SetDirty(profile);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Selection.activeObject = profile;
			Debug.Log($"Created default config assets under {ConfigRoot}. Assign DefaultGameSessionProfile to GameSessionBoot.");
		}

		private static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			var existing = AssetDatabase.LoadAssetAtPath<T>(path);
			if (existing != null)
			{
				ApplyDefaults(existing);
				EditorUtility.SetDirty(existing);
				return existing;
			}

			var asset = ScriptableObject.CreateInstance<T>();
			ApplyDefaults(asset);
			AssetDatabase.CreateAsset(asset, path);
			return asset;
		}

		private static void ApplyDefaults(ScriptableObject asset)
		{
			switch (asset)
			{
				case WorldConfigAsset world:
					world.ApplyCodeDefaults();
					break;
				case TerrainConfigAsset terrain:
					terrain.ApplyCodeDefaults();
					break;
				case PlayerConfigAsset player:
					player.ApplyCodeDefaults();
					break;
				case EnemySpawnConfigAsset enemies:
					enemies.ApplyCodeDefaults();
					break;
				case PolarBearConfigAsset polarBear:
					polarBear.ApplyCodeDefaults();
					break;
				case TopDownCameraConfigAsset camera:
					camera.ApplyCodeDefaults();
					break;
				case DebugInputConfigAsset debug:
					debug.ApplyCodeDefaults();
					break;
			}
		}

		private static void EnsureFolder(string path)
		{
			if (AssetDatabase.IsValidFolder(path))
				return;

			var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
			var folderName = Path.GetFileName(path);
			if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
				return;

			if (!AssetDatabase.IsValidFolder(parent))
				EnsureFolder(parent);

			AssetDatabase.CreateFolder(parent, folderName);
		}
	}
}
#endif

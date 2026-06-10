using Game.UnityBridge.Configs;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public sealed class GameSessionBoot : MonoBehaviour
	{
		[SerializeField] GameSessionProfileAsset profile;

		private void Awake() => GameSessionSpawner.Spawn(transform, profile);
	}
}

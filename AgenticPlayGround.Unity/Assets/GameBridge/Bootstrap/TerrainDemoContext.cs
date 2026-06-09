using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Runtime;
using Game.Systems.Integration.Actors;
using Game.UnityBridge.Input;
using Game.UnityBridge.Presentation;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public sealed class TerrainDemoContext
	{
		public TerrainDemoContext(
			GameRuntime runtime,
			GeneratedWorldMap map,
			ActorHandle player,
			UnityWorldPresenter worldPresenter,
			PlayerFacingController facing,
			TopDownRpgCameraFollow cameraFollow,
			Camera camera,
			GameSessionState sessionState,
			CombatRuntimeServices combatServices)
		{
			Runtime = runtime;
			Map = map;
			Player = player;
			WorldPresenter = worldPresenter;
			Facing = facing;
			CameraFollow = cameraFollow;
			Camera = camera;
			SessionState = sessionState;
			CombatServices = combatServices;
		}

		public GameRuntime Runtime { get; }
		public GeneratedWorldMap Map { get; }
		public ActorHandle Player { get; }
		public UnityWorldPresenter WorldPresenter { get; }
		public PlayerFacingController Facing { get; }
		public TopDownRpgCameraFollow CameraFollow { get; }
		public Camera Camera { get; }
		public GameSessionState SessionState { get; }
		public CombatRuntimeServices CombatServices { get; }
	}
}

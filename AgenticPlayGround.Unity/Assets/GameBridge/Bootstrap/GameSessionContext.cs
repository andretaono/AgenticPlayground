using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime;
using Game.UnityBridge.Configs;
using Game.UnityBridge.Input;
using Game.UnityBridge.Presentation;
using Game.UnityBridge.Terrain;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public sealed class GameSessionContext
	{
		public GameSessionContext(
			TerrainDemoSessionResult session,
			UnityWorldPresenter worldPresenter,
			UnityTerrainPresenter terrainPresenter,
			PlayerFacingController facing,
			TopDownRpgCameraFollow cameraFollow,
			Camera camera,
			Transform sessionRoot,
			Transform terrainRoot,
			DebugInputSettings debug)
		{
			Session = session;
			Runtime = session.Runtime;
			Config = session.Config;
			Map = session.Map;
			Player = session.Player.Player;
			WorldPresenter = worldPresenter;
			TerrainPresenter = terrainPresenter;
			Facing = facing;
			CameraFollow = cameraFollow;
			Camera = camera;
			SessionRoot = sessionRoot;
			TerrainRoot = terrainRoot;
			SessionState = session.SessionState;
			CombatServices = session.CombatServices;
			Debug = debug ?? DebugInputSettings.Default;
		}

		public TerrainDemoSessionResult Session { get; }
		public GameRuntime Runtime { get; }
		public GameSessionConfig Config { get; }
		public GeneratedWorldMap Map { get; }
		public Game.Systems.Integration.Actors.ActorHandle Player { get; }
		public UnityWorldPresenter WorldPresenter { get; }
		public UnityTerrainPresenter TerrainPresenter { get; }
		public PlayerFacingController Facing { get; }
		public TopDownRpgCameraFollow CameraFollow { get; }
		public Camera Camera { get; }
		public Transform SessionRoot { get; }
		public Transform TerrainRoot { get; }
		public GameSessionState SessionState { get; }
		public CombatRuntimeServices CombatServices { get; }
		public DebugInputSettings Debug { get; }
	}
}

using Game.UnityBridge.Bootstrap;
using Game.UnityBridge.Debugging;
using UnityEngine;

namespace Game.UnityBridge.Runtime
{
	public sealed class WorldLayerDebugHost : MonoBehaviour
	{
		private GameSessionContext _context;
		private WorldLayerDebugOverlay _overlay;
		private WorldLayerDebugMode _mode = WorldLayerDebugMode.Off;
		private GUIStyle _boxStyle;
		private GUIStyle _labelStyle;

		public void Initialize(GameSessionContext context, Transform overlayParent)
		{
			_context = context;
			var terrain = context.Config.Terrain;

			_overlay = new WorldLayerDebugOverlay(
				overlayParent,
				context.Map,
				terrain.WorldUnitsPerTile,
				terrain.Heights.GroundHeight + 0.05f);
		}

		private void Update()
		{
			if (_context == null || !_context.Debug.EnableLayerDebug)
				return;

			HandleInput();
		}

		private void HandleInput()
		{
			if (UnityEngine.Input.GetKeyDown(_context.Debug.DebugGroundKey))
				SetMode(WorldLayerDebugMode.Ground);
			else if (UnityEngine.Input.GetKeyDown(_context.Debug.DebugOffKey))
				SetMode(WorldLayerDebugMode.Off);
		}

		private void SetMode(WorldLayerDebugMode mode)
		{
			_mode = mode;
			_overlay?.SetMode(mode);
		}

		private void OnGUI()
		{
			if (_context == null || !_context.Debug.EnableLayerDebug)
				return;

			EnsureStyles();

			const int boxWidth = 360;
			UnityEngine.GUILayout.BeginArea(new UnityEngine.Rect(10f, 10f, boxWidth, 180f), _boxStyle);

			if (TryGetPlayerTile(out var tileX, out var tileY))
			{
				var map = _context.Map;
				UnityEngine.GUILayout.Label($"Tile ({tileX}, {tileY})", _labelStyle);
				UnityEngine.GUILayout.Label($"Ground: {map.GroundLayer[tileX, tileY].Id}", _labelStyle);
			}
			else
			{
				UnityEngine.GUILayout.Label("Player outside map bounds", _labelStyle);
			}

			UnityEngine.GUILayout.Label($"Mode: {_mode}", _labelStyle);
			if (_mode != WorldLayerDebugMode.Off)
				UnityEngine.GUILayout.Label(WorldLayerDebugColors.GetLegend(_mode), _labelStyle);

			UnityEngine.GUILayout.Label("Keys: 1=Ground 0=Off", _labelStyle);

			UnityEngine.GUILayout.EndArea();
		}

		private bool TryGetPlayerTile(out int tileX, out int tileY)
		{
			tileX = 0;
			tileY = 0;

			var movement = _context.Runtime.Systems.Movement;
			if (movement == null)
				return false;

			var position = movement.Input.GetPosition(_context.Player.EntityId);
			tileX = UnityEngine.Mathf.FloorToInt(position.X);
			tileY = UnityEngine.Mathf.FloorToInt(position.Y);

			var map = _context.Map;
			return tileX >= 0 && tileY >= 0 && tileX < map.Width && tileY < map.Height;
		}

		private void EnsureStyles()
		{
			if (_boxStyle != null)
				return;

			_boxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
			{
				alignment = TextAnchor.UpperLeft
			};

			_labelStyle = new GUIStyle(UnityEngine.GUI.skin.label)
			{
				fontSize = 13,
				normal = { textColor = UnityEngine.Color.white }
			};
		}
	}
}

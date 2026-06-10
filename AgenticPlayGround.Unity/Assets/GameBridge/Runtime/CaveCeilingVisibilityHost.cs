using Game.UnityBridge.Bootstrap;
using Game.UnityBridge.Terrain;
using UnityEngine;

namespace Game.UnityBridge.Runtime
{
	public sealed class CaveCeilingVisibilityHost : MonoBehaviour
	{
		private GameSessionContext _context;
		private CaveCeilingVisibility _visibility;

		public void Initialize(GameSessionContext context, CaveCeilingVisibility visibility)
		{
			_context = context;
			_visibility = visibility;
		}

		private void LateUpdate()
		{
			if (_context == null || _visibility == null)
				return;

			var movement = _context.Runtime.Systems.Movement;
			if (movement == null)
				return;

			var position = movement.Input.GetPosition(_context.Player.EntityId);
			var tileX = Mathf.Clamp(Mathf.FloorToInt(position.X), 0, _context.Map.Width - 1);
			var tileY = Mathf.Clamp(Mathf.FloorToInt(position.Y), 0, _context.Map.Height - 1);
			var regionId = _context.Map.CaveRegionIndex[tileX, tileY];

			_visibility.UpdateForPlayerRegion(regionId);
		}
	}
}

using Game.UnityBridge.Bootstrap;
using UnityEngine;

namespace Game.UnityBridge.Runtime
{
	public sealed class CameraFollowHost : MonoBehaviour
	{
		private GameSessionContext _context;

		public void Initialize(GameSessionContext context) => _context = context;

		private void LateUpdate()
		{
			if (_context == null ||
			    !_context.WorldPresenter.TryGetTransform(_context.Player.EntityId, out var playerTransform))
			{
				return;
			}

			var camera = _context.Camera != null ? _context.Camera : UnityEngine.Camera.main;
			if (camera == null)
				return;

			_context.CameraFollow.LateUpdate(
				playerTransform,
				camera,
				_context.Facing.FacingYawDegrees);
		}
	}
}

using Game.UnityBridge.Bootstrap;
using UnityEngine;

namespace Game.UnityBridge.Runtime
{
	public sealed class GameLoopHost : MonoBehaviour
	{
		private GameSessionContext _context;

		public void Initialize(GameSessionContext context) => _context = context;

		private void Update()
		{
			if (_context == null || _context.SessionState.PlayerIsDead)
				return;

			_context.Facing.ApplyTurnInput(
				UnityEngine.Input.GetAxisRaw("Horizontal"),
				UnityEngine.Time.deltaTime,
				_context.Config.Player.TurnSpeedDegrees);
			_context.Runtime.Tick(UnityEngine.Time.deltaTime);
		}
	}
}

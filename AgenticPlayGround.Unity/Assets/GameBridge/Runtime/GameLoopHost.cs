using Game.UnityBridge.Bootstrap;
using UnityEngine;

namespace Game.UnityBridge.Runtime
{
	public sealed class GameLoopHost : MonoBehaviour
	{
		private TerrainDemoContext _context;
		private float _turnSpeedDegrees;

		public void Initialize(TerrainDemoContext context, float turnSpeedDegrees)
		{
			_context = context;
			_turnSpeedDegrees = turnSpeedDegrees;
		}

		private void Update()
		{
			if (_context == null)
				return;

			_context.Facing.ApplyTurnInput(
				UnityEngine.Input.GetAxisRaw("Horizontal"),
				UnityEngine.Time.deltaTime,
				_turnSpeedDegrees);
			_context.Runtime.Tick(UnityEngine.Time.deltaTime);
		}
	}
}

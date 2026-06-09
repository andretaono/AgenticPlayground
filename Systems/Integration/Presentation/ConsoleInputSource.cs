using System.Collections.Generic;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;

namespace Game.Systems.Integration.Presentation;

public sealed class ConsoleInputSource : IInputSource
{
	private readonly AgentId _boundAgentId;
	private Vector2 _lastDirection;

	public ConsoleInputSource(AgentId boundAgentId) => _boundAgentId = boundAgentId;

	public Vector2 PollMovementInput(AgentId agentId)
	{
		if (!agentId.Equals(_boundAgentId))
			return Vector2.Zero;

		try
		{
			if (!Console.KeyAvailable)
				return _lastDirection;

			var keys = new HashSet<ConsoleKey>();
			while (Console.KeyAvailable)
			{
				var keyInfo = Console.ReadKey(intercept: true);
				keys.Add(keyInfo.Key);
			}

			_lastDirection = keys.Count > 0 ? NormalizeDirection(keys) : Vector2.Zero;
			return _lastDirection;
		}
		catch (InvalidOperationException)
		{
			return Vector2.Zero;
		}
	}

	public bool PollAttackInput(AgentId agentId) => false;

	public void OnKey(ConsoleKey key)
	{
		_lastDirection = KeyToDirection(key);
	}

	public void OnKeys(IEnumerable<ConsoleKey> keys) => _lastDirection = NormalizeDirection(keys);

	private static Vector2 NormalizeDirection(IEnumerable<ConsoleKey> keys)
	{
		float x = 0f, y = 0f;
		foreach (var key in keys)
		{
			switch (key)
			{
				case ConsoleKey.W: y -= 1f; break;
				case ConsoleKey.S: y += 1f; break;
				case ConsoleKey.A: x -= 1f; break;
				case ConsoleKey.D: x += 1f; break;
			}
		}

		var dir = new Vector2(x, y);
		return dir.Magnitude() <= 1e-6f ? Vector2.Zero : dir.Normalized();
	}

	private static Vector2 KeyToDirection(ConsoleKey key) => key switch
	{
		ConsoleKey.W => new Vector2(0f, -1f),
		ConsoleKey.S => new Vector2(0f, 1f),
		ConsoleKey.A => new Vector2(-1f, 0f),
		ConsoleKey.D => new Vector2(1f, 0f),
		_ => Vector2.Zero
	};
}

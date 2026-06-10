using UnityEngine;

namespace Game.UnityBridge.Configs
{
	public sealed class DebugInputSettings
	{
		public DebugInputSettings(bool enableLayerDebug, KeyCode debugGroundKey, KeyCode debugOffKey)
		{
			EnableLayerDebug = enableLayerDebug;
			DebugGroundKey = debugGroundKey;
			DebugOffKey = debugOffKey;
		}

		public static DebugInputSettings Default { get; } =
			new(true, KeyCode.Alpha1, KeyCode.Alpha0);

		public bool EnableLayerDebug { get; }
		public KeyCode DebugGroundKey { get; }
		public KeyCode DebugOffKey { get; }
	}
}

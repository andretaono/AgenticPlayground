using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "DebugInputConfig", menuName = ConfigAssetMenus.DebugInput)]
	public sealed class DebugInputConfigAsset : ScriptableObject
	{
		[SerializeField] bool enableLayerDebug = true;
		[SerializeField] KeyCode debugGroundKey = KeyCode.Alpha1;
		[SerializeField] KeyCode debugOffKey = KeyCode.Alpha0;

		public DebugInputSettings ToSettings() =>
			new(enableLayerDebug, debugGroundKey, debugOffKey);

		public void ApplyCodeDefaults()
		{
			var defaults = DebugInputSettings.Default;
			enableLayerDebug = defaults.EnableLayerDebug;
			debugGroundKey = defaults.DebugGroundKey;
			debugOffKey = defaults.DebugOffKey;
		}
	}
}

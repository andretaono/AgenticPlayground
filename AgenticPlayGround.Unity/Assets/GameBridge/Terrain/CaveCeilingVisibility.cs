using System.Collections.Generic;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public sealed class CaveCeilingVisibility
	{
		private readonly Dictionary<int, List<Renderer>> _renderersByRegion = new();
		private int _hiddenRegionId = -1;

		public void Register(int regionId, Renderer renderer)
		{
			if (regionId < 0 || renderer is null)
				return;

			if (!_renderersByRegion.TryGetValue(regionId, out var renderers))
			{
				renderers = new List<Renderer>();
				_renderersByRegion[regionId] = renderers;
			}

			renderers.Add(renderer);
		}

		public void UpdateForPlayerRegion(int playerRegionId)
		{
			if (_hiddenRegionId == playerRegionId)
				return;

			if (_hiddenRegionId >= 0)
				SetRegionVisible(_hiddenRegionId, true);

			_hiddenRegionId = playerRegionId;

			if (_hiddenRegionId >= 0)
				SetRegionVisible(_hiddenRegionId, false);
		}

		private void SetRegionVisible(int regionId, bool visible)
		{
			if (!_renderersByRegion.TryGetValue(regionId, out var renderers))
				return;

			foreach (var renderer in renderers)
			{
				if (renderer != null)
					renderer.enabled = visible;
			}
		}
	}
}

using System.Collections.Generic;
using Game.Systems.Domain.TerrainMesh;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.TerrainMesh.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using UnityEngine;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;
using UnityVector3 = UnityEngine.Vector3;

namespace Game.UnityBridge.Presentation
{
	public sealed class UnityWorldPresenter : IWorldPresenter
	{
		private readonly Transform _actorsRoot;
		private readonly Heightmap _heightmap;
		private readonly IHeightmapSampler _heightmapSampler;
		private readonly float _worldUnitsPerTile;
		private readonly float _heightScale;
		private readonly float _characterHalfHeight;
		private readonly Dictionary<EntityId, Transform> _actors = new();

		public UnityWorldPresenter(
			Transform actorsRoot,
			Heightmap heightmap,
			float worldUnitsPerTile,
			float heightScale,
			float characterHalfHeight,
			IHeightmapSampler heightmapSampler = null)
		{
			_actorsRoot = actorsRoot ?? throw new System.ArgumentNullException(nameof(actorsRoot));
			_heightmap = heightmap ?? throw new System.ArgumentNullException(nameof(heightmap));
			_worldUnitsPerTile = worldUnitsPerTile;
			_heightScale = heightScale;
			_characterHalfHeight = characterHalfHeight;
			_heightmapSampler = heightmapSampler ?? new TerrainMeshSystem().Sampler;
		}

		public void SyncActorPosition(EntityId entityId, GameVector2 position)
		{
			if (!_actors.TryGetValue(entityId, out var actorTransform))
			{
				actorTransform = CreateActorVisual(entityId);
				_actors[entityId] = actorTransform;
			}

			var worldX = position.X * _worldUnitsPerTile;
			var worldZ = position.Y * _worldUnitsPerTile;
			var surfaceHeight = _heightmapSampler.SampleBilinear(_heightmap, worldX, worldZ) * _heightScale;

			actorTransform.position = new UnityVector3(
				worldX,
				surfaceHeight + _characterHalfHeight,
				worldZ);
		}

		public bool TryGetTransform(EntityId entityId, out Transform transform) =>
			_actors.TryGetValue(entityId, out transform);

		private Transform CreateActorVisual(EntityId entityId)
		{
			var actorObject = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Capsule);
			actorObject.name = $"Actor_{entityId.Value}";
			actorObject.transform.SetParent(_actorsRoot, worldPositionStays: false);
			actorObject.transform.localScale = new UnityVector3(0.5f, _characterHalfHeight, 0.5f);

			var renderer = actorObject.GetComponent<Renderer>();
			if (renderer != null)
				renderer.material.color = new UnityEngine.Color(0.9f, 0.25f, 0.2f);

			var collider = actorObject.GetComponent<Collider>();
			if (collider != null)
				UnityEngine.Object.Destroy(collider);

			return actorObject.transform;
		}
	}
}

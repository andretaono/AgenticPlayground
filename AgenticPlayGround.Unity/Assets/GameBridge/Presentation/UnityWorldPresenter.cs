using System.Collections.Generic;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
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
		private readonly WalkableSurfaceHeightSampler _surfaceHeightSampler;
		private readonly float _worldUnitsPerTile;
		private readonly float _heightScale;
		private readonly float _characterHalfHeight;
		private readonly float _characterRadius;
		private readonly Dictionary<EntityId, Transform> _actors = new();
		private readonly HashSet<EntityId> _polarBears = new();
		private readonly Dictionary<EntityId, float> _actorVisualRadii = new();

		public UnityWorldPresenter(
			Transform actorsRoot,
			Heightmap heightmap,
			float worldUnitsPerTile,
			float heightScale,
			float characterHalfHeight,
			float characterRadius,
			WalkableSurfaceHeightSampler surfaceHeightSampler)
		{
			_actorsRoot = actorsRoot ?? throw new System.ArgumentNullException(nameof(actorsRoot));
			_heightmap = heightmap ?? throw new System.ArgumentNullException(nameof(heightmap));
			_surfaceHeightSampler = surfaceHeightSampler ??
			                        throw new System.ArgumentNullException(nameof(surfaceHeightSampler));
			_worldUnitsPerTile = worldUnitsPerTile;
			_heightScale = heightScale;
			_characterHalfHeight = characterHalfHeight;
			_characterRadius = characterRadius;
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
			var surfaceHeight = _surfaceHeightSampler.Sample(
				_heightmap,
				position.X,
				position.Y,
				_heightScale);

			actorTransform.position = new UnityVector3(
				worldX,
				surfaceHeight + _characterHalfHeight,
				worldZ);
		}

		public bool TryGetTransform(EntityId entityId, out Transform transform) =>
			_actors.TryGetValue(entityId, out transform);

		public void ConfigurePolarBearVisual(EntityId entityId, float bodyRadius)
		{
			_polarBears.Add(entityId);
			_actorVisualRadii[entityId] = bodyRadius;
		}

		private Transform CreateActorVisual(EntityId entityId)
		{
			const float unityCapsuleMeshRadius = 0.5f;

			var actorObject = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Capsule);
			actorObject.name = _polarBears.Contains(entityId)
				? $"PolarBear_{entityId.Value}"
				: $"Actor_{entityId.Value}";
			actorObject.transform.SetParent(_actorsRoot, worldPositionStays: false);

			var isPolarBear = _polarBears.Contains(entityId);
			var visualRadius = ResolveVisualRadius(entityId);
			var visualRadiusWorld = visualRadius * _worldUnitsPerTile;
			var horizontalScale = visualRadiusWorld / unityCapsuleMeshRadius;
			actorObject.transform.localScale = new UnityVector3(
				horizontalScale,
				_characterHalfHeight * (isPolarBear ? 1.2f : 1f),
				horizontalScale);

			var renderer = actorObject.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = isPolarBear
					? new UnityEngine.Color(0.92f, 0.94f, 0.97f)
					: new UnityEngine.Color(0.9f, 0.25f, 0.2f);
			}

			var collider = actorObject.GetComponent<Collider>();
			if (collider != null)
				UnityEngine.Object.Destroy(collider);

			return actorObject.transform;
		}

		private float ResolveVisualRadius(EntityId entityId) =>
			_actorVisualRadii.TryGetValue(entityId, out var radius) ? radius : _characterRadius;
	}
}

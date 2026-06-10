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
		private readonly Dictionary<EntityId, ActorVisual> _actors = new();
		private readonly HashSet<EntityId> _polarBears = new();

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
			var visual = GetOrCreateVisual(entityId);
			var worldX = position.X * _worldUnitsPerTile;
			var worldZ = position.Y * _worldUnitsPerTile;
			var surfaceHeight = _surfaceHeightSampler.Sample(
				_heightmap,
				position.X,
				position.Y,
				_heightScale);

			visual.Root.position = new UnityVector3(
				worldX,
				surfaceHeight + _characterHalfHeight,
				worldZ);
		}

		public void SyncActorHealth(EntityId entityId, float current, float maximum)
		{
			var visual = GetOrCreateVisual(entityId);
			visual.HealthBar.SetFill(current, maximum);
		}

		public void SyncActorFacing(EntityId entityId, float yawDegrees)
		{
			if (!_actors.TryGetValue(entityId, out var visual))
				return;

			visual.Root.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
		}

		public void ShowAttackArc(
			EntityId entityId,
			float range,
			float arcDegrees,
			float durationSeconds)
		{
			if (!_actors.TryGetValue(entityId, out var visual))
				return;

			visual.ArcVisualizer.Show(
				range * _worldUnitsPerTile,
				arcDegrees,
				durationSeconds);
		}

		public void RemoveActor(EntityId entityId)
		{
			if (!_actors.TryGetValue(entityId, out var visual))
				return;

			Object.Destroy(visual.Root.gameObject);
			_actors.Remove(entityId);
		}

		private readonly Dictionary<EntityId, ActorVisualDescriptor> _visualDescriptors = new();

		public void ConfigureActorVisual(EntityId entityId, ActorVisualDescriptor descriptor)
		{
			_visualDescriptors[entityId] = descriptor ?? throw new System.ArgumentNullException(nameof(descriptor));
			if (descriptor.IsPolarBear)
				_polarBears.Add(entityId);
		}

		public bool TryGetTransform(EntityId entityId, out Transform transform)
		{
			if (_actors.TryGetValue(entityId, out var visual))
			{
				transform = visual.Root;
				return true;
			}

			transform = null!;
			return false;
		}

		private ActorVisual GetOrCreateVisual(EntityId entityId)
		{
			if (_actors.TryGetValue(entityId, out var visual))
				return visual;

			visual = CreateActorVisual(entityId);
			_actors[entityId] = visual;
			return visual;
		}

		private ActorVisual CreateActorVisual(EntityId entityId)
		{
			const float unityCapsuleMeshRadius = 0.5f;

			var actorObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			actorObject.name = _polarBears.Contains(entityId)
				? $"PolarBear_{entityId.Value}"
				: $"Actor_{entityId.Value}";
			actorObject.transform.SetParent(_actorsRoot, worldPositionStays: false);

			var descriptor = ResolveDescriptor(entityId);
			var isPolarBear = descriptor.IsPolarBear;
			var visualRadius = descriptor.BodyRadius;
			var visualRadiusWorld = visualRadius * _worldUnitsPerTile;
			var horizontalScale = visualRadiusWorld / unityCapsuleMeshRadius;
			actorObject.transform.localScale = new UnityVector3(
				horizontalScale,
				_characterHalfHeight * descriptor.VerticalScale,
				horizontalScale);

			var renderer = actorObject.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = new Color(
					descriptor.ColorR,
					descriptor.ColorG,
					descriptor.ColorB);
			}

			var collider = actorObject.GetComponent<Collider>();
			if (collider != null)
				Object.Destroy(collider);

			var healthBar = ActorHealthBarView.Create(actorObject.transform, _characterHalfHeight);
			var arcVisualizer = AttackArcVisualizer.Create(actorObject.transform);

			return new ActorVisual(actorObject.transform, healthBar, arcVisualizer);
		}

		private ActorVisualDescriptor ResolveDescriptor(EntityId entityId) =>
			_visualDescriptors.TryGetValue(entityId, out var descriptor)
				? descriptor
				: new ActorVisualDescriptor { BodyRadius = _characterRadius };

		private sealed class ActorVisual
		{
			public ActorVisual(Transform root, ActorHealthBarView healthBar, AttackArcVisualizer arcVisualizer)
			{
				Root = root;
				HealthBar = healthBar;
				ArcVisualizer = arcVisualizer;
			}

			public Transform Root { get; }
			public ActorHealthBarView HealthBar { get; }
			public AttackArcVisualizer ArcVisualizer { get; }
		}
	}
}

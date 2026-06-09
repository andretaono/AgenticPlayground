using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;
using UnityEngine;

namespace Game.UnityBridge.Presentation
{
	public sealed class AttackArcVisualizer : MonoBehaviour
	{
		private LineRenderer _lineRenderer = null!;
		private float _hideAtTime;

		public static AttackArcVisualizer Create(Transform actorRoot)
		{
			var rootObject = new GameObject("AttackArc");
			rootObject.transform.SetParent(actorRoot, worldPositionStays: false);
			rootObject.transform.localPosition = Vector3.zero;
			var visualizer = rootObject.AddComponent<AttackArcVisualizer>();
			visualizer.Build();
			return visualizer;
		}

		private void Build()
		{
			_lineRenderer = gameObject.AddComponent<LineRenderer>();
			_lineRenderer.useWorldSpace = false;
			_lineRenderer.loop = false;
			_lineRenderer.widthMultiplier = 0.05f;
			_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			_lineRenderer.startColor = new Color(1f, 0.85f, 0.2f, 0.75f);
			_lineRenderer.endColor = new Color(1f, 0.35f, 0.1f, 0.35f);
			_lineRenderer.positionCount = 0;
		}

		private void Update()
		{
			if (_lineRenderer.positionCount == 0)
				return;

			if (Time.time >= _hideAtTime)
				_lineRenderer.positionCount = 0;
		}

		public void Show(GameVector2 forward, float rangeWorld, float arcDegrees, float durationSeconds)
		{
			const int segments = 16;
			var halfArc = arcDegrees * 0.5f * Mathf.Deg2Rad;
			var baseAngle = Mathf.Atan2(forward.X, forward.Y);
			var positions = new Vector3[segments + 2];
			positions[0] = Vector3.zero;

			for (var i = 0; i <= segments; i++)
			{
				var t = i / (float)segments;
				var angle = baseAngle - halfArc + (2f * halfArc * t);
				positions[i + 1] = new Vector3(Mathf.Sin(angle) * rangeWorld, 0.05f, Mathf.Cos(angle) * rangeWorld);
			}

			_lineRenderer.positionCount = positions.Length;
			_lineRenderer.SetPositions(positions);
			_hideAtTime = Time.time + durationSeconds;
		}
	}
}

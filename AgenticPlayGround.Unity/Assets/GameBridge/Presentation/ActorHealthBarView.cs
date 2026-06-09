using UnityEngine;

namespace Game.UnityBridge.Presentation
{
	public sealed class ActorHealthBarView : MonoBehaviour
	{
		private Transform _fillTransform = null!;
		private float _barWidth = 0.8f;

		public static ActorHealthBarView Create(Transform actorRoot, float characterHalfHeight)
		{
			var rootObject = new GameObject("HealthBar");
			rootObject.transform.SetParent(actorRoot, worldPositionStays: false);
			rootObject.transform.localPosition = new Vector3(0f, characterHalfHeight * 2.2f, 0f);
			rootObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

			var view = rootObject.AddComponent<ActorHealthBarView>();
			view.Build();
			return view;
		}

		private void Build()
		{
			var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
			background.name = "Background";
			background.transform.SetParent(transform, worldPositionStays: false);
			background.transform.localPosition = Vector3.zero;
			background.transform.localScale = new Vector3(_barWidth, 0.12f, 1f);
			Destroy(background.GetComponent<Collider>());
			background.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

			var fillObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
			fillObject.name = "Fill";
			fillObject.transform.SetParent(transform, worldPositionStays: false);
			fillObject.transform.localPosition = new Vector3(-_barWidth * 0.5f, 0f, -0.01f);
			fillObject.transform.localScale = new Vector3(_barWidth, 0.08f, 1f);
			Destroy(fillObject.GetComponent<Collider>());
			fillObject.GetComponent<Renderer>().material.color = new Color(0.2f, 0.85f, 0.25f, 0.95f);
			_fillTransform = fillObject.transform;
		}

		public void SetFill(float current, float maximum)
		{
			if (_fillTransform == null || maximum <= 0f)
				return;

			var ratio = Mathf.Clamp01(current / maximum);
			_fillTransform.localScale = new Vector3(_barWidth * ratio, 0.08f, 1f);
			_fillTransform.localPosition = new Vector3(-_barWidth * 0.5f + (_barWidth * ratio * 0.5f), 0f, -0.01f);
		}
	}
}

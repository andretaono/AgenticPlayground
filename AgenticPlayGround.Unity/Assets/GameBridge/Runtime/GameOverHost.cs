using Game.UnityBridge.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UnityBridge.Runtime
{
	public sealed class GameOverHost : MonoBehaviour
	{
		private GameSessionContext _context;
		private Canvas _canvas;
		private bool _shown;

		public void Initialize(GameSessionContext context)
		{
			_context = context;
			BuildOverlay();
		}

		private void Update()
		{
			if (_context == null || _shown || !_context.SessionState.PlayerIsDead)
				return;

			_shown = true;
			_canvas.gameObject.SetActive(true);
		}

		private void BuildOverlay()
		{
			var canvasObject = new GameObject("GameOverOverlay");
			canvasObject.transform.SetParent(transform, worldPositionStays: false);
			_canvas = canvasObject.AddComponent<Canvas>();
			_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_canvas.sortingOrder = 100;
			canvasObject.AddComponent<CanvasScaler>();
			canvasObject.AddComponent<GraphicRaycaster>();
			canvasObject.SetActive(false);

			var panelObject = new GameObject("Panel");
			panelObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);
			var panelImage = panelObject.AddComponent<Image>();
			panelImage.color = new Color(0f, 0f, 0f, 0.72f);
			var panelRect = panelObject.GetComponent<RectTransform>();
			panelRect.anchorMin = Vector2.zero;
			panelRect.anchorMax = Vector2.one;
			panelRect.offsetMin = Vector2.zero;
			panelRect.offsetMax = Vector2.zero;

			var textObject = new GameObject("Title");
			textObject.transform.SetParent(panelObject.transform, worldPositionStays: false);
			var text = textObject.AddComponent<Text>();
			text.text = "Game Over";
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = 48;
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.white;
			var textRect = textObject.GetComponent<RectTransform>();
			textRect.anchorMin = new Vector2(0.5f, 0.55f);
			textRect.anchorMax = new Vector2(0.5f, 0.55f);
			textRect.sizeDelta = new Vector2(600f, 80f);

			var hintObject = new GameObject("Hint");
			hintObject.transform.SetParent(panelObject.transform, worldPositionStays: false);
			var hint = hintObject.AddComponent<Text>();
			hint.text = "You were defeated.";
			hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			hint.fontSize = 24;
			hint.alignment = TextAnchor.MiddleCenter;
			hint.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
			var hintRect = hintObject.GetComponent<RectTransform>();
			hintRect.anchorMin = new Vector2(0.5f, 0.42f);
			hintRect.anchorMax = new Vector2(0.5f, 0.42f);
			hintRect.sizeDelta = new Vector2(600f, 40f);
		}
	}
}

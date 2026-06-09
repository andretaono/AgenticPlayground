using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class CombatRuntimeServices
{
	private readonly Dictionary<Ability, ArcAttackAbilityDefinition> _abilityDefinitions = new();

	public CombatRuntimeServices(
		AgentOrientationStore orientation,
		AttackCooldownTracker cooldownTracker,
		CombatFeedbackStore feedbackStore,
		GameSessionState sessionState)
	{
		Orientation = orientation ?? throw new ArgumentNullException(nameof(orientation));
		CooldownTracker = cooldownTracker ?? throw new ArgumentNullException(nameof(cooldownTracker));
		FeedbackStore = feedbackStore ?? throw new ArgumentNullException(nameof(feedbackStore));
		SessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
	}

	public AgentOrientationStore Orientation { get; }
	public AttackCooldownTracker CooldownTracker { get; }
	public CombatFeedbackStore FeedbackStore { get; }
	public GameSessionState SessionState { get; }
	public float CurrentTime { get; set; }

	public void RegisterAbilityDefinition(Ability ability, ArcAttackAbilityDefinition definition)
	{
		if (ability is null)
			throw new ArgumentNullException(nameof(ability));
		if (definition is null)
			throw new ArgumentNullException(nameof(definition));

		_abilityDefinitions[ability] = definition;
	}

	public bool TryGetAbilityDefinition(Ability ability, out ArcAttackAbilityDefinition definition) =>
		_abilityDefinitions.TryGetValue(ability, out definition!);
}

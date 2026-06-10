namespace Game.Systems.Integration.Runtime;

public static class StandardTickOrder
{
	public const int PreCognition = 30;
	public const int WorldCognition = 35;
	public const int AgentBehaviour = 40;
	public const int Input = 50;
	public const int BehaviourIntentSubmission = 55;
	public const int CommandExecution = 75;
	public const int AgentOrientation = 79;
	public const int AgentCombat = 80;
	public const int Vitality = 85;
	public const int MovementState = 95;
	public const int AgentMovement = 100;
	public const int WorldPresentation = 105;
	public const int CombatPresentation = 106;
	public const int EntityResource = 110;
}

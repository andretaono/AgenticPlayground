namespace Game.Systems.Integration.Enemies.Common.Advantage;

public interface IAttackAdvantageRule
{
	bool Evaluate(AdvantageContext context);
}

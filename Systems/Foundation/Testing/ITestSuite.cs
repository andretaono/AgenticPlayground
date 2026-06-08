namespace Game.Systems.Foundation.Testing;

public interface ITestSuite
{
	string Name { get; }

	void Register(TestRegistry registry);
}

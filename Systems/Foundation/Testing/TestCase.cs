namespace Game.Systems.Foundation.Testing;

public sealed class TestCase
{
	public TestCase(string fullName, Action run)
	{
		FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
		Run = run ?? throw new ArgumentNullException(nameof(run));
	}

	public string FullName { get; }
	public Action Run { get; }
}

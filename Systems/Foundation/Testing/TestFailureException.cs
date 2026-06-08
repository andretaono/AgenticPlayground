namespace Game.Systems.Foundation.Testing;

public sealed class TestFailureException : Exception
{
	public TestFailureException(string message) : base(message)
	{
	}
}

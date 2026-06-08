namespace Game.Systems.Foundation.Testing;

public sealed class TestCaseResult
{
	public TestCaseResult(string fullName, bool passed, string? message, long elapsedMilliseconds)
	{
		FullName = fullName;
		Passed = passed;
		Message = message;
		ElapsedMilliseconds = elapsedMilliseconds;
	}

	public string FullName { get; }
	public bool Passed { get; }
	public string? Message { get; }
	public long ElapsedMilliseconds { get; }
}

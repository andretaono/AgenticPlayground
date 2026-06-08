namespace Game.Systems.Foundation.Testing;

public sealed class TestRunResult
{
	public TestRunResult(IReadOnlyList<TestCaseResult> results, long totalElapsedMilliseconds)
	{
		Results = results ?? throw new ArgumentNullException(nameof(results));
		TotalElapsedMilliseconds = totalElapsedMilliseconds;
	}

	public IReadOnlyList<TestCaseResult> Results { get; }
	public long TotalElapsedMilliseconds { get; }
	public int PassedCount => Results.Count(r => r.Passed);
	public int FailedCount => Results.Count(r => !r.Passed);
}

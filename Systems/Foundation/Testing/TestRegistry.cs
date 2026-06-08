using System.Diagnostics;

namespace Game.Systems.Foundation.Testing;

public sealed class TestRegistry
{
	private readonly List<TestCase> _cases = new();

	public void Add(string suiteName, string caseName, Action run)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(suiteName);
		ArgumentException.ThrowIfNullOrWhiteSpace(caseName);
		ArgumentNullException.ThrowIfNull(run);

		var fullName = $"{suiteName} / {caseName}";
		_cases.Add(new TestCase(fullName, run));
	}

	public TestRunResult RunAll(string? filter = null)
	{
		var cases = string.IsNullOrWhiteSpace(filter)
			? _cases
			: _cases.Where(c => c.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

		var stopwatch = Stopwatch.StartNew();
		var results = new List<TestCaseResult>(cases.Count);

		foreach (var testCase in cases)
		{
			var caseStopwatch = Stopwatch.StartNew();
			try
			{
				testCase.Run();
				results.Add(new TestCaseResult(testCase.FullName, passed: true, message: null, caseStopwatch.ElapsedMilliseconds));
			}
			catch (TestFailureException ex)
			{
				results.Add(new TestCaseResult(testCase.FullName, passed: false, ex.Message, caseStopwatch.ElapsedMilliseconds));
			}
			catch (Exception ex)
			{
				results.Add(new TestCaseResult(
					testCase.FullName,
					passed: false,
					$"{ex.GetType().Name}: {ex.Message}",
					caseStopwatch.ElapsedMilliseconds));
			}
		}

		stopwatch.Stop();
		return new TestRunResult(results, stopwatch.ElapsedMilliseconds);
	}
}

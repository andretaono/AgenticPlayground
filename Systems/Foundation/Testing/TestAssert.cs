namespace Game.Systems.Foundation.Testing;

public static class TestAssert
{
	public static void True(bool condition, string? message = null)
	{
		if (!condition)
			throw new TestFailureException(message ?? "Expected True but was False.");
	}

	public static void False(bool condition, string? message = null)
	{
		if (condition)
			throw new TestFailureException(message ?? "Expected False but was True.");
	}

	public static void Equal<T>(T expected, T actual, string? message = null)
		where T : IEquatable<T>
	{
		if (expected is null && actual is null)
			return;

		if (expected is null || actual is null || !expected.Equals(actual))
		{
			throw new TestFailureException(
				message ?? $"Expected {FormatValue(expected)} but was {FormatValue(actual)}.");
		}
	}

	public static void NotEqual<T>(T expected, T actual, string? message = null)
		where T : IEquatable<T>
	{
		if (expected is null && actual is null)
			throw new TestFailureException(message ?? "Expected values to differ but both were null.");

		if (expected is not null && expected.Equals(actual))
		{
			throw new TestFailureException(
				message ?? $"Expected a value other than {FormatValue(expected)}.");
		}
	}

	private static string FormatValue<T>(T? value) => value?.ToString() ?? "null";
}

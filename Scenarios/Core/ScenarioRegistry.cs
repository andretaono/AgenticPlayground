using Game.Scenarios.Core.Interfaces;

namespace Game.Scenarios.Core
{
	public sealed class ScenarioRegistry
	{
		private readonly Dictionary<string, IScenario> _scenarios = new();

		public void Register(IScenario scenario)
		{
			_scenarios.Add(scenario.Name, scenario);
		}

		public bool TryGet(string name, out IScenario scenario)
		{
			return _scenarios.TryGetValue(name, out scenario);
		}

		public IEnumerable<string> Names => _scenarios.Keys;
	}
}

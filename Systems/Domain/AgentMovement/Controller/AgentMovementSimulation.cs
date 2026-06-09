using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Interfaces;

using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Controller;

internal class AgentMovementSimulation : IAgentMovementSimulation
{
    private readonly IGameMath _math;
    private readonly AgentMovementStateStore _store;
	private readonly IAgentMovementPolicy _movementPolicy;

    public AgentMovementSimulation(
		IGameMath math,
		AgentMovementStateStore store,
		IAgentMovementPolicy movementPolicy)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _store = store ?? throw new ArgumentNullException(nameof(store));
		_movementPolicy = movementPolicy ?? throw new ArgumentNullException(nameof(movementPolicy));
	}

    public void AdvanceSimulation(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

        foreach (var kvp in _store.Agents)
        {
            var agent = kvp.Value;
            Step(kvp.Key, agent, deltaTime, _math);

            if (!_math.IsFinite(agent.Position))
                throw new InvalidOperationException($"Movement produced a non-finite position for entity '{kvp.Key}'.");
        }
    }

    private void Step(EntityId entityId, AgentMovementAgentState agent, float deltaTime, IGameMath math)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (math == null) throw new ArgumentNullException(nameof(math));

        var config = agent.MovementConfig;
        if (config == null)
            throw new InvalidOperationException($"Movement config missing for entity '{entityId}'.");

        var speed = agent.MovementState switch
        {
            AgentMovementState.Grounded => config.GroundSpeed,
            AgentMovementState.Swimming => config.SwimSpeed,
            AgentMovementState.Airborne => config.AirSpeed,
            _ => config.GroundSpeed
        };

        var direction = math.Normalize(agent.PendingInput);
        if (math.MagnitudeSquared(direction) <= 0f)
        {
            agent.PendingInput = GameMathSystem.Zero;
            agent.Velocity = GameMathSystem.Zero;
            return;
        }

        var displacement = math.Scale(math.Scale(direction, speed), deltaTime);
        var resolvedPosition = ResolveSlidingPosition(
            entityId,
            agent.Position,
            displacement,
            config.BodyRadius,
            math);

        var moved = math.Distance(agent.Position, resolvedPosition) > 1e-6f;
        if (!moved)
        {
            agent.PendingInput = GameMathSystem.Zero;
            agent.Velocity = GameMathSystem.Zero;
            return;
        }

        if (deltaTime > 0f)
        {
            var actualDisplacement = math.Subtract(resolvedPosition, agent.Position);
            agent.Velocity = math.Scale(actualDisplacement, 1f / deltaTime);
        }
        else
        {
            agent.Velocity = GameMathSystem.Zero;
        }

        agent.Position = new Game.Systems.Foundation.GameMath.Core.Model.Vector3(
            resolvedPosition.X,
            resolvedPosition.Y,
            resolvedPosition.Z);

        // Input is treated as "per-frame intent" (caller sets every frame).
        agent.PendingInput = GameMathSystem.Zero;
    }

    private IVector3 ResolveSlidingPosition(
        EntityId entityId,
        IVector3 currentPosition,
        IVector3 displacement,
        float bodyRadius,
        IGameMath math)
    {
        var proposed = math.Add(currentPosition, displacement);
        if (_movementPolicy.CanMoveTo(entityId, proposed, bodyRadius))
            return proposed;

        var deltaX = math.Create(displacement.X, 0f, 0f);
        var deltaY = math.Create(0f, displacement.Y, 0f);

        var afterX = TryAxisMove(entityId, currentPosition, deltaX, bodyRadius, math);
        var afterY = TryAxisMove(entityId, currentPosition, deltaY, bodyRadius, math);
        var afterYX = TryAxisMove(entityId, afterY, deltaX, bodyRadius, math);
        var afterXY = TryAxisMove(entityId, afterX, deltaY, bodyRadius, math);

        return ChooseBestSlide(
            currentPosition,
            displacement,
            afterX,
            afterY,
            afterYX,
            afterXY,
            math);
    }

    private static IVector3 ChooseBestSlide(
        IVector3 currentPosition,
        IVector3 intendedDisplacement,
        IVector3 afterX,
        IVector3 afterY,
        IVector3 afterYX,
        IVector3 afterXY,
        IGameMath math)
    {
        var best = currentPosition;
        var bestScore = 0f;

        ConsiderSlideCandidate(currentPosition, intendedDisplacement, afterX, math, ref best, ref bestScore);
        ConsiderSlideCandidate(currentPosition, intendedDisplacement, afterY, math, ref best, ref bestScore);
        ConsiderSlideCandidate(currentPosition, intendedDisplacement, afterYX, math, ref best, ref bestScore);
        ConsiderSlideCandidate(currentPosition, intendedDisplacement, afterXY, math, ref best, ref bestScore);

        return best;
    }

    private static void ConsiderSlideCandidate(
        IVector3 currentPosition,
        IVector3 intendedDisplacement,
        IVector3 candidate,
        IGameMath math,
        ref IVector3 best,
        ref float bestScore)
    {
        if (math.Distance(currentPosition, candidate) <= 1e-6f)
            return;

        var score = math.Dot(math.Subtract(candidate, currentPosition), intendedDisplacement);
        if (score <= bestScore + 1e-6f)
            return;

        bestScore = score;
        best = candidate;
    }

    private IVector3 TryAxisMove(
        EntityId entityId,
        IVector3 currentPosition,
        IVector3 axisDelta,
        float bodyRadius,
        IGameMath math)
    {
        if (math.MagnitudeSquared(axisDelta) <= 1e-12f)
            return currentPosition;

        var proposed = math.Add(currentPosition, axisDelta);
        return _movementPolicy.CanMoveTo(entityId, proposed, bodyRadius)
            ? proposed
            : currentPosition;
    }
}

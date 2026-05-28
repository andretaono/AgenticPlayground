using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Foundation.GameMath.Interfaces;

public interface IGameMath
{
    Vector3 Create(float x, float y, float z);
    Vector3 Add(IVector3 a, IVector3 b);
    Vector3 Subtract(IVector3 a, IVector3 b);
    Vector3 Scale(IVector3 v, float scalar);

    float Dot(IVector3 a, IVector3 b);
    float MagnitudeSquared(IVector3 v);
    float Magnitude(IVector3 v);
    float Distance(IVector3 a, IVector3 b);
    Vector3 Normalize(IVector3 v);

    bool IsFinite(IVector3 v);
}


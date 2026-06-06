public sealed class SeededRng : IRng
{
    private int _state;

    public SeededRng(int seed)
    {
        _state = seed;
    }

    public int Next(int min, int max)
    {
        _state ^= _state << 13;
        _state ^= _state >> 17;
        _state ^= _state << 5;

        var value = (_state & 0x7fffffff);
        return min + (value % (max - min));
    }

    public float NextFloat()
    {
        _state ^= _state << 13;
        _state ^= _state >> 17;
        _state ^= _state << 5;

        return (_state & 0x7fffffff) / (float)int.MaxValue;
    }
}
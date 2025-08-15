namespace Yatzy;

public class Player(string name)
{
    public string Name => name;

    public override bool Equals(object? obj)
    {
        return obj is Player player && Name.Equals(player.Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}

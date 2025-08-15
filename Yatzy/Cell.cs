namespace Yatzy;

public class Cell(Player player, Throw throwEnum)
{
    public Player Player => player;
    public Throw Throw => throwEnum;

    public override bool Equals(object? obj)
    {
        return obj is Cell cell && cell.Player.Equals(Player) && cell.Throw.Equals(Throw);
    }

    public override int GetHashCode()
    {
        return Player.GetHashCode() ^ Throw.GetHashCode();
    }
}

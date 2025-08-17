namespace Yatzy;

public class DiceThrow(Throw @throw, List<IGrouping<uint, Die>> dice)
{
    public List<IGrouping<uint, Die>> Dice => dice;
    public Throw Throw => @throw;
}

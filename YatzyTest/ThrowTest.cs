using Yatzy;

namespace YatzyTest;

public class ThrowTest
{
    [Theory]
    [InlineData(1, 2, 3, 4, 5)]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(5, 4, 3, 2, 1)]
    [InlineData(1, 3, 2, 4, 6, 5)]
    public void TestThatSmallStraightCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.SMALL_STRAIGHT.IsValid(dice) && Throw.SMALL_STRAIGHT.GetValid(dice).Count != 0);
    }
    
    [Theory]
    [InlineData(15, 1, 2, 3, 4, 5)]
    [InlineData(15, 1, 2, 3, 4, 5, 6)]
    [InlineData(15, 5, 4, 3, 2, 1)]
    [InlineData(15, 1, 3, 2, 4, 6, 5)]
    public void TestThatSmallStraightValuesAreCorrect(int expected, params int[] values)
    {
        var combinations = Throw.SMALL_STRAIGHT.GetValid(values.Select(i => Die.Fixed((uint)i)).ToArray());
        Assert.Equal(1, combinations.Count(combination => Throw.SMALL_STRAIGHT.CountDice(combination) == expected));
    }
    
    [Theory]
    [InlineData(2, 3, 4, 5, 6)]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(6, 5, 4, 3, 2)]
    [InlineData(1, 3, 2, 4, 6, 5)]
    public void TestThatBigStraightCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.BIG_STRAIGHT.IsValid(dice) && Throw.BIG_STRAIGHT.GetValid(dice).Count != 0);
    }
    
    [Theory]
    [InlineData(20, 2, 3, 4, 5, 6)]
    [InlineData(20, 1, 2, 3, 4, 5, 6)]
    [InlineData(20, 6, 5, 4, 3, 2)]
    [InlineData(20, 1, 3, 2, 4, 6, 5)]
    public void TestThatBigStraightValuesAreCorrect(int expected, params int[] values)
    {
        var combinations = Throw.BIG_STRAIGHT.GetValid(values.Select(i => Die.Fixed((uint)i)).ToArray());
        Assert.Equal(1, combinations.Count(combination => Throw.BIG_STRAIGHT.CountDice(combination) == expected));
    }
    
    [Theory]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(6, 5, 4, 3, 2, 1)]
    [InlineData(1, 3, 2, 4, 6, 5)]
    public void TestThatRoyalStraightCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.ROYAL_STRAIGHT.IsValid(dice) && Throw.ROYAL_STRAIGHT.GetValid(dice).Count != 0);
    }
    
    [Theory]
    [InlineData(21, 1, 2, 3, 4, 5, 6)]
    [InlineData(21, 6, 5, 4, 3, 2, 1)]
    [InlineData(21, 1, 3, 2, 4, 6, 5)]
    public void TestThatRoyalStraightValuesAreCorrect(int expected, params int[] values)
    {
        var combinations = Throw.ROYAL_STRAIGHT.GetValid(values.Select(i => Die.Fixed((uint)i)).ToArray());
        Assert.Equal(1, combinations.Count(combination => Throw.ROYAL_STRAIGHT.CountDice(combination) == expected));
    }
    
    [Theory]
    [InlineData(6, 6, 6, 5, 5)]
    [InlineData(6, 6, 5, 5, 5)]
    [InlineData(6, 6, 6, 5, 5, 5)]
    [InlineData(6, 5, 6, 5, 6)]
    [InlineData(1, 2, 1, 2, 1)]
    public void TestThatFullHouseCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.FULL_HOUSE.IsValid(dice) && Throw.FULL_HOUSE.GetValid(dice).Count != 0);
    }

    [Theory]
    [InlineData(28, 6, 6, 6, 5, 5)]
    [InlineData(27, 6, 6, 5, 5, 5)]
    [InlineData(18, 4, 3, 3, 4, 4)]
    public void TestThatFullHouseValuesAreCorrect(int expected, params int[] values)
    {
        var combinations = Throw.FULL_HOUSE.GetValid(values.Select(i => Die.Fixed((uint)i)).ToArray());
        Assert.Equal(1, combinations.Count(combination => Throw.FULL_HOUSE.CountDice(combination) == expected));
    }
    
    [Theory]
    [InlineData(6, 6, 6, 5, 5, 5)]
    [InlineData(6, 5, 6, 5, 6, 5)]
    [InlineData(1, 2, 2, 1, 2, 1)]
    public void TestThatThreeSameTwiceCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.THREE_SAME_TWICE.IsValid(dice) && Throw.THREE_SAME_TWICE.GetValid(dice).Count != 0);
    }
    
    [Theory]
    [InlineData(1, 1, 2, 2, 3)]
    [InlineData(1, 1, 2, 2, 3, 4)]
    [InlineData(2, 3, 1, 3, 1)]
    public void TestThatTwoPairsCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.TWO_PAIRS.IsValid(dice) && Throw.TWO_PAIRS.GetValid(dice).Count != 0);
    }
}

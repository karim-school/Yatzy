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
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(6, 5, 4, 3, 2, 1)]
    [InlineData(1, 3, 2, 4, 6, 5)]
    public void TestThatRoyalStraightCanBeFound(params int[] values)
    {
        var dice = values.Select(i => Die.Fixed((uint)i)).ToArray();
        Assert.True(Throw.ROYAL_STRAIGHT.IsValid(dice) && Throw.ROYAL_STRAIGHT.GetValid(dice).Count != 0);
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

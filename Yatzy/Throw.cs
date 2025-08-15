namespace Yatzy;

public enum Throw
{
    ONES,
    TWOS,
    THREES,
    FOURS,
    FIVES,
    SIXES,
    ONE_PAIR,
    TWO_PAIRS,
    THREE_PAIRS,
    THREE_SAME,
    FOUR_SAME,
    THREE_SAME_TWICE,
    SMALL_STRAIGHT,
    BIG_STRAIGHT,
    ROYAL_STRAIGHT,
    FULL_HOUSE,
    CHANCE,
    YATZY
}

public static class ThrowExtensions
{
    public static string GetSimpleName(this Throw e)
    {
        return e switch
        {
            Throw.ONES => "1'ere",
            Throw.TWOS => "2'ere",
            Throw.THREES => "3'ere",
            Throw.FOURS => "4'ere",
            Throw.FIVES => "5'ere",
            Throw.SIXES => "6'ere",
            Throw.ONE_PAIR => "1 par",
            Throw.TWO_PAIRS => "2 par",
            Throw.THREE_PAIRS => "3 par",
            Throw.THREE_SAME => "3 ens",
            Throw.FOUR_SAME => "4 ens",
            Throw.THREE_SAME_TWICE => "2 x 3 ens",
            Throw.SMALL_STRAIGHT => "Lille straight",
            Throw.BIG_STRAIGHT => "Stor straight",
            Throw.ROYAL_STRAIGHT => "Royal straight",
            Throw.FULL_HOUSE => "Hus",
            Throw.CHANCE => "Chance",
            Throw.YATZY => "Yatzy",
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    public static uint GetMaxValue(this Throw e, uint dice)
    {
        return e switch
        {
            Throw.ONES => dice,
            Throw.TWOS => dice * 2,
            Throw.THREES => dice * 3,
            Throw.FOURS => dice * 4,
            Throw.FIVES => dice * 5,
            Throw.SIXES => dice * 6,
            Throw.ONE_PAIR => 12,           // 6 * 2
            Throw.TWO_PAIRS => 22,          // 6 * 2 + 5 * 2
            Throw.THREE_PAIRS => 30,        // 6 * 2 + 5 * 2 + 4 * 2
            Throw.THREE_SAME => 18,         // 6 * 3
            Throw.FOUR_SAME => 24,          // 6 * 4
            Throw.THREE_SAME_TWICE => 33,   // 6 * 3 + 5 * 3
            Throw.SMALL_STRAIGHT => 15,     // 1 + 2 + 3 + 4 + 5
            Throw.BIG_STRAIGHT => 20,       // 2 + 3 + 4 + 5 + 6
            Throw.ROYAL_STRAIGHT => 21,     // 1 + 2 + 3 + 4 + 5 + 6
            Throw.FULL_HOUSE => 28,         // 6 * 3 + 5 * 2
            Throw.CHANCE => dice * 6,
            Throw.YATZY => dice > 5 ? 100u : 50u,
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }
    
    private static IGrouping<uint, Die>? GetSingleDice(Die[] dice, uint eyes)
    {
        return dice.GroupBy(die => die.Value).FirstOrDefault(grouping => grouping.Key == eyes);
    }

    private static List<IGrouping<uint, Die>> GetPossiblePairs(Die[] dice)
    {
        var possible = dice.GroupBy(die => die.Value)
            .Where(grouping => grouping.Count() > 1)
            .ToList();
        List<IGrouping<uint, Die>> pairs = [];
        foreach (var it in possible.Select(grouping => grouping.GetEnumerator()))
        {
            var pair = new Die[2];
            it.MoveNext();
            pair[0] = it.Current;
            it.MoveNext();
            pair[1] = it.Current;
            pairs.AddRange(pair.GroupBy(die => die.Value));
        }
        return pairs;
    }

    private static List<IGrouping<uint, Die>> GetSameDice(Die[] dice, uint count)
    {
        var possible = dice.GroupBy(die => die.Value)
            .Where(grouping => grouping.Count() >= count)
            .ToList();
        List<IGrouping<uint, Die>> same = [];
        foreach (var grouping in possible)
        {
            if (grouping.Count() == count)
            {
                same.Add(grouping);
                continue;
            }
            var temp = new Die[count];
            using var it = grouping.GetEnumerator();
            var i = 0;
            while (it.MoveNext() && i < count) temp[i++] = it.Current;
            same.AddRange(temp.GroupBy(die => die.Value));
        }
        return same;
    }

    // Adapted from https://stackoverflow.com/a/10629938
    private static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IGrouping<uint, Die>
    {
        if (length == 1) return list.Select(t => new[] { t });
        return GetKCombs(list, length - 1)
            .SelectMany(t => list.Where(o => o.Key.CompareTo(t.Last().Key) > 0), 
                (t1, t2) => t1.Concat([t2]));
    }
    
    public static List<List<IGrouping<uint, Die>>> GetValid(this Throw e, Die[] dice)
    {
        List<List<IGrouping<uint, Die>>> validThrows = null;
        IGrouping<uint, Die>? singleDice;
        List<IGrouping<uint, Die>> tempList;
        IEnumerable<IEnumerable<IGrouping<uint, Die>>> combinations;
        switch (e)
        {
            case Throw.ONES:
                singleDice = GetSingleDice(dice, 1);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.TWOS:
                singleDice = GetSingleDice(dice, 2);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.THREES:
                singleDice = GetSingleDice(dice, 3);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.FOURS:
                singleDice = GetSingleDice(dice, 4);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.FIVES:
                singleDice = GetSingleDice(dice, 5);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.SIXES:
                singleDice = GetSingleDice(dice, 6);
                validThrows = singleDice != null ? [[singleDice]] : [];
                break;
            case Throw.ONE_PAIR:
                validThrows = GetPossiblePairs(dice).Select(grouping => (List<IGrouping<uint, Die>>)[grouping]).ToList();
                break;
            case Throw.TWO_PAIRS:
                validThrows = [];
                combinations = GetKCombs(GetPossiblePairs(dice), 2);
                foreach (var combination in combinations)
                {
                    tempList = [];
                    tempList.AddRange(combination);
                    validThrows.Add(tempList);
                }
                break;
            case Throw.THREE_PAIRS:
                validThrows = [];
                combinations = GetKCombs(GetPossiblePairs(dice), 3);
                foreach (var combination in combinations)
                {
                    tempList = [];
                    tempList.AddRange(combination);
                    validThrows.Add(tempList);
                }
                break;
            case Throw.THREE_SAME:
                validThrows = GetSameDice(dice, 3).Select(grouping => (List<IGrouping<uint, Die>>)[grouping]).ToList();
                break;
            case Throw.FOUR_SAME:
                validThrows = GetSameDice(dice, 4).Select(grouping => (List<IGrouping<uint, Die>>)[grouping]).ToList();
                break;
            case Throw.THREE_SAME_TWICE:
                validThrows = [];
                combinations = GetKCombs(GetSameDice(dice, 3), 2);
                foreach (var combination in combinations)
                {
                    tempList = [];
                    tempList.AddRange(combination);
                    validThrows.Add(tempList);
                }
                break;
            case Throw.SMALL_STRAIGHT:
                tempList = dice.GroupBy(die => die.Value).OrderBy(grouping => grouping.Key).Take(5).ToList();
                // Doesn't work with 10 dice: 1,2,3,4,5 + 1,2,3,4,5
                validThrows = tempList.Select(grouping => grouping.Key).SequenceEqual<uint>([1, 2, 3, 4, 5]) ? [tempList.ToList()] : [];
                break;
            case Throw.BIG_STRAIGHT:
                tempList = dice.GroupBy(die => die.Value).OrderBy(grouping => grouping.Key).TakeLast(5).ToList();
                // Doesn't work with 10 dice: 2,3,4,5,6 + 2,3,4,5,6
                validThrows = tempList.Select(grouping => grouping.Key).SequenceEqual<uint>([2, 3, 4, 5, 6]) ? [tempList.ToList()] : [];
                break;
            case Throw.ROYAL_STRAIGHT:
                tempList = dice.GroupBy(die => die.Value).OrderBy(grouping => grouping.Key).Take(6).ToList();
                // Doesn't work with 12 dice: 1,2,3,4,5,6 + 1,2,3,4,5,6
                validThrows = tempList.Select(grouping => grouping.Key).SequenceEqual<uint>([1, 2, 3, 4, 5, 6]) ? [tempList.ToList()] : [];
                break;
            case Throw.FULL_HOUSE:
                validThrows = [];
                tempList = dice.GroupBy(die => die.Value).Where(grouping => grouping.Count() > 1).ToList();
                combinations = GetKCombs(tempList, 2);
                foreach (var combination in combinations)
                {
                    var groupings = combination as IGrouping<uint, Die>[] ?? combination.ToArray();
                    if (!groupings.Any(grouping => grouping.Count() > 2) || !groupings.Any(grouping => grouping.Count() > 1)) continue;
                    List<IGrouping<uint, Die>> newGroupings = [];
                    var foundThree = 0u;
                    foreach (var grouping in groupings)
                    {
                        List<Die> temp = [];
                        using var it = grouping.GetEnumerator();
                        var i = 0;
                        if (grouping.Count() > 2)
                        {
                            if (foundThree == 0)
                            {
                                while (it.MoveNext() && i++ < 3) temp.Add(it.Current);
                                foundThree = grouping.Key;
                            }
                            else
                            {
                                while (it.MoveNext() && i++ < 2) temp.Add(it.Current);
                                List<Die> reverseTemp = [];
                                it.Reset();
                                i = 0;
                                while (it.MoveNext() && i++ < 3) reverseTemp.Add(it.Current);
                                using var itPrevious = newGroupings[0].GetEnumerator();
                                i = 0;
                                while (itPrevious.MoveNext() && i++ < 2) reverseTemp.Add(itPrevious.Current);
                                validThrows.Add(reverseTemp.GroupBy(die => die.Value).ToList());
                            }
                        }
                        else
                        {
                            temp.AddRange(grouping);
                        }
                        newGroupings.AddRange(temp.GroupBy(die => die.Value));
                    }
                    validThrows.Add(newGroupings);
                }
                break;
            case Throw.CHANCE:
                validThrows = [dice.GroupBy(die => die.Value).ToList()];
                break;
            case Throw.YATZY:
                tempList = dice.GroupBy(die => die.Value).DistinctBy(grouping => grouping.Key).ToList();
                validThrows = tempList.Count == 1 ? [tempList] : [];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e, null);
        }
        return validThrows;
    }
    
    public static bool IsValid(this Throw e, Die[] dice)
    {
        return e switch
        {
            Throw.ONES => true,
            Throw.TWOS => true,
            Throw.THREES => true,
            Throw.FOURS => true,
            Throw.FIVES => true,
            Throw.SIXES => true,
            Throw.ONE_PAIR => dice.GroupBy(die => die.Value).Any(grouping => grouping.Count() > 1),
            Throw.TWO_PAIRS => dice.GroupBy(die => die.Value).Count(grouping => grouping.Count() > 1) > 1,
            Throw.THREE_PAIRS => dice.GroupBy(die => die.Value).Count(grouping => grouping.Count() > 1) > 2,
            Throw.THREE_SAME => dice.GroupBy(die => die.Value).Any(grouping => grouping.Count() > 2),
            Throw.FOUR_SAME => dice.GroupBy(die => die.Value).Any(grouping => grouping.Count() > 3),
            Throw.THREE_SAME_TWICE => dice.GroupBy(die => die.Value).Count(grouping => grouping.Count() > 2) > 1,
            Throw.SMALL_STRAIGHT => dice.Select(die => die.Value).Order().Take(5).SequenceEqual<uint>([1,2,3,4,5]),
            Throw.BIG_STRAIGHT => dice.Select(die => die.Value).Order().TakeLast(5).SequenceEqual<uint>([2,3,4,5,6]),
            Throw.ROYAL_STRAIGHT => dice.Select(die => die.Value).Order().Take(6).SequenceEqual<uint>([1,2,3,4,5,6]),
            Throw.FULL_HOUSE => GetValid(e, dice).Count != 0,
            Throw.CHANCE => true,
            Throw.YATZY => dice.Select(die => die.Value).Distinct().Count() == 1,
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    public static uint CountDice(this Throw e, List<IGrouping<uint, Die>> dice)
    {
        if (dice.Count == 0) return 0;
        List<Die> validDice = [];
        var it = dice.GetEnumerator();
        while (it.MoveNext()) validDice.AddRange(it.Current);
        return (uint)validDice.Sum(die => die.Value);
    }
}

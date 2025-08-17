using System.Text;

namespace Yatzy;

public class Yatzy
{
    private static readonly Random Random = new Random();

    private readonly Throw[] _throws;
    private readonly Die[] _dice;
    private readonly Player[] _players;
    private readonly Dictionary<Cell, uint> _values;
    private readonly List<DiceThrow> _options;

    private int _currentPlayer;
    
    public Yatzy(uint dice, string[] players)
    {
        if (dice < 5 || dice > 6) throw new ArgumentException("Only 5 or 6 dice are supported");
        _throws = GetThrows(dice);
        _dice = new Die[dice];
        _players = players.Select(name => new Player(name.ToUpper())).ToArray();
        _values = new Dictionary<Cell, uint>(_players.Length * _throws.Length);
        for (var i = 0; i < _dice.Length; i++) _dice[i] = new Die();
        _options = [];
    }

    public void Start()
    {
        Console.WriteLine(BuildBoard());
        var player = _players[_currentPlayer];
        while (GetRemainingThrows(player).Length != 0)
        {
            Console.WriteLine($"It is {player.Name}'s turn.");
            RollDice();
            List<string> actions;
            var action = "reroll";
            var rolls = 3;
            while (action.Equals("reroll") && rolls-- > 0)
            {
                Console.WriteLine($"You rolled: {string.Join(", ", _dice.Select(die => die.Value))}");
                ListOptions(player);
                Console.WriteLine();
                actions = ["board", "cross"];
                if (_options.Count > 0) actions.Add("use");
                if (rolls > 0) actions.Add("reroll");
                Console.WriteLine("You can undo an action by typing 'undo'");
                var redo = true;
                while (redo)
                {
                    action = GetString($"What action would you like to take? [{string.Join(", ", actions)}] ", input => actions.Any(a => a.Equals(input.ToLowerInvariant())));
                    while (action.Equals("board"))
                    {
                        Console.WriteLine(BuildBoard());
                        Console.WriteLine($"It is {player.Name}'s turn.");
                        Console.WriteLine($"You rolled: {string.Join(", ", _dice.Select(die => die.Value))}");
                        ListOptions(player);
                        Console.WriteLine();
                        action = GetString($"What action would you like to take? [{string.Join(", ", actions)}] ", input => actions.Any(a => a.Equals(input.ToLowerInvariant())));
                    }
                    switch (action)
                    {
                        case "use":
                        {
                            var choiceIndex = GetUnsignedInt("Which combination would you like to go with? ", true, u => u > 0 && u <= _options.Count);
                            if (choiceIndex == null) break;
                            var choice = _options[(int)(choiceIndex - 1)];
                            Insert(player, choice.Throw, choice.Throw.CountDice(choice.Dice));
                            redo = false;
                            break;
                        }
                        case "cross":
                        {
                            var throws = GetRemainingThrows(player);
                            for (var i = 0; i < throws.Length; i++) Console.WriteLine($"[{i + 1}] {throws[i].GetSimpleName()}");
                            var crossThrow = GetUnsignedInt("Which throw would you like to cross? ", true, u => u > 0 && u <= throws.Length);
                            if (crossThrow == null) break;
                            Insert(player, throws[(int)(crossThrow - 1)], 0);
                            redo = false;
                            break;
                        }
                        case "reroll":
                        {
                            Console.WriteLine(string.Join("\t", _dice.Select((die, i) => $"[{i + 1}] {die.Value}")));
                            var keep = GetUnsignedIntSet("Which dice would you like to keep? (comma separated list) ", true, u => u > 0 && u <= _dice.Length);
                            if (keep == null) break;
                            foreach (var die in _dice.Where((_, i) => !keep.Contains((uint)(i + 1)))) die.Roll();
                            redo = false;
                            break;
                        }
                    }                        
                }
            }
            player = _players[++_currentPlayer % _players.Length];
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine();
        }
        Dictionary<Player, uint> sums = [];
        foreach (var participant in _players)
        {
            var sum = 0u;
            var bonus = false;
            for (var i = 0; i < _throws.Length; i++)
            {
                if (i == 6 && sum >= (_dice.Length > 5 ? 84 : 63)) bonus = true;
                if (!_values.TryGetValue(new Cell(participant, _throws[i]), out var value)) continue;
                sum += value;
            }
            if (bonus) sum += _dice.Length > 5 ? 100u : 50u;
            sums[participant] = sum;
        }
        var orderedEntries = sums.OrderByDescending(entry => entry.Value).ToList();
        Console.WriteLine(BuildBoard());
        Console.WriteLine("Game is over");
        Console.WriteLine($"The winner is: {orderedEntries[0].Key.Name}");
    }

    private Throw[] GetRemainingThrows(Player player)
    {
        var throws = _values.Where(entry => entry.Key.Player.Equals(player)).Select(entry => entry.Key.Throw).ToArray();
        return _throws.Where(@throw => !throws.Contains(@throw)).ToArray();
    }
    
    private static uint[]? GetUnsignedIntSet(string prompt, bool allowUndo, Predicate<uint>? validator = null)
    {
        uint[] result = [];
        var valid = false;
        do
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input == null) continue;
            if (allowUndo && input.ToLowerInvariant().Equals("undo")) return null;
            var members = input.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            List<uint> integers = [];
            foreach (var member in members)
            {
                if (!uint.TryParse(member, out var u)) break;
                if (!integers.Contains(u)) integers.Add(u);
            }
            valid = integers.Count == members.Length;
            if (valid) result = integers.ToArray();
        } while (!valid || (validator != null && !result.All(u => validator(u))));
        return result;
    }

    private static uint? GetUnsignedInt(string prompt, bool allowUndo, Predicate<uint>? validator = null)
    {
        uint result;
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
            if (input == null) continue;
            if (allowUndo && input.ToLowerInvariant().Equals("undo")) return null;
        } while (!uint.TryParse(input, out result) || (validator != null && !validator(result)));
        return result;
    }
    
    private static string GetString(string prompt, Predicate<string>? validator = null)
    {
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        } while (input != null && validator != null && !validator(input));
        return input!;
    }

    private void RollDice()
    {
        foreach (var die in _dice) die.Roll();
    }

    private void Insert(Player player, Throw @throw, uint value)
    {
        _values[new Cell(player, @throw)] = value;
        if (value == 0) return;
        if (@throw == Throw.YATZY) _values[new Cell(player, @throw)] += _dice.Length > 5 ? 100u : 50u;
    }

    private void ListOptions(Player player)
    {
        _options.Clear();
        foreach (var @throw in GetRemainingThrows(player))
        {
            var valid = @throw.GetValid(_dice);
            foreach (var combinations in valid)
            {
                _options.Add(new DiceThrow(@throw, combinations));
                var value = @throw.CountDice(combinations);
                Console.WriteLine($"[{_options.Count}] - {@throw.GetSimpleName()} ({value}): {string.Join(", ", combinations.Select(grouping => $"{grouping.Count()}x{grouping.Key}"))}");
            }
        }
    }

    private string[] ConcatDice(uint offset, uint size, int gap)
    {
        var array = new string[5];
        for (var i = offset; i < offset + size; i++)
        {
            var lines = _dice[i].ToArray();
            for (var j = 0; j < array.Length; j++)
            {
                array[j] += (i != offset ? new string(' ', gap) : string.Empty) + lines[j];
            }
        }
        return array;
    }

    private string BuildHeader(string content)
    {
        var playerNameCells = string.Join(" | ", _players.Select(x => x.Name.PadRight(3)));
        var row = $"| {content,-19} | {playerNameCells} |";
        var hr = new string('-', row.Length);
        return $"{hr}\n{row}\n{hr}";
    }

    private string BuildThrow(Throw e)
    {
        var rowHeader = $"| {e.GetSimpleName(),-15} {e.GetMaxValue((uint)_dice.Length),3}";
        var playerCells = new StringBuilder();
        foreach (var player in _players)
        {
            if (_values.TryGetValue(new Cell(player, e), out var value))
            {
                if (value == 0) playerCells.Append("  -  |");
                else playerCells.Append($" {value,3} |");
            }
            else playerCells.Append($"     |");
        }
        return $"{rowHeader} |{playerCells}";
    }

    private string BuildBonus()
    {
        var sums = new StringBuilder($"| {"SUM",-19} |");
        var bonuses = new StringBuilder($"| {"Bonus",-15} {50,3} |");
        foreach (var player in _players)
        {
            var sum = 0u;
            foreach (var @throw in _throws.Take(6))
            {
                if (!_values.TryGetValue(new Cell(player, @throw), out var value)) continue;
                sum += value;
            }
            sums.Append($" {sum,3} |");
            if (sum > (_dice.Length > 5 ? 84 : 63)) bonuses.Append($" {(_dice.Length > 5 ? 100u : 50u),3} |");
            else if (_throws.Take(6).Count(@throw => _values.TryGetValue(new Cell(player, @throw), out _)) == 6) bonuses.Append($"  -  |");
            else bonuses.Append($"     |");
        }
        return $"{sums}\n{bonuses}";
    }

    private string BuildSum()
    {
        var sums = new StringBuilder($"| {"SUM",-19} |");
        foreach (var player in _players)
        {
            var sum = 0u;
            var bonus = false;
            for (var i = 0; i < _throws.Length; i++)
            {
                if (i == 6 && sum >= (_dice.Length > 5 ? 84 : 63)) bonus = true;
                if (!_values.TryGetValue(new Cell(player, _throws[i]), out var value)) continue;
                sum += value;
            }
            if (bonus) sum += _dice.Length > 5 ? 100u : 50u;
            sums.Append($" {sum,3} |");
        }
        return sums.ToString();
    }

    private string BuildBoard()
    {
        var builder = new StringBuilder();
        var header = BuildHeader("Maximum points");
        var hr = header[..header.IndexOf('\n')];
        builder.AppendLine(header);
        foreach (var @throw in _throws.Take(6))
        {
            builder.AppendLine(BuildThrow(@throw));
        }
        builder.AppendLine(hr);
        builder.AppendLine(BuildBonus());
        builder.AppendLine(hr);
        foreach (var @throw in _throws.Skip(6))
        {
            builder.AppendLine(BuildThrow(@throw));
        }
        builder.AppendLine(hr);
        builder.AppendLine(BuildSum());
        builder.AppendLine(hr);
        return builder.ToString();
    }
    
    public string GetBoardString()
    {
        var builder = new StringBuilder();
        var drawDice = _dice.Any(die => die.Value != 0);
        var playerColumns = new StringBuilder();
        foreach (var player in _players) playerColumns.Append($" {player.Name,3} |");
        var hr = "-----------------------" + new string('-', playerColumns.Length);
        builder.AppendLine(hr);
        builder.AppendLine($"| Maximum points      |{playerColumns}");
        builder.AppendLine(hr);
        var emptyCells = string.Concat(Enumerable.Repeat("     |", _players.Length));
        if (drawDice)
        {
            var dice = ConcatDice(0, 3, 3);
            var margin = new string(' ', 5);
            builder.Append($"| 1'ere            {_dice.Length,2} |{emptyCells}").Append(margin).Append(dice[0]).AppendLine();
            builder.Append($"| 2'ere            {_dice.Length * 2,2} |{emptyCells}").Append(margin).Append(dice[1]).AppendLine();
            builder.Append($"| 3'ere            {_dice.Length * 3,2} |{emptyCells}").Append(margin).Append(dice[2]).AppendLine();
            builder.Append($"| 4'ere            {_dice.Length * 4,2} |{emptyCells}").Append(margin).Append(dice[3]).AppendLine();
            builder.Append($"| 5'ere            {_dice.Length * 5,2} |{emptyCells}").Append(margin).Append(dice[4]).AppendLine();
        }
        else
        {
            builder.AppendLine($"| 1'ere            {_dice.Length,2} |{emptyCells}");
            builder.AppendLine($"| 2'ere            {_dice.Length * 2,2} |{emptyCells}");
            builder.AppendLine($"| 3'ere            {_dice.Length * 3,2} |{emptyCells}");
            builder.AppendLine($"| 4'ere            {_dice.Length * 4,2} |{emptyCells}");
            builder.AppendLine($"| 5'ere            {_dice.Length * 5,2} |{emptyCells}");
        }
        builder.AppendLine($"| 6'ere            {_dice.Length * 6,2} |{emptyCells}");
        if (drawDice)
        {
            var dice = ConcatDice(3, _dice.Length > 5 ? 3u : 2u, 3);
            var margin = new string(' ', _dice.Length > 5 ? 5 : 13);
            builder.Append(hr).Append(margin).Append(dice[0]).AppendLine();
            builder.Append($"| SUM                 |{emptyCells}").Append(margin).Append(dice[1]).AppendLine();
            builder.Append($"| Bonus           {(_dice.Length > 5 ? 100 : 50),3} |{emptyCells}").Append(margin).Append(dice[2]).AppendLine();
            builder.Append(hr).Append(margin).Append(dice[3]).AppendLine();
            builder.Append($"| 1 par            12 |{emptyCells}").Append(margin).Append(dice[4]).AppendLine();
        }
        else
        {
            builder.AppendLine($"-----------------------------------------------------");
            builder.AppendLine($"| SUM                 |    |    |    |    |    |    |");
            builder.AppendLine($"| Bonus           {(_dice.Length > 5 ? 100 : 50),3} |    |    |    |    |    |    |");
            builder.AppendLine($"-----------------------------------------------------");
            builder.AppendLine($"| 1 par            12 |    |    |    |    |    |    |");
        }
        builder.AppendLine($"| 2 par            22 |{emptyCells}");
        builder.AppendLine($"| 3 ens            18 |{emptyCells}");
        builder.AppendLine($"| 4 ens            24 |{emptyCells}");
        builder.AppendLine($"| Lille straight   15 |{emptyCells}");
        builder.AppendLine($"| Stor straight    20 |{emptyCells}");
        builder.AppendLine($"| Hus              28 |{emptyCells}");
        builder.AppendLine($"| Chance           {_dice.Length * 6,2} |{emptyCells}");
        builder.AppendLine($"| YATZY           {(_dice.Length > 5 ? 100 : 50),3} |{emptyCells}");
        builder.AppendLine(hr);
        builder.AppendLine($"| SUM                 |{emptyCells}");
        builder.AppendLine(hr);
        return builder.ToString();
    }

    private static Throw[] GetThrows(uint dice)
    {
        return dice switch
        {
            5 => [
                Throw.ONES,
                Throw.TWOS,
                Throw.THREES,
                Throw.FOURS,
                Throw.FIVES,
                Throw.SIXES,
                Throw.ONE_PAIR,
                Throw.TWO_PAIRS,
                Throw.THREE_SAME,
                Throw.FOUR_SAME,
                Throw.SMALL_STRAIGHT,
                Throw.BIG_STRAIGHT,
                Throw.FULL_HOUSE,
                Throw.CHANCE,
                Throw.YATZY
            ],
            6 => [
                Throw.ONES,
                Throw.TWOS,
                Throw.THREES,
                Throw.FOURS,
                Throw.FIVES,
                Throw.SIXES,
                Throw.ONE_PAIR,
                Throw.TWO_PAIRS,
                Throw.THREE_PAIRS,
                Throw.THREE_SAME,
                Throw.FOUR_SAME,
                Throw.THREE_SAME_TWICE,
                Throw.SMALL_STRAIGHT,
                Throw.BIG_STRAIGHT,
                Throw.ROYAL_STRAIGHT,
                Throw.FULL_HOUSE,
                Throw.CHANCE,
                Throw.YATZY
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(dice), dice, null)
        };
    }
}

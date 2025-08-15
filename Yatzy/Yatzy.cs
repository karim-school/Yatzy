using System.Text;

namespace Yatzy;

public class Yatzy
{
    private static readonly Random Random = new Random();

    private readonly Throw[] _throws;
    private readonly Die[] _dice;
    private readonly Player[] _players;
    private readonly Dictionary<Cell, uint> _values;

    private int _currentPlayer = -1;
    private List<IGrouping<uint, Die>>[] _options; // TODO: Make 'Choice' class with field for Throw enum
    private bool _done;
    
    public Yatzy(uint dice, string[] players)
    {
        if (dice < 5 || dice > 6) throw new ArgumentException("Only 5 or 6 dice are supported");
        _throws = GetThrows(dice);
        _dice = new Die[dice];
        _players = players.Select(name => new Player(name.ToUpper())).ToArray();
        _values = new Dictionary<Cell, uint>(_players.Length * _throws.Length);
        for (var i = 0; i < _dice.Length; i++) _dice[i] = new Die();
    }

    public void Start()
    {
        if (_done) return;
        Console.WriteLine(BuildBoard());
        do
        {
            _currentPlayer = (_currentPlayer + 1) % _players.Length;
            var player = _players[_currentPlayer];
            Console.WriteLine($"It is {player.Name}'s turn.");
            RollDice();
            Console.WriteLine($"You rolled: {string.Join(", ", _dice.Select(die => die.Value))}");
            ListOptions();
            var choiceIndex = GetUnsignedInt("Which combination would you like to go with?", u => u > 0 && u <= _options.Length);
            var choice = _options[choiceIndex - 1];
            Insert(player, null, choice);
            _done = true;
        } while (!_done);
        // TODO: Announce winner
        Console.WriteLine("Game is over");
    }

    private static uint GetUnsignedInt(string prompt, Predicate<uint>? validator = null)
    {
        uint result;
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        } while (!uint.TryParse(input, out result) || (validator != null && !validator(result)));
        return result;
    }

    private void RollDice()
    {
        foreach (var die in _dice) die.Roll();
    }

    private void Insert(Player player, Throw @throw, List<IGrouping<uint, Die>> choice)
    {
        _values[new Cell(player, @throw)] = @throw.CountDice(choice);
    }

    private void ListOptions()
    {
        foreach (var @throw in _throws)
        {
            var valid = @throw.GetValid(_dice);
            _options = new List<IGrouping<uint, Die>>[valid.Count];
            for (var i = 0; i < valid.Count; i++)
            {
                var combinations = valid[i];
                _options[i] = combinations;
                var value = @throw.CountDice(combinations);
                Console.WriteLine($"[{i + 1}] - {@throw.GetSimpleName()} ({value}): {string.Join(", ", combinations.Select(grouping => $"{grouping.Count()}x{grouping.Key}"))}");
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
            if (_values.TryGetValue(new Cell(player, e), out var value)) playerCells.Append($" {value,3} |");
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
            bonuses.Append($" {(sum > (_dice.Length > 5 ? 84 : 63) ? " X " : ""),3} |");
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
            if (bonus) sum += 50;
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

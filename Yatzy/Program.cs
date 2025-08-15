namespace Yatzy;

class Program
{
    private static void Main(string[] args)
    {
        /*
        var dice = new[]
        {
            Die.Fixed(1),
            Die.Fixed(1),
            Die.Fixed(1),
            Die.Fixed(3),
            Die.Fixed(3),
            Die.Fixed(3),
            Die.Fixed(3),
        };
        const Throw throwEnum = Throw.FULL_HOUSE;
        Console.WriteLine("IsValid: " + throwEnum.IsValid(dice));
        var valid = throwEnum.GetValid(dice);
        foreach (var combination in valid)
        {
            var validString = string.Join(", ", combination.Select(grouping => $"{grouping.Key}x{grouping.Count()}"));
            Console.WriteLine("Valid : " + validString);
            Console.WriteLine("Points: " + throwEnum.CountDice(combination));
        }
        if (valid.Count == 0)
        {
            Console.WriteLine("Valid : None");
            Console.WriteLine("Points: 0");
        }
        */
        /*
        Console.WriteLine("Unique players separated by commas. Max 3 characters per player.");
        var players = GetUniquePlayers("Players: ");
        Console.Clear();
        Console.WriteLine("Do you want to play yatzy with 5 or 6 dice?");
        var dice = GetUnsignedInt("Dice: ", d => d == 5 || d == 6);
        Console.Clear();
        var yatzy = new Yatzy(dice, players);
        Console.WriteLine(yatzy);
        */
        var yatzy = new Yatzy(6, ["a", "b", "c"]);
        yatzy.Start();
        //yatzy.ListOptions();
        //Console.Write("Which dice do you want to keep? ");
        //Console.ReadLine();
    }

    private static string[] GetUniquePlayers(string prompt)
    {
        string[]? players;
        do
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            players = input?.Split(',');
        } while (players?.Length < 2 || players?.Distinct().Count() != players?.Length || (bool)players?.Any(name => name.Length > 3 || name.Length == 0));
        return players!;
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
}

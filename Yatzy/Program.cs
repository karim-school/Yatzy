namespace Yatzy;

class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Unique players separated by commas. Max 3 characters per player.");
        var players = GetUniquePlayers("Players: ");
        Console.Clear();
        Console.WriteLine("Do you want to play yatzy with 5 or 6 dice?");
        var dice = GetUnsignedInt("Dice: ", d => d == 5 || d == 6);
        Console.Clear();
        var yatzy = new Yatzy(dice, players);
        yatzy.Start();
    }

    private static string[] GetUniquePlayers(string prompt)
    {
        string[]? players;
        do
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            players = input?.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        } while (players?.Length < 2 || players?.Distinct().Count() != players?.Length || players!.Any(name => name.Length > 3 || name.Length == 0));
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

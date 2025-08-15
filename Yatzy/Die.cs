using System.Text;

namespace Yatzy;

public class Die
{
    private static readonly Random Random = new Random();

    public uint Value { get; private set; }

    public uint Roll()
    {
        return Value = (uint)Random.Next(1, 7);
    }

    public string[] ToArray()
    {
        var array = new string[5];
        array[0] = "-------------";
        switch (Value)
        {
            case 1:
                array[1] = "|           |";
                array[2] = "|     *     |";
                array[3] = "|           |";
                break;
            case 2:
                array[1] = "|  *        |";
                array[2] = "|           |";
                array[3] = "|        *  |";
                break;
            case 3:
                array[1] = "|        *  |";
                array[2] = "|     *     |";
                array[3] = "|  *        |";
                break;
            case 4:
                array[1] = "|  *     *  |";
                array[2] = "|           |";
                array[3] = "|  *     *  |";
                break;
            case 5:
                array[1] = "|  *     *  |";
                array[2] = "|     *     |";
                array[3] = "|  *     *  |";
                break;
            case 6:
                array[1] = "|  *     *  |";
                array[2] = "|  *     *  |";
                array[3] = "|  *     *  |";
                break;
        }
        array[4] = "-------------";
        return array;
    }

    public override string ToString()
    {
        return string.Join('\n', ToArray());
    }

    public static Die Fixed(uint value)
    {
        return new Die { Value = value };
    }
}

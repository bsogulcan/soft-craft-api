using System.Text;

namespace Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder InsertTab(this StringBuilder stringBuilder, int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            stringBuilder.Append('\t');
        }

        return stringBuilder;
    }

    public static StringBuilder NewLine(this StringBuilder stringBuilder, int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            stringBuilder.Append(Environment.NewLine);
        }

        return stringBuilder;
    }
}
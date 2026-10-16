namespace CopyDate;

internal static class ChineseNumerals
{
    private static readonly string[] Digit = ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九"];

    public static string Year(int year) => year == 1 ? "元" : Number(year);

    public static string Number(int value)
    {
        if (value is < 1 or > 9999)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (value < 10)
        {
            return Digit[value];
        }

        if (value < 100)
        {
            return Tens(value, leadingYi: false);
        }

        if (value < 1000)
        {
            return Hundreds(value);
        }

        return Thousands(value);
    }

    private static string Tens(int value, bool leadingYi)
    {
        int tens = value / 10;
        int ones = value % 10;
        string head = tens == 1
            ? (leadingYi ? "一十" : "十")
            : Digit[tens] + "十";
        return ones == 0 ? head : head + Digit[ones];
    }

    private static string Hundreds(int value)
    {
        int hundreds = value / 100;
        int rest = value % 100;
        string head = Digit[hundreds] + "百";
        if (rest == 0)
        {
            return head;
        }

        if (rest < 10)
        {
            return head + "零" + Digit[rest];
        }

        return head + Tens(rest, leadingYi: true);
    }

    private static string Thousands(int value)
    {
        int thousands = value / 1000;
        int rest = value % 1000;
        string head = Digit[thousands] + "千";
        if (rest == 0)
        {
            return head;
        }

        if (rest < 10)
        {
            return head + "零" + Digit[rest];
        }

        if (rest < 100)
        {
            return head + "零" + Tens(rest, leadingYi: true);
        }

        return head + Hundreds(rest);
    }
}

namespace Firefly.Models;

public class FontSizeItem
{
    public static FontSizeItem[] FontSizes { get; } =
        [
        new FontSizeItem("初号 (42)",42) ,
        new FontSizeItem("小初 (36)", 36),
        new FontSizeItem("一号 (26)",  26 ),
        new FontSizeItem("小一 (24)",  24),
        new FontSizeItem("二号 (22)",  22),
        new FontSizeItem("小二 (18)",  18),
        new FontSizeItem("三号 (16)",  16),
        new FontSizeItem("小三 (15)",  15),
        new FontSizeItem("四号 (14)",  14),
        new FontSizeItem("小四 (12)",  12),
        new FontSizeItem("五号 (10.5)",  10.5 ),
        new FontSizeItem("小五 (9)",  9),
        new FontSizeItem("六号 (7.5)",  7.5),
        new FontSizeItem("小六 (6.5)",  6.5),
        new FontSizeItem("七号 (5.5)",  5.5),
        new FontSizeItem("八号 (5)",  5),
        new FontSizeItem("8",  8),
        new FontSizeItem("9",  9),
        new FontSizeItem("10",  10),
        new FontSizeItem("11", 11),
        new FontSizeItem("12", 12),
        new FontSizeItem("14",  14),
        new FontSizeItem("16",  16),
        new FontSizeItem("18",  18),
        new FontSizeItem("20",  20),
        new FontSizeItem("22",  22),
        new FontSizeItem("24",  24),
        new FontSizeItem("26",  26),
        new FontSizeItem("28",  28),
        new FontSizeItem("36",  36),
        new FontSizeItem("48",  48),
        new FontSizeItem("72",  72)];

    public string Name { get; set; }

    public double Size { get; set; }

    public FontSizeItem(string name, double size)
    {
        Name = name;
        Size = size;
    }
}

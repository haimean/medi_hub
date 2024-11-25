using System.Collections.Generic;

namespace DashboardApi.Utils;

public class CommoncheckColor
{
    private static List<string> colorList =
        new List<string>
        {
            "Aqua",
            "Coral",
            "Black",
            "Blue",
            "Beige",
            "BlueViolet",
            "Brown",
            "BurlyWood",
            "Chartruse",
            "Chocolate",
            "Crimson",
            "DarkBlue",
            "DarkCyan",
            "DarkGreen",
            "DarkOrange",
            "DarkOrchid",
            "DarkRed",
            "DarkOliveGreen",
            "DarkMagenta",
            "deeppink",
            "DeepSkyBlue",
            "Olive",
            "ForestGreen",
            "Fuchsia",
            "Teal",
            "GreenYellow",
            "Gray",
            "Gold",
            "goldenrod",
            "HotPink",
            "Indigo",
            "Lightsalmon",
            "LightCoral",
            "LightGreen",
            "Lime",
            "RoyalBlue",
            "SaddleBrown",
            "SteelBlue",
            "Salmon",
            "Silver",
            "Sienna",
            "Tomato",
            "SpringGreen",
            "Purple",
            "PowderBlue",
            "Maroon",
            "MediumOrchid",
            "MediumPurple",
            "MediumTurquoise",
            "Navy",
            "FireBrick",
        };

    public static string getColor(int index)
    {
        if (colorList.ElementAtOrDefault(index) != null)
        {
            return colorList[index];
        } else
        {
            return "Red";
        }
    }
}
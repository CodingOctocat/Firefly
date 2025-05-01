using System;
using System.Linq;

using Firefly.Extensions;

namespace Firefly.Models;

public class CccfFieldFeature
{
    public string Field { get; }

    public int Length { get; }

    public int LetterCount { get; }

    public double LetterRatio { get; }

    public int NumberCount { get; }

    public double NumberRatio { get; }

    public int OtherCharCount { get; }

    public double OtherCharRatio { get; }

    public int SymbolCount { get; }

    public double SymbolRatio { get; }

    public int WhiteSpaceCount { get; }

    public double WhiteSpaceRatio { get; }

    public CccfFieldFeature(string field)
    {
        Field = field.CleanText();
        Length = Field.Length;
        LetterCount = Field.Count(Char.IsAsciiLetter);
        LetterRatio = (double)LetterCount / Length;
        NumberCount = Field.Count(Char.IsAsciiDigit);
        NumberRatio = (double)NumberCount / Length;
        SymbolCount = Field.Count(Char.IsPunctuation);
        SymbolRatio = (double)SymbolCount / Length;
        WhiteSpaceCount = Field.Count(Char.IsWhiteSpace);
        WhiteSpaceRatio = (double)WhiteSpaceCount / Length;
        OtherCharCount = Length - LetterCount - NumberCount - SymbolCount - WhiteSpaceCount;
        OtherCharRatio = (double)OtherCharCount / Length;
    }

    public override string ToString()
    {
        return $"{Field} | {Length} | {LetterCount} | {LetterRatio:F2} | {NumberCount} | {NumberRatio:F2} | {SymbolCount} | {SymbolRatio:F2} | {WhiteSpaceCount} | {WhiteSpaceRatio:F2} | {OtherCharCount} | {OtherCharRatio:F2}";
    }
}

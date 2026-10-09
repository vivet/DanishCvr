using System;
using System.Globalization;
using DanishCvr.Responses.Models;

namespace DanishCvr.Extensions;

/// <summary>
/// String Extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// To String Pretty.
    /// Formatted with lowercase, and first word capitalized.
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns>The pretty string.</returns>
    public static string ToStringPretty(this string input)
    {
        if (input == null) 
            throw new ArgumentNullException(nameof(input));

        input = input
            .Replace('_', ' ')
            .Trim()
            .ToLower();

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        return char.ToUpper(input[0]) + input[1..];
    }

    /// <summary>
    /// Get Year And Month.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The <see cref="MonthAndDay"/>.</returns>
    public static MonthAndDay GetYearAndMonth(this string input)
    {
        if (input == null) 
            throw new ArgumentNullException(nameof(input));

        var parts = input[1..].Split('-');

        if (string.IsNullOrEmpty(input) || input[0] != '-' || parts.Length != 3)
        {
            return null;
        }

        var successMonth = int.TryParse(parts[1], out var month);
        var successDay = int.TryParse(parts[2], out var day);

        if (successDay && successMonth)
        {
            return new MonthAndDay
            {
                Month = month,
                Day = day
            };
        }
        
        return null;
    }

    /// <summary>
    /// Try Parse Bool.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    public static bool TryParseBool(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var success = bool.TryParse(input, out var @bool);

        return success && @bool;
    }

    /// <summary>
    /// Try Parse Double.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="numberFormatInfo">The <see cref="NumberFormatInfo"/>.</param>
    /// <returns>The <see cref="double"/>.</returns>
    public static double TryParseDouble(this string input, NumberFormatInfo numberFormatInfo)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var success = double.TryParse(input, numberFormatInfo, out var @double);

        if (success)
        {
            return @double;
        }

        return 0.00D;
    }

    /// <summary>
    /// Try Parse Date Only.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The <see cref="DateOnly"/>.</returns>
    public static DateOnly? TryParseDateOnly(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var success = DateOnly.TryParse(input, out var dateOnly);

        return success
            ? dateOnly
            : null;
    }
}
using System;
using System.Text.RegularExpressions;

public class FinnishReferenceNumber : ReferenceNumber
{
    public FinnishReferenceNumber(string refn)
        : base(refn)
    {

    }


    /// <summary>
    /// Validates a finnish invoice reference number.
    /// </summary>
    /// <param name="refn">Whitespace ignored.</param>
    /// <returns></returns>
    public static bool IsValidReferenceNumber(string refn)
    {
        if (refn == null)
            throw new ArgumentNullException("refn");

        if (refn.Length == 0)
            return false;


        int[] weights = new int[] { 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1 };

        char[] reversed = Regex.Replace(refn, "\\s", "").ToCharArray(0, refn.Length - 1);
        Array.Reverse(reversed);

        int sum = 0;

        for (int i = 0; i < reversed.Length; i++)
        {
            sum += weights[i] * ((int)reversed[i] - 48);
        }

        int checksum = 10 - (sum % 10);

        return refn.Substring(refn.Length - 1, 1) == checksum.ToString();
    }


    public string RefNumberForPrint
    {
        get
        {
            return insertSpaces(RefNumber.TrimStart('0'), 5, false);
        }
    }
    private string insertSpaces(string original, int interval, bool left = true)
    {
        if (original == null)
            throw new ArgumentNullException("original");

        int origLen = original.Length;
        int spaces = (original.Length / interval);
        string spaced = original;

        for (int i = 1; i <= spaces; i++)
        {
            int index = i * interval + (i - 1);
            if (index == origLen + i - 1) break;
            if (left)
                spaced = spaced.Insert(index, " ");
            else
                spaced = spaced.Insert(spaced.Length - index, " ");
        }

        return spaced;
    }
}
public class ReferenceNumber
{
    public readonly string RefNumber;

    public ReferenceNumber(string refn)
    {
        RefNumber = refn;
    }
}
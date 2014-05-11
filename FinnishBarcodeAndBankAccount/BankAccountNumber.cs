using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class FinnishBankAccountNumber : BankAccountNumber
{
    private static CultureInfo invariantCulture = CultureInfo.InvariantCulture;
    private static Regex rxNotNumber = new Regex(@"[^\d]", RegexOptions.Compiled);
        
    /// <summary>
    /// Pre 2012 BBAN in the nnnnnn-nn[nnnnnn] format
    /// </summary>
    public string BBAN;

    /// <summary>
    /// Pre 2012 BBAN in the 14 character nnnnnnnnnnnnnn format
    /// </summary>
    public string MachineBBAN;

    /// <summary>
    /// Parses and converts to IBAN a pre 2012 BBAN in the nnnnnn-nn[nnnnnn] format. Ignores all non-digit characters.
    /// </summary>
    /// <param name="bban">Pre 2012 BBAN in the nnnnnn-nn[nnnnnn] format. Ignores all non-digit characters.</param>
    /// <returns></returns>
    public static FinnishBankAccountNumber FromBBAN(string bban)
    {
        if (bban == null)
            throw new ArgumentNullException("bban");

        FinnishBankAccountNumber account = new FinnishBankAccountNumber();

        string cleanBBAN = rxNotNumber.Replace(bban, "");

        int accountLength = cleanBBAN.Length;
        bool valid = false;
        
        /*
         * BBAN account can be at the smallest
         * 123456-12 and at most 123456-12345678
         * So without extra stuff 8 to 14 chars
         */

        if (accountLength >= 8 && accountLength <= 14)
        {
            FinnishBankInfo bank = FinnishBankInfoFromBBAN(cleanBBAN);
            if (bank != null)
            {
                if (accountLength < 14)
                {
                    cleanBBAN = cleanBBAN.Substring(0, bank.BBANOffset) + "".PadRight(14 - accountLength, '0') + cleanBBAN.Substring(bank.BBANOffset);
                }
                if (valid = IsValidMachineBBAN(cleanBBAN))
                {
                    account.MachineBBAN = cleanBBAN;
                    account.Bank = bank;
                    account.BBAN = MachineBBANtoHumanBBAN(cleanBBAN, bank);
                }
            }
            else
                throw new Exception("No matching bank found.");
        }
        else
            throw new ArgumentException("Not a valid BBAN account.");
        

        if (valid)
        {
            int intIBANchecksum = 98 - BankAccountNumber.ChecksumISO7064(cleanBBAN + "151800");
            string strChecksum = intIBANchecksum.ToString().PadLeft(2, '0').Substring(0, 2);
            account.IBAN = "FI" + strChecksum + cleanBBAN;

            return account;
        }
        else
            throw new ArgumentException("Not a valid BBAN account.");

    }

    /// <summary>
    /// Creates the object from a finnish IBAN account. Ignores whitespace.
    /// </summary>
    /// <param name="iban">A finnish IBAN account. Whitespace ignored.</param>
    public static FinnishBankAccountNumber FromIBAN(string iban)
    {
        if (iban == null)
            throw new ArgumentNullException("iban");

        string cleanIBAN = Regex.Replace(iban, "\\s", "");

        if (!cleanIBAN.StartsWith("FI"))
            throw new ArgumentException("Not a finnish IBAN, country code is '" + cleanIBAN.Substring(0, 2) + "'");

        FinnishBankAccountNumber account = new FinnishBankAccountNumber();

        account.IBAN = cleanIBAN;
        account.MachineBBAN = cleanIBAN.Substring(4);
        account.Bank = FinnishBankInfoFromBBAN(account.MachineBBAN);
        account.BBAN = MachineBBANtoHumanBBAN(account.MachineBBAN, (FinnishBankInfo) account.Bank);

        return account;
    }

    /// <summary>
    /// Incrementally searches bank dictionary to find a matching finnish bank.
    /// </summary>
    public static FinnishBankInfo FinnishBankInfoFromBBAN(string BBAN)
    {
        if (BBAN == null)
            throw new ArgumentNullException("BBAN");

        FinnishBankInfo bank = null;
        if (!FinnishBankDictionary.TryGetValue(int.Parse(BBAN.Substring(0, 1)), out bank))
            if(!FinnishBankDictionary.TryGetValue(int.Parse(BBAN.Substring(0, 2)), out bank))
                FinnishBankDictionary.TryGetValue(int.Parse(BBAN.Substring(0, 3)), out bank);

        if (bank == null)
            throw new Exception("Could not find a corresponding bank.");

        return bank;
    }

    /// <summary>
    /// Converts a 14 character machine BBAN to the old \d{6}-\d{2,8} format.
    /// </summary>
    public static string MachineBBANtoHumanBBAN(string mbban, FinnishBankInfo bank)
    {
        if (mbban == null)
            throw new ArgumentNullException("mbban");

        if (bank == null)
            throw new ArgumentNullException("bank");

        return mbban.Substring(0, bank.BBANOffset) + "-" + mbban.Substring(bank.BBANOffset).TrimStart('0');
    }

    /// <summary>
    /// Luhn algorithm validation. See http://en.wikipedia.org/wiki/Luhn_algorithm
    /// (used only by pre 2012 finnish bank accounts)
    /// </summary>
    public static bool IsValidMachineBBAN(string BBAN)
    {
        if (BBAN == null)
            throw new ArgumentNullException("BBAN");

        if (BBAN.Length < 14)
            return false;

        int checksum = 0;
        for (int i = 0; i < 14; i++)
        {
            int a = int.Parse(BBAN.Substring(i, 1));
            if ((i & 1) == 0)
            {
                a *= 2;
                if (a > 9) a -= 9;
            }
            checksum += a;
        }
        return ((checksum % 10) == 0);
    }

    //Source: http://www.fkl.fi/teemasivut/sepa/tekninen_dokumentaatio/Dokumentit/Suomalaiset_rahalaitostunnukset_ja_BIC-koodit.pdf
    public static Dictionary<int, FinnishBankInfo> FinnishBankDictionary = new Dictionary<int, FinnishBankInfo>()
    {
        {1, new FinnishBankInfo("NDEAFIHH", 6, "Nordea Pankki") },
        {2, new FinnishBankInfo("NDEAFIHH", 6, "Nordea Pankki") },
        {31, new FinnishBankInfo("HANDFIHH", 6, "Handelsbanken") },
        {33, new FinnishBankInfo("ESSEFIHX", 6, "Skandinaviska Enskilda Banken") },
        {34, new FinnishBankInfo("DABAFIHX", 6, "Danske Bank") },
        {36, new FinnishBankInfo("TAPIFI22", 6, "Tapiola Pankki") },
        {37, new FinnishBankInfo("DNBAFIHX", 6, "DNB Bank ASA, Finland Branch") },
        {38, new FinnishBankInfo("SWEDFIHH", 6, "Swedbank") },
        {39, new FinnishBankInfo("SBANFIHH", 6, "S-Pankki") },
        {4, new FinnishBankInfo("HELSFIHH", 7, "Aktia Pankki, Säästöpankit (Sp) ja POP") },
        {5, new FinnishBankInfo("OKOYFIHH", 7, "OP-Pohjola (Osuuspankki)") },
        {6, new FinnishBankInfo("AABAFI22", 6, "Ålandsbanken") },
        {711, new FinnishBankInfo("BSUIFIHH", 6, "Calyon") },           // IBAN only
        {713, new FinnishBankInfo("CITIFIHX", 6, "Citibank") },         // IBAN only
        {715, new FinnishBankInfo("ITELFIHH", 6, "Itella Pankki") },    // IBAN only
        {8, new FinnishBankInfo("DABAFIHH", 6, "Sampo Pankki") },
    };
}

public class BankAccountNumber
{
    private static CultureInfo invariantCulture = CultureInfo.InvariantCulture;

    static BankAccountNumber()
    {        
        IBANRegex = new Dictionary<string, Regex>();
        foreach (var item in IBANRegexString)
        {           
            IBANRegex[item.Key] = new Regex(item.Value, RegexOptions.Compiled);
        }
    }

    /// <summary>
    /// Default is true. Set to false if you want to skip regex validation.
    /// </summary>
    public static bool UseRegexToValidate = true;

    public BankInfo Bank;
    public string IBAN;

    /// <summary>
    /// IBAN in print format (for example GB29 NWBK 6016 1331 9268 19).
    /// </summary>
    public string IBANForPrint 
    { 
        get 
        {
            if (IBAN == null)
                return null;

            int origLen = IBAN.Length;
            int spaces = (IBAN.Length / 4);
            string spaced = IBAN;

            for (int i = 1; i <= spaces; i++)
            {
                int index = i * 4 + (i - 1);
                if (index == origLen + i - 1) break;
                spaced = spaced.Insert(index, " ");
            }

            return spaced;
        } 
    }


    /// <summary>
    /// Implementation of ISO/IEC 7064:2003 checksum
    /// </summary>
    public static int ChecksumISO7064(string input)
    {
        if (input == null)
            throw new ArgumentNullException("input");

        if (input.Length == 0)
            throw new ApplicationException("input cannot be empty");

        for (int i = 10; i <= 35; i++)
        {
            char c = (char)(i + 55);
            input = input.Replace(c.ToString(), i.ToString());
        }
        int checksum = int.Parse(input.Substring(0, 1), invariantCulture);
        for (var i = 1; i < input.Length; i++)
        {
            int v = int.Parse(input.Substring(i, 1), invariantCulture);
            checksum *= 10;
            checksum += v;
            checksum %= 97;
        }
        return checksum;
    }


    /// <summary>
    /// Returns true if the provided IBAN is valid. Whitespace ignored. Uses length, checksum and regex to validate.
    /// <para>See UseRegexToValidate to turn off regex validation</para>
    /// </summary>
    /// <param name="IBAN">IBAN to validate. Whitespace ignored.</param>
    /// <returns>True if the provided IBAN is valid</returns>
    public static bool IsValidIBAN(string IBAN)
    {
        if (IBAN == null)
            throw new ArgumentNullException("IBAN");
        try
        {
            string cleanIBAN = Regex.Replace(IBAN, "\\s", "");

            int checksum = ChecksumISO7064(cleanIBAN.Substring(4) + cleanIBAN.Substring(0, 4));
            bool validChecksum = checksum == 1;

            if (!validChecksum) return false;

            string countryCode = cleanIBAN.Substring(0, 2);
            int countryLen;

            if (!IBANLengths.TryGetValue(countryCode, out countryLen))
                throw new Exception("No country with code '" + countryCode + "'");

            bool validLength = cleanIBAN.Length == IBANLengths[cleanIBAN.Substring(0, 2)];

            if (!validLength) return false;

            if (IBANRegex.ContainsKey(countryCode) && UseRegexToValidate)
            {
                bool validRegex = IBANRegex[countryCode].IsMatch(cleanIBAN);
                if (!validRegex) return false;
            }

            return true;
        }
        catch { return false; }
    }

    public readonly static Dictionary<string, int> IBANLengths = new Dictionary<string, int>
    {
        {"AL", 28}, {"AD", 24}, {"AT", 20}, {"AZ", 28}, {"BE", 16}, {"BH", 22}, {"BA", 20}, {"BR", 29}, {"BG", 22}, {"CR", 21}, 
        {"HR", 21}, {"CY", 28}, {"CZ", 24}, {"DK", 18}, {"DO", 28}, {"EE", 20}, {"FO", 18}, {"FI", 18}, {"FR", 27}, {"GE", 22}, 
        {"DE", 22}, {"GI", 23}, {"GR", 27}, {"GL", 18}, {"GT", 28}, {"HU", 28}, {"IS", 26}, {"IE", 22}, {"IL", 23}, {"IT", 27}, 
        {"KZ", 20}, {"KW", 30}, {"LV", 21}, {"LB", 28}, {"LI", 21}, {"LT", 20}, {"LU", 20}, {"MK", 19}, {"MT", 31}, {"MR", 27}, 
        {"MU", 30}, {"MC", 27}, {"MD", 24}, {"ME", 22}, {"NL", 18}, {"NO", 15}, {"PK", 24}, {"PS", 29}, {"PL", 28}, {"PT", 25}, 
        {"RO", 24}, {"SM", 27}, {"SA", 24}, {"RS", 22}, {"SK", 24}, {"SI", 19}, {"ES", 24}, {"SE", 24}, {"CH", 21}, {"TN", 24}, 
        {"TR", 26}, {"AE", 23}, {"GB", 22}, {"VG", 24}, {"QA", 29}
    };

    public readonly static Dictionary<string, Regex> IBANRegex;

    //Source: http://www.swift.com/dsp/resources/documents/IBAN_Registry.pdf
    private static Dictionary<string, string> IBANRegexString = new Dictionary<string, string>
    {
        {"AL", @"AL\d{2}\d{8}[A-Z0-9]{16}"}, {"AD", @"AD\d{2}\d{4}\d{4}[A-Z0-9]{12}"}, {"AT", @"AT\d{2}\d{5}\d{11}"}, 
        {"AZ", @"AZ\d{2}[A-Z]{4}[A-Z0-9]{20}"}, {"BH", @"BH\d{2}[A-Z]{4}[A-Z0-9]{14}"}, {"BE", @"BE\d{2}\d{3}\d{7}\d{2}"}, 
        {"BA", @"BA\d{2}\d{3}\d{3}\d{8}\d{2}"}, {"BR", @"BR\d{2}\d{8}\d{5}\d{10}[A-Z]{1}[A-Z0-9]{1}"}, 
        {"BG", @"BG\d{2}[A-Z]{4}\d{4}\d{2}[A-Z0-9]{8}"}, {"CR", @"CR\d{2}\d{3}\d{14}"}, {"HR", @"HR\d{2}\d{7}\d{10}"}, 
        {"CY", @"CY\d{2}\d{3}\d{5}[A-Z0-9]{16}"}, {"CZ", @"CZ\d{2}\d{4}\d{6}\d{10}"}, {"DK", @"DK\d{2}\d{4}\d{9}\d{1}"}, 
        {"DO", @"DO\d{2}[A-Z0-9]{4}\d{20}"}, {"EE", @"EE\d{2}\d{2}\d{2}\d{11}\d{1}"}, {"FO", @"FO\d{2}\d{4}\d{9}\d{1}"}, 
        {"FI", @"FI\d{2}\d{6}\d{7}\d{1}"}, {"FR", @"FR\d{2}\d{5}\d{5}[A-Z0-9]{11}\d{2}"}, {"GE", @"GE\d{2}[A-Z]{2}\d{16}"}, 
        {"DE", @"DE\d{2}\d{8}\d{10}"}, {"GI", @"GI\d{2}[A-Z]{4}[A-Z0-9]{15}"}, {"GR", @"GR\d{2}\d{3}\d{4}[A-Z0-9]{16}"}, 
        {"GL", @"GL\d{2}\d{4}\d{9}\d{1}"}, {"GT", @"GT\d{2}[A-Z0-9]{4}[A-Z0-9]{20}"}, {"HU", @"HU\d{2}\d{3}\d{4}\d{1}\d{15}\d{1}"}, 
        {"IS", @"IS\d{2}\d{4}\d{2}\d{6}\d{10}"}, {"IE", @"IE\d{2}[A-Z]{4}\d{6}\d{8}"}, {"IL", @"IL\d{2}\d{3}\d{3}\d{13}"}, 
        {"IT", @"IT\d{2}[A-Z]{1}\d{5}\d{5}[A-Z0-9]{12}"}, {"KZ", @"KZ\d{2}\d{3}[A-Z0-9]{13}"}, {"KW", @"KW\d{2}[A-Z]{4}[A-Z0-9]{22}"}, 
        {"LV", @"LV\d{2}[A-Z]{4}[A-Z0-9]{13}"}, {"LB", @"LB\d{2}\d{4}[A-Z0-9]{20}"}, {"LI", @"LI\d{2}\d{5}[A-Z0-9]{12}"}, 
        {"LT", @"LT\d{2}\d{5}\d{11}"}, {"LU", @"LU\d{2}\d{3}[A-Z0-9]{13}"}, {"MK", @"MK\d{2}\d{3}[A-Z0-9]{10}\d{2}"}, 
        {"MT", @"MT\d{2}[A-Z]{4}\d{5}[A-Z0-9]{18}"}, {"MR", @"MR\d{2}\d{5}\d{5}\d{11}\d{2}"}, 
        {"MU", @"MU\d{2}[A-Z]{4}\d{2}\d{2}\d{12}\d{3}[A-Z]{3}"}, {"MD", @"MD\d{2}[A-Z0-9]{20}"}, 
        {"MC", @"MC\d{2}\d{5}\d{5}[A-Z0-9]{11}\d{2}"}, {"ME", @"ME\d{2}\d{3}\d{13}\d{2}"}, {"NL", @"NL\d{2}[A-Z]{4}\d{10}"}, 
        {"NO", @"NO\d{2}\d{4}\d{6}\d{1}"}, {"PK", @"PK\d{2}[A-Z]{4}[A-Z0-9]{16}"}, {"PL", @"PL\d{2}\d{8}\d{16}"}, 
        {"PS", @"PS\d{2}[A-Z]{4}[A-Z0-9]{21}"}, {"PT", @"PT\d{2}\d{4}\d{4}\d{11}\d{2}"}, {"QA", @"QA\d{2}[A-Z]{4}[A-Z0-9]{21}"}, 
        {"RO", @"RO\d{2}[A-Z]{4}[A-Z0-9]{16}"}, {"SM", @"SM\d{2}[A-Z]{1}\d{5}\d{5}[A-Z0-9]{12}"}, {"SA", @"SA\d{2}\d{2}[A-Z0-9]{18}"}, 
        {"RS", @"RS\d{2}\d{3}\d{13}\d{2}"}, {"SK", @"SK\d{2}\d{4}\d{6}\d{10}"}, {"SI", @"SI\d{2}\d{5}\d{8}\d{2}"}, 
        {"ES", @"ES\d{2}\d{4}\d{4}\d{1}\d{1}\d{10}"}, {"SE", @"SE\d{2}\d{3}\d{16}\d{1}"}, {"CH", @"CH\d{2}\d{5}[A-Z0-9]{12}"}, 
        {"TN", @"TN\d{2}\d{2}\d{3}\d{13}\d{2}"}, {"TR", @"TR\d{2}\d{5}[A-Z0-9]{1}[A-Z0-9]{16}"}, {"AE", @"AE\d{2}\d{3}\d{16}"}, 
        {"GB", @"GB\d{2}[A-Z]{4}\d{6}\d{8}"}, {"VG", @"VG\d{2}[A-Z]{4}\d{16}"},
    };

}

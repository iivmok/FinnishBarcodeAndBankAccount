using System;
using System.Collections.Generic;
using System.Text;

public class FinnishBankInfo : BankInfo
{
    public readonly int BBANOffset;
    public FinnishBankInfo(string bic, int bban_offset)
        : base(bic)
    {
        if (bic == null) throw new ArgumentNullException("bic");

        BBANOffset = bban_offset;
    }
    public FinnishBankInfo(string bic, int bban_offset, string text)
        : base(bic, text)
    {
        if (bic == null) throw new ArgumentNullException("bic");
        if (text == null) throw new ArgumentNullException("text");

        BBANOffset = bban_offset;
    }

}

public class BankInfo
{
    private string m_bic = null;

    public readonly string Text;

    /// <summary>
    /// ISO 9362, Business Identifier Code. Also known as SWIFT code.
    /// </summary>
    public string BIC
    {
        get
        {
            return m_bic;
        }
    }
    /// <summary>
    /// Returns true if the bank in question is a primary office of the bank.
    /// </summary>
    public bool IsPrimaryOffice
    {
        get
        {
            return m_bic.Length == 8;
        }
    }
    /// <summary>
    /// Returns a two-letter ISO 3166-1 alpha-2 country code
    /// </summary>
    public string Country
    {
        get
        {
            return m_bic.Substring(4, 2);
        }
    }
    /// <summary>
    /// A four-letter bank code
    /// </summary>
    public string BankCode
    {
        get
        {
            return m_bic.Substring(0, 4);
        }
    }
    /// <summary>
    /// Branch-identifying code. If not present - presented as "XXX"
    /// </summary>
    public string BranchCode
    {
        get
        {
            if (m_bic.Length == 8)
                return "XXX";
            else
                return m_bic.Substring(8, 3);
        }
    }
    public BankInfo(string bic)
    {
        if (bic == null) throw new ArgumentNullException("bic");

        if (bic.Length == 8 || bic.Length == 11)
        {
            m_bic = bic.ToUpperInvariant();
        }
        else
        {
            throw new ArgumentException("Length of BIC is 8 or 11 as per ISO 9362");
        }
    }
    public BankInfo(string bic, string text)
        : this(bic)
    {
        if (bic == null) throw new ArgumentNullException("bic");

        if (text == null) throw new ArgumentNullException("text");

        Text = text;
    }
    public override string ToString()
    {
        return BIC;
    }
}
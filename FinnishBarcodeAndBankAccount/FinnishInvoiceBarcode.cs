using System;

public class FinnishInvoiceBarcode
{
    private string m_euro, m_cent, zero;
    public string Version, IBAN, RefNumString, DateString;
    public decimal Sum;
    public DateTime? Date = null;
    public FinnishBankAccountNumber Account;
    public FinnishReferenceNumber ReferenceNumber;

    /// <summary>
    /// Parses a 54 character invoice barcode, as per http://www.fkl.fi/teemasivut/sepa/tekninen_dokumentaatio/Dokumentit/Pankkiviivakoodi-opas.pdf
    /// </summary>
    /// <param name="barcode">The barcode to parse in plain "486...516" format. Must be 54 digits long.</param>
    public FinnishInvoiceBarcode(string barcode)
    {
        try
        {
            if (barcode == null)
                throw new ArgumentException("Barcode can not be null", "barcode");
            if (barcode.Length != 54)
                throw new ArgumentException("Barcode length should be 54, is " + barcode.Length.ToString(), "barcode");

            Version = barcode.Substring(0, 1);
            IBAN = "FI" + barcode.Substring(1, 16);
            m_euro = barcode.Substring(17, 6);
            m_cent = barcode.Substring(23, 2);
            zero = barcode.Substring(25, 3);
            RefNumString = barcode.Substring(28, 20);
            DateString = barcode.Substring(48, 6);

            Account = FinnishBankAccountNumber.FromIBAN(IBAN);
            ReferenceNumber = new FinnishReferenceNumber(RefNumString);

            Sum = decimal.Parse(m_euro) + (decimal.Parse(m_cent) / 100);

            try
            {
                Date = DateTime.ParseExact(DateString, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid barcode.", ex);
        }
    }
}

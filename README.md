FinnishBarcodeAndBankAccount
============================

A set of C#/.NET APIs for:
 - IBAN validation (checksum + regex)
 - Finnish BBAN validation and conversion to IBAN (also extraction of bank info)
 - Finnish invoice ref number verification
 - Finnish national invoice barcode parsing

Includes unit tests (currently 9/9), including IBAN validation check against 195 account numbers from all IBAN countries.

### Examples
```C#

var valid = BankAccountNumber.IsValidIBAN("FI3756300020065345");

var acc = FinnishBankAccountNumber.FromBBAN("563000-20065345");
var ibanPrint = acc.IBANForPrint; //"FI37 5630 0020 0653 45"
var bankBIC = acc.Bank.BIC; //"OKOYFIHH";

var bc = new FinnishInvoiceBarcode("486500001200088250005333500000000031418730683329150505");
var sum = bc.Sum; //533.35

var validrf = FinnishReferenceNumber.IsValidReferenceNumber("6174354");
```

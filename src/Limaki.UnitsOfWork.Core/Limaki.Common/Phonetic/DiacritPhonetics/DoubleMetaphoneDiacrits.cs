using PhoneticNet;

namespace Limaki.Common.Phonetics
{
    public class DoubleMetaphoneDiacrits : DoubleMetaphone
    {
        public override string GenerateKey(string strInput, int keyLength)
        {
            strInput = Diacrits.FoldToASCII(strInput);
            return base.GenerateKey(strInput, keyLength);
        }
    }
}




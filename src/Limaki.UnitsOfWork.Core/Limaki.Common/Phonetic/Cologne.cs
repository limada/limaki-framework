using PhoneticNet;

namespace Limaki.Common.Phonetics
{
    /// <summary>
    /// Transforms a word into the respective Cologne Phontetics notation
    /// <seealso cref="https://de.wikipedia.org/wiki/K%C3%B6lner_Phonetik"/>
    /// </summary>
    public class Cologne : IPhonetic
    {

        public string PrimaryKey => "";

        public string AlternateKey => "";

        public string GenerateKey(string strInput) => GenerateKey(strInput, -1);

        public virtual string GenerateKey(string strInput, int keyLength)
        {
            return ColognePhoneticsSharp.ColognePhonetics.GetPhonetics(strInput);
        }
    }
}




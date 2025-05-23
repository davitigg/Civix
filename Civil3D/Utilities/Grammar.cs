namespace Civil3D.Utilities
{
    public static class Grammar
    {
        public static string ToGenitive(this string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            if (word.EndsWith("ი")) return word + "ს"; // ბურთი
            if (word.EndsWith("ო")) return word + "ს"; // მეტრო
            if (word.EndsWith("უ")) return word + "ს"; // კუ
            if (word.EndsWith("ა")) return word.Substring(0, word.Length - 1) + "ის"; // დედა 
            if (word.EndsWith("ე")) return word.Substring(0, word.Length - 1) + "ის"; // ხე

            return word + "ს";
        }
    }
}
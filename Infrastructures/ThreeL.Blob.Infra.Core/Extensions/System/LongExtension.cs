namespace ThreeL.Blob.Infra.Core.Extensions.System
{
    public static class LongExtension
    {
        public static string ToSizeText(this long size)
        {
            if (size == 0)
                return "0KB";

            if (size < 1024)
            {
                return $"{size}B";
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return $"{size * 1.0 / 1024:0.00}KB";
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return $"{size * 1.0 / (1024 * 1024):0.00}MB";
            }
            else
            {
                return $"{size * 1.0 / (1024 * 1024 * 1024):0.00}GB";
            }
        }
    }
}

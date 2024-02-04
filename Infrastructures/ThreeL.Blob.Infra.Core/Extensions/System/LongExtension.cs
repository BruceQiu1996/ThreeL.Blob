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
                return $"{size * 1.0 / 1024:0.0}KB";
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return $"{size * 1.0 / (1024 * 1024):0.0}MB";
            }
            else
            {
                return $"{size * 1.0 / (1024 * 1024 * 1024):0.0}GB";
            }
        }

        public static DateTime ToDateTime(this long time)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(time);
        }

        public static long ToLong(this DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}

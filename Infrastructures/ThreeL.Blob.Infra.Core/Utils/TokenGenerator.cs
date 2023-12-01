using System.Text;

namespace ThreeL.Blob.Infra.Core.Utils
{
    public class TokenGenerator
    {
        //设置生成随机数的随机值字符
        static char[] Constant ={
            '0','1','_','2','3','4','_','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','i','_','j','k','l','m','n','o','_','p','q','r','s','t','u','v','w','x','y','z','_',
            'A','B','C','D','E','F','G','H','I','_','J','K','L','M','N','O','_','P','Q','R','S','T','U','V','W','X','Y','Z','_',
            };

        public static string GenerateToken(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < 24; i++)
            {
                sb.Append(Constant[rd.Next(Constant.Length)]);
            }

            return sb.ToString();
        }
    }
}

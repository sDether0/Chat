using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql;

namespace Server
{
    static class MessageHistory
    {
        public static void SaveMessage(string Name1, string Name2, string text)
        {
            string command = "select text from history where (name1=@p1 and name2=@p2) or (name1=@p2 and name2=@p1)";
            var dt = Mysql.fill(command, new[] { Name1, Name2 });
            if (dt.Rows.Count > 0)
            {
                var t = dt.Rows[0][0].ToString() + "\r\n" + text;
                string com = "update history set text = @p1 where (name1=@p2 and name2=@p3) or (name1=@p3 and name2=@p2)";
                Mysql.com(com,new[] {t,Name1,Name2});
            }
            else
            {
                string com = "insert into history (name1, name2, text) values (@p1, @p2, @p3)";
                Mysql.com(com, new[] { Name1, Name2, text });
            }
        }

        public static string[,] GetMessageHistory(string Name1)
        {
            string command = "select text, name1, name2 from history where name1=@p1 or name2=@p1";
            var dt = Mysql.fill(command,new[] { Name1 });
            var len = dt.Rows.Count;
            string[,] result = new string[len,2];
            for (int i=0;i<dt.Rows.Count;i++)
            {
                result[i, 0] = dt.Rows[i][0].ToString();
                if (dt.Rows[i][1].ToString() != Name1)
                {
                    result[i, 1] = dt.Rows[i][1].ToString();
                }
                else
                {
                    result[i, 1] = dt.Rows[i][2].ToString();
                }
            }
            return result;
        }
    }
}

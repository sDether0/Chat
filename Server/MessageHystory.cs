using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonClasses;
using MySql;

namespace Server
{
    static class MessageHistory
    {
        public static void SaveMessage(string fromuser, string touser, string text)
        {
            string com = "insert into history (fromuser, touser, time, text) values (@p1, @p2, @p3, @p4)";
            var time = DateTime.UtcNow;
            Mysql.com(com, new object[] { fromuser, touser, time, text });
        }

        public static List<HistoryMessage> GetMessageHistory(string fromuser)
        {
            string command = "select time, text, fromuser, touser from history where fromuser=@p1 or touser=@p1";
            var dt = Mysql.fill(command, new[] { fromuser });
            var len = dt.Rows.Count;
            List<HistoryMessage> list = new List<HistoryMessage>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][2].ToString() == fromuser)
                {
                    list.Add(new HistoryMessage() { time = Convert.ToDateTime(dt.Rows[i][0].ToString()), text = dt.Rows[i][1].ToString(), login = dt.Rows[i][3].ToString(), self = true });
                }
                else
                {
                    list.Add(new HistoryMessage() { time = Convert.ToDateTime(dt.Rows[i][0].ToString()), text = dt.Rows[i][1].ToString(), login = dt.Rows[i][2].ToString(), self = false });
                }
            }
            return list;
        }
    }
}

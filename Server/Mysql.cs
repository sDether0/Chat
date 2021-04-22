///////////////////////////////
///////Copyright Claim/////////
////
//    
//    
//    
//    
//    
//    
//    
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace Server
{
    public static class Mysql
    {
        private static string pass = "1uniCorn1_";
        static MySqlConnection con = new MySqlConnection("Server=localhost;Database=ftp_chat;User=sDether;Password=1namQfeg1_;");
        static MySqlConnection conas = new MySqlConnection("Server=localhost;Database=ftp_chat;User=sDether;Password=1namQfeg1_;");
        public static void com(string cm, object[] parm)
        {
            if ((con.State & ConnectionState.Open) != 0)
            {
                con.OpenAsync();
                MySqlCommand com = new MySqlCommand(cm, con);
                int i = 1;
                foreach (object s in parm)
                {
                    com.Parameters.AddWithValue("@p" + i, s);
                    i++;
                }

                com.ExecuteNonQueryAsync();
                con.CloseAsync();
            }
            else
            {
                con.Open();
                MySqlCommand com = new MySqlCommand(cm, con);
                int i = 1;
                foreach (object s in parm)
                {
                    com.Parameters.AddWithValue("@p" + i, s);
                    i++;
                }

                com.ExecuteNonQuery();
                con.Close();
            }
        }

        public static DataTable fill(string cm, object[] parm)
        {
            DataTable dtrl = new DataTable();
            MySqlCommand com = new MySqlCommand(cm, con);
            int i = 1;
            foreach (object s in parm)
            {
                com.Parameters.AddWithValue("@p" + i, s);
                i++;
            }
            con.Open();
            MySqlDataAdapter ad = new MySqlDataAdapter();
            ad.SelectCommand = com;
            ad.Fill(dtrl);
            con.Close();
            return dtrl;
        }

        public static DataTable fill(string cm)
        {
            DataTable dtrl = new DataTable();
            MySqlCommand com = new MySqlCommand(cm, con);
            con.Open();
            MySqlDataAdapter ad = new MySqlDataAdapter();
            ad.SelectCommand = com;
            ad.Fill(dtrl);
            con.Close();

            return dtrl;
        }

        public static DataTable fillas(string cm)
        {

            DataTable dtrl = new DataTable();
            MySqlCommand com = new MySqlCommand(cm, conas);
            conas.OpenAsync();
            MySqlDataAdapter ad = new MySqlDataAdapter();
            ad.SelectCommand = com;
            ad.FillAsync(dtrl);
            conas.CloseAsync();
            return dtrl;
        }

        public static void com(MySqlConnection con2, string cm, object[] parm)
        {
            MySqlCommand com = new MySqlCommand(cm, con2);
            int i = 1;
            foreach (object s in parm)
            {
                com.Parameters.AddWithValue("@p" + i, s);
                i++;
            }
            com.ExecuteNonQuery();
        }

        public static DataTable fill(MySqlConnection con2, string cm)
        {
            DataTable dtrl = new DataTable();
            MySqlCommand com = new MySqlCommand(cm, con2);
            MySqlDataAdapter ad = new MySqlDataAdapter();
            ad.SelectCommand = com;
            ad.Fill(dtrl);
            return dtrl;
        }

        public static DataTable fill(MySqlConnection con2, string cm, object[] parm)
        {
            DataTable dtrl = new DataTable();
            MySqlCommand com = new MySqlCommand(cm, con2);
            int i = 1;
            foreach (object s in parm)
            {
                com.Parameters.AddWithValue("@p" + i, s);
                i++;
            }
            MySqlDataAdapter ad = new MySqlDataAdapter();
            ad.SelectCommand = com;
            ad.Fill(dtrl);
            return dtrl;
        }
    }
}

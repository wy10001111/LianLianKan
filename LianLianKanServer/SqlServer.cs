using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using LianLianKanLib;
using System.Collections.ObjectModel;
using System.IO;

namespace LianLianKanServer
{
    public class SqlServer
    {
        MySqlConnection _mySqlConnection;

        public bool OpenDatabase(string user, string password)
        {
            try
            {
                string mysqlConf = System.Configuration.ConfigurationManager.ConnectionStrings["mysqlConf"].ConnectionString;
                string connectStr = mysqlConf + $"user={user};password={password}";
                _mySqlConnection = new MySqlConnection(connectStr);
                _mySqlConnection.Open();
                if (_mySqlConnection.State != ConnectionState.Open)
                    throw new Exception("连接数据库失败！");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }
        public void CloseDatabase()
        {
            _mySqlConnection?.Close();
            _mySqlConnection?.Dispose();
            _mySqlConnection = null;
        }

        public void Execute(string sqlStr, Action<MySqlDataReader> respondsFunc)
        {
            try
            {
                using (MySqlCommand command = new MySqlCommand(sqlStr, _mySqlConnection))
                using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
                    respondsFunc?.Invoke(reader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void InitUserList(ObservableCollection<UIUser> userList)
        {
            this.Execute("SELECT * FROM lianliankan_user ORDER BY id ASC", async (reader) => {
                while (await reader.ReadAsync())
                {
                    var user = new UIUser((int)(reader["id"]),
                        reader["user_account"].ToString(),
                        reader["user_password"].ToString(),
                        reader["user_name"].ToString());
                    user.Introduce = reader["user_introduce"].ToString();
                    user.HeadImagePath = reader["user_head_path"].ToString();
                    userList.Add(user);
                }
            });
        }

        public void GetGameRecords(UIUser user)
        {
            user.GameRecords.Clear();
            this.Execute($"SELECT * FROM lianliankan_game_record WHERE id = '{user.ID}' ORDER BY play_time ASC", async (reader) => {
                while (await reader.ReadAsync())
                {
                    DateTime playTime = (DateTime)reader["play_time"];
                    string gameTime = reader["game_time"] as string;
                    user.AddGameRecord(playTime, gameTime);
                }
            });
        }

        public void InsertGameRecord(UIUser user, GameRecord record)
        {
            this.Execute($"CALL insert_into_lianliankan_game_record('{user.ID}', '{record.PlayTime}', '{record.SpentTime}')", null);
        }

        public UIUser InsertUser(UIUser user)
        {
            UIUser rtUser = null;
            this.Execute("INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce, user_head_path) "
                + $"VALUES('{user.Account}', '{user.Password}', '{user.Name}', '{user.Introduce}', '{user.HeadImageName}')", null);
            this.Execute($"SELECT * FROM lianliankan_user WHERE user_account = '{user.Account}'", async (reader) => {
                while (await reader.ReadAsync())
                {
                    rtUser = new UIUser((int)(reader["id"]),
                        reader["user_account"].ToString(),
                        reader["user_password"].ToString(),
                        reader["user_name"].ToString());
                    rtUser.Introduce = reader["user_introduce"].ToString();
                    rtUser.HeadImagePath = reader["user_head_path"].ToString();
                }
            });
            return rtUser;
        }


    }
}

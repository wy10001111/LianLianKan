using LianLianKanLib;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LianLianKanServer
{
    public class SqlServer
    {
        #region 属性与变脸

        /// <summary>
        /// 
        /// </summary>
        MySqlConnection _mySqlConnection;

        #endregion

        #region 方法

        /// <summary>
        /// 打开数据库
        /// </summary>
        public bool OpenDatabase(string user, string password)
        {
            try
            {
                string mysqlConf = "server=localhost;port=3306;database=mydatabase;";
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

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void CloseDatabase()
        {
            _mySqlConnection?.Close();
            _mySqlConnection?.Dispose();
            _mySqlConnection = null;
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="respondsFunc"></param>
        private void Execute(string sqlStr, Action<MySqlDataReader> respondsFunc)
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

        /// <summary>
        /// 初始化用户列表
        /// </summary>
        /// <param name="userList"></param>
        public void InitUserList(ObservableCollection<User> userList)
        {
            this.Execute("SELECT * FROM lianliankan_user ORDER BY id ASC", async (reader) => {
                while (await reader.ReadAsync())
                {
                    var user = new User((int)(reader["id"]),
                        reader["user_account"].ToString(),
                        reader["user_password"].ToString(),
                        reader["user_name"].ToString());
                    user.Introduce = reader["user_introduce"].ToString();
                    var directory = Path.Combine(Environment.CurrentDirectory, $@"HeadImages");
                    Directory.CreateDirectory(directory);
                    var filePath = Path.Combine(directory, reader["user_head_name"].ToString());
                    user.ChangeHead(filePath);
                    userList.Add(user);
                }
            });
        }

        /// <summary>
        /// 查询游戏记录
        /// </summary>
        public void GetGameRecords(User user)
        {
            user.GameRecords.Clear();
            this.Execute($"SELECT * FROM lianliankan_game_record WHERE user_id = '{user.ID}' ORDER BY play_time ASC", async (reader) => {
                while (await reader.ReadAsync())
                {
                    DateTime playTime = (DateTime)reader["play_time"];
                    string elapsedTime = reader["elapsed_time"] as string;
                    user.AddGameRecord(playTime, elapsedTime);
                }
            });
        }

        /// <summary>
        /// 插入游戏记录
        /// </summary>
        public void InsertGameRecord(User user, GameRecord record)
        {
            this.Execute($"CALL insert_into_lianliankan_game_record('{user.ID}', '{record.PlayTime}', '{record.SpentTime}')", null);
        }

        /// <summary>
        /// 插入用户
        /// </summary>
        public User InsertUser(string account, string password, string name, string introduce, string headImagePath)
        {
            User rtUser = null;
            this.Execute("INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce, user_head_name) "
                + $"VALUES('{account}', '{password}', '{name}', '{introduce}', '{System.IO.Path.GetFileName(headImagePath)}')", null);
            this.Execute($"SELECT * FROM lianliankan_user WHERE user_account = '{account}'", async (reader) => {
                while (await reader.ReadAsync())
                {
                    rtUser = new User((int)(reader["id"]),
                        reader["user_account"].ToString(),
                        reader["user_password"].ToString(),
                        reader["user_name"].ToString());
                    rtUser.Introduce = reader["user_introduce"].ToString();
                    var directory = Path.Combine(Environment.CurrentDirectory, $@"HeadImages");
                    Directory.CreateDirectory(directory);
                    var filePath = Path.Combine(directory, reader["user_head_name"].ToString());
                    rtUser.ChangeHead(filePath);
                }
            });
            return rtUser;
        }

        #endregion

    }
}

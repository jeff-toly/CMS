using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;

namespace PerformanceMerit.DAL
{
    public static class DBExtension
    {  
        /// <summary>
        /// 调用Function/Procedure时添加傳入參數.
        /// <param name="type">参数类型</param>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="dataBase"></param>
        public static void AddInParameter(this Database dataBase, DbCommand command, string parameterName, OracleType type, int size, object value)
        {
            try
            {
                OracleParameter oraclePar = new OracleParameter();
                oraclePar.ParameterName = parameterName;
                oraclePar.OracleType = type;
                oraclePar.Value = value;
                oraclePar.Size = size;
                command.Parameters.Add(oraclePar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 调用Function/Procedure时添加返回值.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type">参数类型</param>
        public static void AddReturnValue(this Database dataBase, DbCommand command, OracleType type)
        {
            try
            {
                OracleParameter oraclePar = new OracleParameter();
                oraclePar.OracleType = type;
                oraclePar.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(oraclePar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 调用Function/Procedure时添加返回值.
        /// </summary>
        /// <param name="command">數據庫處理命令</param>
        /// <param name="parameterName">參數名</param>
        /// <param name="type">参数类型</param>
        /// <param name="size">長度 or 字符數</param>

        public static void AddReturnValue(this Database dataBase, DbCommand command, string parameterName, OracleType type, int size)
        {
            try
            {
                OracleParameter parameter = new OracleParameter();
                parameter.OracleType = type;
                parameter.Direction = ParameterDirection.ReturnValue;
                parameter.Size = size;
                parameter.ParameterName = parameterName;
                command.Parameters.Add(parameter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 调用Function/Procedure时添加返回值.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type">参数类型</param>
        public static void AddReturnValue(this Database dataBase, DbCommand command, string name, OracleType type)
        {
            try
            {
                OracleParameter oraclePar = new OracleParameter();
                oraclePar.OracleType = type;
                oraclePar.ParameterName = name;
                oraclePar.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(oraclePar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 调用Function/Procedure时添加输出参数(一般输出参数为Cursor时使用).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name">参数名称</param>
        /// <param name="type">参数类型(一般为Cursor)</param>
        public static void AddOutParameter(this Database dataBase, DbCommand command, string name, OracleType type)
        {
            try
            {
                OracleParameter oraclePar = new OracleParameter();
                oraclePar.OracleType = type;
                oraclePar.ParameterName = name;
                oraclePar.Direction = ParameterDirection.Output;
                command.Parameters.Add(oraclePar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 调用Function/Procedure时添加輸入输出参数.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name">参数名称</param>
        /// <param name="type">参数类型</param>
        public static void AddInOutParameter(this Database dataBase, DbCommand command, string name, OracleType type, object value, int size)
        {
            try
            {
                OracleParameter oraclePar = new OracleParameter();
                oraclePar.OracleType = type;
                oraclePar.ParameterName = name;
                oraclePar.Value = value;
                oraclePar.Direction = ParameterDirection.InputOutput;
                oraclePar.Size = size;
                command.Parameters.Add(oraclePar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 批量執行DbCommand(帶事務).
        /// </summary>
        /// <param name="db"></param>
        /// <param name="listCommand">DbCommand集合</param>
        /// <returns>執行結果(大於0表示成功,否則失敗)</returns>
        public static int ExecuteNonQuery(this Database db, List<DbCommand> listCommand)
        {
            int returnValue = 0;
            DbConnection conn = db.CreateConnection();
            conn.Open();
            using (DbTransaction tran = conn.BeginTransaction())
            {
                try
                {
                    foreach (DbCommand command in listCommand)
                    {
                        command.Connection = conn;
                        db.ExecuteNonQuery(command, tran);
                        int result = 0;
                        int.TryParse(command.Parameters[command.Parameters.Count - 1].Value.ToString(), out result);
                        returnValue += result;
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    returnValue = -1;
                    tran.Rollback();
                    throw ex;
                }
            }
			conn.Close();
			return returnValue;
        }

        /// <summary>
        /// 事務執行DbCommand.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="listCommand">DbCommand集合</param>
        /// <returns>執行結果(大於0表示成功,否則失敗)</returns>
        public static int ExecuteNonQueryTran(this Database db, List<DbCommand> listCommand)
        {
            int result = 0;
            DbConnection conn = db.CreateConnection();
            conn.Open();
            using (DbTransaction tran = conn.BeginTransaction())
            {
                try
                {
                    foreach (DbCommand command in listCommand)
                    {
                        command.Connection = conn;
                        result = db.ExecuteNonQuery(command, tran);
                        if (result == 0)
                            break;
                    }
                    if (result > 0)
                        tran.Commit();
                    else
                        tran.Rollback();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
			conn.Close();
            return result;
        }

		/// <summary>
		/// 批量執行DbCommand(帶事務).
		/// </summary>
		/// <param name="listCommand">DbCommand集合</param>
		/// <returns>執行結果(大於0表示成功,否則失敗)</returns>
		public static bool ReWriteExecuteNonQuery(this Database db, List<DbCommand> listCommand, out List<string> msglist)
		{
			bool returnValue = true;
			DbConnection conn = db.CreateConnection();
			conn.Open();
			using (DbTransaction tran = conn.BeginTransaction())
			{
				try
				{
					msglist = new List<string>();
					foreach (DbCommand command in listCommand)
					{
						command.Connection = conn;
						db.ExecuteNonQuery(command, tran);
						int res = int.Parse(command.Parameters["P_RESULT"].Value.ToString().Trim());
						string msg = command.Parameters["P_MSG"].Value.ToString();
						if (res == 0)
						{
							if (!string.IsNullOrEmpty(msg))
								msglist.Add(msg);
							returnValue = false;
						}
					}
					tran.Commit();
				}
				catch (Exception ex)
				{
					returnValue = false;
					tran.Rollback();
					throw ex;
				}
			}
			conn.Close();
			return returnValue;
		}
    }
}

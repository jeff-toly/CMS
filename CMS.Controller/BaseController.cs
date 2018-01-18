using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CMS.ControllerCollection
{
    public class BaseController : Controller
    {
        protected string DataTableToJson2(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return "[]";
            }

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            var result = jsonBuilder.ToString();
            result = result.Replace("\r", "\\r").Replace("\n", "\\n");
            return result;
        }
        
        protected string DataTableToJson(DataTable dt)  //比ToJson多加了TableName
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"");
            jsonBuilder.Append(dt.TableName);
            jsonBuilder.Append("\",");
            jsonBuilder.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");                
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        protected string ToJson(DataTable dt)
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            json.MaxJsonLength = Int32.MaxValue;
            ArrayList aList = new ArrayList();
            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (DataColumn dc in dt.Columns)
                {
                    dict.Add(dc.ColumnName, dr[dc.ColumnName].ToString());                    
                }
                aList.Add(dict);
            }
            return json.Serialize(aList);
        }

        public DataTable ToDataTable(string json)
        {
            DataTable dt = new DataTable();
            DataTable result;
            JavaScriptSerializer jsonS = new JavaScriptSerializer();
            jsonS.MaxJsonLength = Int32.MaxValue;
            ArrayList aList = jsonS.Deserialize<ArrayList>(json);
            if (aList.Count > 0)
            {
                foreach (Dictionary<string, object> dict in aList)
                {
                    if (dict.Keys.Count == 0)
                    {
                        result = dt;
                        return result;
                    }
                    if (dt.Columns.Count == 0)
                    {
                        foreach (string current in dict.Keys)
                        {
                            dt.Columns.Add(current, dict[current].GetType());
                        }
                    }
                    DataRow dr = dt.NewRow();
                    foreach (string current in dict.Keys)
                    {
                        dr[current] = dict[current];
                    }
                    dt.Rows.Add(dr);
                }
            }
            result = dt;
            return result;
        }

        /// <summary>
        /// 对前端主动通过encodeURIComponent加密的字符串进行解密
        /// </summary>
        /// <param name="encodeString"></param>
        /// <returns></returns>
        protected string UrlDecode(string encodeString)
        {
            // 因MVC框架又进行了一次编码，所以需要连续解码2次
            string decodeStr = System.Web.HttpUtility.UrlDecode(encodeString);
            decodeStr = System.Web.HttpUtility.UrlDecode(decodeStr);
            return decodeStr;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.UserLanguages != null)
            {
                string cultureName = Request.UserLanguages[0];
                if (!string.IsNullOrEmpty(cultureName))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureName);
                }
            }
            base.OnActionExecuting(filterContext);
        }


        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public string RandomWithTicks()
        {
            var result = "";
            int ticks = (int)DateTime.Now.Ticks;
            var rnd = new Random(ticks);
            var no = rnd.Next(0, 999999);
            result = no.ToString().PadLeft(6, '0');
            return result;
        }

        public string FileSendToService(HttpPostedFileBase file, string fileName)
        {
            Socket s = GetSocketOfServer();
            string result = "";
            if (s == null)
            {
                result = "连接创建失败";
            }
            else
            {
                List<Byte> contentList = new List<Byte>();
                //文件内容
                Byte[] fileContent = System.IO.File.ReadAllBytes(file.FileName);
                contentList.AddRange(fileContent);
                //文件校验
                Byte[] checkContent = new Byte[8] { 1, 2, 3, 4, 5, 6, 7, 8, };
                contentList.AddRange(checkContent);
                //文件名
                Byte[] _fileName = Encoding.ASCII.GetBytes(fileName);
                contentList.AddRange(_fileName);
                //文件名校验
                Byte[] checkName = new Byte[8] { 1, 3, 5, 7, 2, 4, 6, 8, };
                contentList.AddRange(checkName);
                //向服务器发送信息
                s.Send(contentList.ToArray(), contentList.Count, SocketFlags.None);
                Thread.Sleep(1000);//暂停1s防止发送过快
                result = "文件成功发送至服务器";
            }
            return result;
        }

        public static Socket GetSocketOfServer()
        {
            int port = 5050;
            IPAddress iPAddress = IPAddress.Parse("192.168.1.105");
            Socket s = null;
            IPEndPoint ipe = new IPEndPoint(iPAddress, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tempSocket.Connect(ipe);
            s = tempSocket;
            return s;
        }
    }
}

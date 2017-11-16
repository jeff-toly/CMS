﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CMS.ControllerCollection
{
    public class BaseController : Controller
    {
        protected string DataTableToJson(DataTable dt)
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
    }
}
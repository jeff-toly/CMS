using CMS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CMS.ControllerCollection.Company
{
    public class CompanyController : BaseController
    {
        public ActionResult Index()
        {
            List<CompanyEntity> companyList = GetCompanyList();

            return View(companyList);
        }

        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <returns></returns>
        private static List<CompanyEntity> GetCompanyList()
        {
            List<CompanyEntity> companyList = new List<CompanyEntity>()
            {
                new CompanyEntity(){ ApproveRoute = "1" },
                new CompanyEntity(){ ApproveRoute = "2" },
                new CompanyEntity(){ ApproveRoute = "3" },
                new CompanyEntity(){ ApproveRoute = "4" },
                new CompanyEntity(){ ApproveRoute = "5" },
            };
            List<CompanyEntity> oldCompanyList = new List<CompanyEntity>()
            {
                new CompanyEntity(){ ApproveRoute = "1", CompanyID = Guid.NewGuid().ToString(), CompanyName = "平安银行", },
                new CompanyEntity(){ ApproveRoute = "3", CompanyID = Guid.NewGuid().ToString(), CompanyName = "工商银行", },
                new CompanyEntity(){ ApproveRoute = "5", CompanyID = Guid.NewGuid().ToString(), CompanyName = "太平保险", },
            };
            UpdateCompanyList(companyList, oldCompanyList);

            return companyList;
        }

        private static void UpdateCompanyList(List<CompanyEntity> companyList, List<CompanyEntity> oldCompanyList)
        {
            if (!companyList.Exists(t => t.ApproveRoute == "5"))
            {
                companyList.Add(new CompanyEntity() { ApproveRoute = "5", CompanyID = Guid.NewGuid().ToString(), });
            }
            else
            {
                companyList.FindLast(t => t.ApproveRoute == "5").CompanyID = oldCompanyList.FindLast(t => t.ApproveRoute == "5").CompanyID;
                companyList.FindLast(t => t.ApproveRoute == "5").CompanyName = oldCompanyList.FindLast(t => t.ApproveRoute == "5").CompanyName;
            }
        }

        public ActionResult JqueryFormIE8()
        {
            CompanyEntity company = new CompanyEntity();
            return View(company);
        }

        [HttpPost]
        public ActionResult SaveCompany(CompanyEntity company, bool validate)
        {
            if (!validate)
                return Json(new { success = false, msg = "验证失败！" }, "text/plain");
            return Json(new { success = true, msg = "保存成功！" }, "text/plain");
        }
    }
}
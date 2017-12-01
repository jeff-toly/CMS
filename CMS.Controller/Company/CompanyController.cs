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
            if (!companyList.Exists(t => t.ApproveRoute == "5"))
            {
                companyList.Add(new CompanyEntity() { ApproveRoute = "5", CompanyID = Guid.NewGuid().ToString(), });
            }
            else
            {
                companyList.FindLast(t => t.ApproveRoute == "5").CompanyID = oldCompanyList.FindLast(t => t.ApproveRoute == "5").CompanyID;
                companyList.FindLast(t => t.ApproveRoute == "5").CompanyName = oldCompanyList.FindLast(t => t.ApproveRoute == "5").CompanyName;
            }

            return View(companyList);
        }
    }
}
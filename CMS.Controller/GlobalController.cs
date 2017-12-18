using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Webdiyer.WebControls.Mvc;
using System.IO;
using TalentPool.BLL;
using TalentPool.Models;
using TalentPool.Utilities;

namespace TalentPool.web.Controllers
{
	[HandleError]
	public class GlobalController : Controller
	{
		private BLPermission g_RolePermission = new BLPermission();
		private BLAttactFile g_AttactFile = new BLAttactFile();
		private EnumInTalentPool enumInTalentPool = new EnumInTalentPool();
		private BLCountry m_Country = new BLCountry();

		#region public property

		/// <summary>
		/// 是否有效下拉框值.
		/// </summary>
		public List<SelectListItem> EnableList
		{
			get
			{
				List<SelectListItem> enableList = new List<SelectListItem>();
				enableList.Add(new SelectListItem() { Value = "1", Text = "是" });
				enableList.Add(new SelectListItem() { Value = "0", Text = "否" });
				return enableList;
			}
		}

		/// <summary>
		/// 性别下拉框值.
		/// </summary>
		public List<SelectListItem> SexList
		{
			get
			{
				List<SelectListItem> enableList = new List<SelectListItem>();
				enableList.Add(new SelectListItem() { Value = "F", Text = "女" });
				enableList.Add(new SelectListItem() { Value = "M", Text = "男" });
				return enableList;
			}
		}

		/// <summary>
		/// 是否有效下拉框值.
		/// </summary>
		public List<SelectListItem> EnableAllList
		{
			get
			{
				List<SelectListItem> enableList = new List<SelectListItem>();
				enableList.Add(new SelectListItem() { Value = "All", Text = "所有" });
				enableList.Add(new SelectListItem() { Value = "1", Text = "是" });
				enableList.Add(new SelectListItem() { Value = "0", Text = "否" });
				return enableList;
			}
		}



		/// <summary>
		/// 登陆用户信息.
		/// </summary>
		public ExtSysUsersVO LoginUser
		{
			get
			{
				//return new ExtSysUsersVO() { USERID = "admin", USERNAME = "管理员" };
				return Session["LoginUser"] as ExtSysUsersVO;
			}
			set
			{

				Session["LoginUser"] = value;
			}
		}

		/// <summary>
		/// 页大小. 
		/// </summary>
		public int PageSize
		{
			get
			{
				int result;
				string str = GetAppSetting("PageSize");
				int.TryParse(str, out result);
				return result;
			}
		}

		/// <summary>
		/// 系统管理员ID 
		/// </summary>
		public int AdminID
		{
			get
			{
				int result;
				string str = GetAppSetting("AdminID");
				int.TryParse(str, out result);
				return result;
			}
		}

		/// <summary>
		/// 账户初始密码
		/// </summary>
		public string InitialPassword
		{
			get
			{
				return GetAppSetting("InitialPassword");
			}
		}

		/// <summary>
		/// 临时文件夹.
		/// </summary>
		public string TempDirectory
		{
			get
			{
				return GetAppSetting("TempDirectory");
			}
		}

		/// <summary>
		/// 上传文件夹.
		/// </summary>
		public string UploadFile
		{
			get
			{
				return GetAppSetting("UploadFile");
			}
		}

		/// <summary>
		/// 视频文件夹.
		/// </summary>
		public string VideoUploadFile
		{
			get
			{
				return GetAppSetting("VideoUploadFile");
			}
		}

		/// <summary>
		/// 人员照片文件夹.
		/// </summary>
		public string PersonUploadFile
		{
			get
			{
				return GetAppSetting("PersonPhoto");
			}
		}

		/// <summary>
		/// 本页URL.
		/// </summary>
		public string PageURL
		{
			get
			{
				string controller = ControllerContext.RouteData.Values["controller"].ToString();
				string action = ControllerContext.RouteData.Values["action"].ToString().Replace("Detail", "").Replace("AddOrEdit", "").Replace("Add", "");
				string url = "/" + controller.Trim() + "/" + action.Trim() + ".aspx";
				return url;
			}
		}



		#endregion

		#region private method

		/// <summary>
		/// 获取Web.config中的值. 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private string GetAppSetting(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		#endregion

		#region public function
		public List<CampusVO> QueryAllParkByUser()
		{
			try
			{
				return g_RolePermission.QueryGetParkByUserIdUrl(LoginUser.USERID.Trim(), PageURL);
			}
			catch (Exception)
			{

				throw;
			}
		}
		#endregion

		#region public function
		/// <summary>
		/// 保存文件.
		/// </summary>
		/// <param name="Files"></param>
		/// <returns></returns>
		public string SaveFile(System.Web.HttpPostedFileBase Files)
		{
			string dir = "UploadFile";
			AttactFileVO fileInfo = new AttactFileVO();
			string uploadPath = Server.MapPath("~/") + dir;
			if (!Directory.Exists(uploadPath))
				Directory.CreateDirectory(uploadPath);                         //判斷物理路徑是否存在,若不存在則創建此物理路徑.
			string fileActualName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			string extension = Path.GetExtension(Files.FileName);
			string filePath = uploadPath + "\\" + fileActualName + extension;
			if (Files.FileName != "")//上傳文件
			{
				Files.SaveAs(filePath);
				string fileName = Path.GetFileName(Files.FileName);
				fileInfo.FILENAME = fileName;
				fileInfo.FILEPATH = "/" + dir + "/" + fileActualName + extension;//william modified 
				fileInfo.FILEID = Guid.NewGuid().ToString();
				fileInfo.ENTRYUSER = LoginUser.USERID;
				if (g_AttactFile.InsertFile(fileInfo) == 1)
					return fileInfo.FILEID;
				else
					return "";
			}
			else
			{
				return "";
			}

		}
		#endregion

		/// <summary>
		/// 下载文件.
		/// </summary>
		/// <param name="FileID">文件ID</param> 
		public void Download(string FileID)
		{
			AttactFileVO file = g_AttactFile.QueryFilleByID(FileID);
			try
			{
				string filePath = Server.MapPath(file.FILEPATH);
				FileInfo info = new FileInfo(filePath);
				if (info.Exists)//判斷文件是否存在，防止文件不存在導致WEB崩潰
				{
					long fileSize = info.Length;
					HttpContext.Response.Clear();

					//指定Http Mime格式为压缩包
					HttpContext.Response.ContentType = "application/x-zip-compressed";

					// Http 协议中有专门的指令来告知浏览器, 本次响应的是一个需要下载的文件. 格式如下:
					// Content-Disposition: attachment;filename=filename.txt
					HttpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + Server.UrlEncode(info.Name));
					//不指明Content-Length用Flush的话不会显示下载进度   
					HttpContext.Response.AddHeader("Content-Length", fileSize.ToString());
					HttpContext.Response.TransmitFile(filePath, 0, fileSize);
					HttpContext.Response.Flush();
					HttpContext.Response.End();
				}
				else
				{
					HttpContext.Response.Write("<script>alert('文件不存在！'); window.close();</script>");
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// 獲取下拉列表
		/// </summary>
		/// <typeparam name="T">泛型：實體類</typeparam>
		/// <param name="dataList">實體類列表</param>
		/// <param name="columnValue">數值</param>
		/// <param name="columnText">文本</param>
		/// <returns></returns>
		public List<SelectListItem> GetSelectList<T>(List<T> dataList, string columnValue, string columnText) where T : class
		{
			SelectList sList = new SelectList(dataList, columnValue, columnText);
			List<SelectListItem> selectList = sList.ToList();
			return selectList;
		}

		/// <summary>
		/// 獲取所有的下拉列表
		/// </summary>
		public void GetSelectListViewData()
		{
			SelectListItem _item = new SelectListItem { Text = "--請選擇--", Value = "" };
			List<SelectListItem> _list = new List<SelectListItem>();
			_list = enumInTalentPool.EnumToList(typeof(EnumInTalentPool.BloodType));
			_list.Insert(0, _item);
			ViewData["BloodType"] = _list;
			_list = enumInTalentPool.EnumStringToList(typeof(EnumInTalentPool.SexSatus));
			_list.Insert(0, _item);
			ViewData["SexList"] = _list;
			_list = enumInTalentPool.EnumToList(typeof(EnumInTalentPool.MaritalStatus));
			_list.Insert(0, _item);
			ViewData["MaritalStatus"] = _list;
			List<ExtCountryVO> _countrySelect = m_Country.QueryCountrySelect();
			_list = GetSelectList(_countrySelect, "COUNTRYNO", "COUNTRYNAME");
			_list.Insert(0, _item);
			ViewData["Nationality"] = _list;
			_list = enumInTalentPool.EnumToList(typeof(EnumInTalentPool.YesOrNo));
			_list.Insert(0, _item);
			ViewData["YesOrNo"] = _list;
			_list = enumInTalentPool.EnumToList(typeof(EnumInTalentPool.Education));
			_list.Insert(0, _item);
			ViewData["EducationList"] = _list;
			_list = enumInTalentPool.EnumToList(typeof(EnumInTalentPool.JobState));
			_list.Insert(0, _item);
			ViewData["JobStateList"] = _list;
			_list = new List<SelectListItem>();
			int year = DateTime.Now.Year;
			for (int i = year; i > year - 3; i--)
			{
				SelectListItem item = new SelectListItem { Text = i.ToString(), Value = i.ToString() };
				_list.Add(item);
			}
			_list.Insert(0, _item);
			ViewData["YearList"] = _list;
        }
		
		/// <summary>
        /// 權限ID 
        /// </summary>
        public string ReviewRoleID
        {
            get
            {
                return "87C1F918A0B0464EBFB2E792B063C54B";
            }
        }

        /// <summary>
        /// 是否有權限查看所有數據 1/0 是/否
        /// </summary>
        /// <returns>1/0 是/否</returns>
        public int ReviewRole()
        {
            int sysRoleID = 0;//管理員角色數量
            SysManageController sysManage = new SysManageController();
            List<string> roleidList = sysManage.getUserRoleID(LoginUser.USERID);
            string[] ReviewRoleIDlist = ReviewRoleID.Split(',');
            foreach (string id in ReviewRoleIDlist)
            {
                foreach (string roleid in roleidList)
                {
                    if (roleid == id)
                    {
                        sysRoleID = 1; // 管理員角色則+1
                        break;
                    }
                }
                if (sysRoleID == 1) break;
            }
            return sysRoleID;
        }
    }
}

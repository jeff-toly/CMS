using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Model
{
    public class CompanyEntity
    {
        [DisplayName(displayName: "签核路由")]
        public string ApproveRoute { get; set; }

        [DisplayName(displayName: "公司表ID")]
        public string CompanyID { get; set; }

        [DisplayName(displayName: "公司名称")]
        public string CompanyName { get; set; }
    }
}

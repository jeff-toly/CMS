using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DAL
{
    public class DBHelper
    {
        public DbProviderFactory DB = DbProviderFactories.GetFactory("123");
    }
}

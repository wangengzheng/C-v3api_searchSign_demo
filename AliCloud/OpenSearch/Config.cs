using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace AliCloud.OpenSearch
{
    public class Config
    {
        private static string key = ConfigurationManager.AppSettings["AliAccessKey"] ?? "替换为AccessKeyID";
        private static string secret = ConfigurationManager.AppSettings["AliAccessSecret"] ?? "替换为AccessKeySecret";
        private static string name = ConfigurationManager.AppSettings["AliProjectName"] ?? "替换为需访问的应用名";
        private static string url = "http://opensearch-cn-hangzhou.aliyuncs.com";//替换为需访问应用的api地址，例如杭州区域

        /// <summary>
        /// 应用KEY
        /// </summary>
        public static string AliAccessKey
        {
            get { return key; }
            set { key = value; }
        }
        /// <summary>
        /// 应用密钥
        /// </summary>
        public static string AliAccessSecret
        {
            get { return secret; }
            set { secret = value; }
        }
        /// <summary>
        /// 应用项目名称
        /// </summary>
        public static string AliProjectName
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 请求路径
        /// </summary>
        public static string HostUrl
        {
            get { return url; }
            set { url = value; }
        }
    }
}

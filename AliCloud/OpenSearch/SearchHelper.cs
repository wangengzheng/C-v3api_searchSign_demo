using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Security.Cryptography;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Globalization;

namespace AliCloud.OpenSearch
{
    /// <summary>
    /// 阿里云开放式搜索帮助类
    /// </summary>
    public class SearchHelper
    {

        #region GET 查询请求
        /// <summary>
        /// Get请求获取值
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string HttpGet(string Url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = headers.ContainsKey("VERB") ==true ? headers["VERB"]:"GET";
            request.ContentType = headers.ContainsKey("Content-Type") == true ? headers["Content-Type"] : "application/json";
            request.Headers.Add("Content-MD5", headers.ContainsKey("Content-MD5") == true ? headers["Content-MD5"] : "");
            //因request.date只能是date格式，且无法设置为 2017-08-17T07:09:46Z此类格式，只能临时去掉该参数
            //该date值不参与签名，因此header中date也无需设置
            //request.Date = DateTime.Parse(SearchHelper.headers["Date"]).AddHours(-8);


            //循环获取canonicalizedHeaders中以X-Opensearch-开头的值，并进行追加
            //注意在header中“X-Opensearch-”开头内容不可以转换成小写内容，放在签名中时才需转换成小写形式
            foreach (KeyValuePair<string, string> hitem in headers)
            {
                if ((hitem.Value != null && !hitem.Value.Equals("")) && hitem.Key.StartsWith("X-Opensearch-"))
                {
                    request.Headers.Add(hitem.Key, hitem.Value);
                }
            }

            request.Headers.Add("Authorization", headers.ContainsKey("Authorization") == true ? headers["Authorization"] : "");

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        #endregion

        #region URL编码/解码
        /// <summary>
        /// Url编码 解决了C# 和JAVA 之间转换后大小写区分
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }
        public static string PercentEncode(string value)
        {
            return UpperCaseUrlEncode(value)
                .Replace("+", "%20")
                .Replace("*", "%2A")
                .Replace("%7E", "~");
        }
        #endregion

        #region Base64 加密/解密
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string EncodeBase64(Encoding encode, string source)
        {
            string code = "";
            byte[] bytes = encode.GetBytes(source);
            try
            {
                code = Convert.ToBase64String(bytes);
            }
            catch
            {
                code = source;
            }
            return code;
        }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string DecodeBase64(Encoding encode, string result)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
        #endregion

        #region 获取Nonce时间戳值 获取Utc时间戳值，该参数因为格式不符合，暂不参与签名计算
        /// <summary>
        /// 获取Nonce时间戳值 获取Utc时间戳值
        /// </summary>
        /// <returns></returns>
        //public static string utcnow_string = DateTime.UtcNow.ToString();

        public static string GetUtcNow()
        {
            return string.Format("{0}Z", DateTime.Parse(DateTime.UtcNow.ToString()).ToString("s"));
            
        }

        /// <summary>
        /// 生成10时间戳 + 5位随机数，作为X-Opensearch-Nonce值
        /// </summary>
        /// <returns></returns>
        public static string GetSignatureNonce()
        {
            DateTime date = new DateTime(1970, 1, 1, 8, 0, 0);
            string UtcNow = ((int)(DateTime.Now - date).TotalSeconds).ToString();
            return string.Format("{0}{1}", UtcNow, new Random().Next(10000, 99999));
        }
        #endregion

        /// <summary>
        /// 初始化签名和请求头共有header，作为固定参数，同时传入签名得出的 Authorization，最终获取完整headers
        /// </summary>
        /// <returns></returns>

        #region 获取完整Url 和 canonicalized_resource
        /// <summary>
        /// 获取完整Url 和 canonicalized_resource
        /// </summary>
        /// <param name="QueryDictionary"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetUrl_Canonic_res(Dictionary<string, string> QueryDictionary)
        {
            string canonicalized_resource = FromatQueryString(QueryDictionary);//查询参数拼接，已完成

            Dictionary<string, string> Url_Dic = new Dictionary<string, string>();
            Url_Dic.Add("url", string.Format("{0}{1}", Config.HostUrl, canonicalized_resource));
            Url_Dic.Add("Canonic_res", canonicalized_resource);

            return Url_Dic;
        }
        #endregion

        /// <summary>
        /// 规范化的请求字符串（canonicalized_resource）“&”拼接，带参数排序
        /// </summary>
        /// <returns></returns>
        public static string FromatQueryString(Dictionary<string, string> dictionary)
        {

            SortedDictionary<string, object> param = GetSorted(dictionary);
            string result = string.Empty;
            foreach (KeyValuePair<string, object> item in param)
            {
                var value = SearchHelper.PercentEncode(item.Value.ToString());
                result += string.Format("{0}={1}&", item.Key, value);
            }
            result = result.Substring(0, result.Length - 1);
            string path = "/v3/openapi/apps/"+Config.AliProjectName+"/search";

            return HttpUtility.UrlEncode(path).Replace("%2f", "/") + "?" + result;
        }

        /// <summary>
        /// 签名参数拼接，已完成
        /// </summary>
        /// <returns></returns>
        public static string FromatSignParamsString(Dictionary<string, string> headers, string canonicalized_resource)
        {
            object value = "";
            string result = "";

            SortedDictionary<string, object> headersParams = GetSorted(headers);

            if (headersParams.ContainsKey("VERB"))
            {
                headersParams.TryGetValue("VERB", out value);
                result += value.Equals("") ? "\n" : (value + "\n");
            }
            if (headersParams.ContainsKey("Content-MD5"))
            {
                headersParams.TryGetValue("Content-MD5", out value);
                result += value.Equals("")?"\n":(value + "\n");
            }
            if (headersParams.ContainsKey("Content-Type"))
            {
                headersParams.TryGetValue("Content-Type", out value);
                result += value.Equals("") ? "\n" : (value + "\n");
            }
            if (headersParams.ContainsKey("Date"))
            {
                headersParams.TryGetValue("Date", out value);
                result += value.Equals("") ? "\n" : (value + "\n");
            }

            //循环获取canonicalizedHeaders中以X-Opensearch-开头的值，并进行追加
            foreach (KeyValuePair<string, object> item in headersParams)
            {
                if ((item.Value != null && !item.Value.Equals("")) && item.Key.StartsWith("X-Opensearch-"))
                {
                    result += item.Key.ToLower() + ":" + item.Value.ToString() + "\n";
                }
            }
            //添加canonicalized_resource值
            result += canonicalized_resource;

            return result;
        }

        /// <summary>
        /// 转换成Sorted
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static SortedDictionary<string, object> GetSorted(Dictionary<string, string> dictionary)
        {
            SortedDictionary<string, object> param = new SortedDictionary<string, object>();
            foreach (KeyValuePair<string, string> item in dictionary)
            {
                param.Add(item.Key, item.Value);
            }
            return param;
        }

        /// <summary>
        /// 拼接参数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetJoinQueryParamsValue(Dictionary<string, string> value)
        {
            string result = "";
            foreach (KeyValuePair<string, string> item in value)
            {
                result += string.Format("{0}={1}&&", item.Key, item.Value);
            }
            result = result.Substring(0, result.Length - 2);//此处用于去掉最后拼接出来字符串尾部的2个“&&”符号
            return result;
        }

        /// <summary>
        /// 根据canonicalized_resource参数，计算签名，并获取带Authorization完整headers
        /// </summary>
        /// <param name="canonicalized_resource"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaders(string canonicalized_resource, Dictionary<string, string> headers)
        {

            string signature_string = FromatSignParamsString(headers, canonicalized_resource);//传入headers和生成签名字符串

            //签名代码
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.UTF8.GetBytes(Config.AliAccessSecret);
            byte[] dataBuffer = Encoding.UTF8.GetBytes(signature_string);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            string Signature = Convert.ToBase64String(hashBytes);

            //追加Authorization header
            string Authorization = string.Format("OPENSEARCH {0}:{1}", Config.AliAccessKey, Convert.ToBase64String(hashBytes));
            headers.Add("Authorization",Authorization);

            return headers;
        }


    }
}

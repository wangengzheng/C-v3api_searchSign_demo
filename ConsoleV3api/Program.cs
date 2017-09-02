using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AliCloud;
using AliCloud.OpenSearch;

namespace ConsoleV3api
{
    public class Program
    {

        static void Main(string[] args)
        {
            //定义query参数中的各子句
            Dictionary<string, string> ops_query_clause = new Dictionary<string, string>();
            ops_query_clause.Add("query", "name:'文档'");//该query子句是必须要设置
            ops_query_clause.Add("config", "format:fulljson,start:0,hit:20");
            ops_query_clause.Add("sort", "id");

            //定义查询参数字典
            Dictionary<string, string> ops_search_params = new Dictionary<string, string>();
            ops_search_params.Add("query", SearchHelper.GetJoinQueryParamsValue(ops_query_clause));
            ops_search_params.Add("fetch_fields", "name");

            //设置header
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("VERB", "GET");
            headers.Add("Content-MD5", "");
            headers.Add("Content-Type", "application/json");
            headers.Add("X-Opensearch-Nonce", SearchHelper.GetSignatureNonce());
            //因request.date无法设置为 2017-08-17T07:09:46Z 此类格式，临时置空绕过该参数
            //headers.Add("Date",GetUtcNow());
            headers.Add("Date", "");


            //获取完整请求url和canonicalized_resource字符串
            Dictionary<string, string> Url_Canonic_res = SearchHelper.GetUrl_Canonic_res(ops_search_params);
            Console.WriteLine(SearchHelper.HttpGet(Url_Canonic_res["url"], SearchHelper.GetHeaders(Url_Canonic_res["Canonic_res"], headers)));

            Console.ReadLine();
        }
    }
}

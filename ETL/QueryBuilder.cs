using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ETL
{
    class QueryBuilder
    {
        private static XDocument doc = XDocument.Load(Utilities.BaseDirectory() + "queries.xml");

        public static List<DataTable> BuildQueries()
        {
            List<DataTable> queries = new List<DataTable>();
            try
            {
                var filters = getFilters();
                foreach (var filter in filters)
                {
                    foreach (DataRow row in filter.Rows)
                    {
                        for (var i = 0; i < filter.Columns.Count; i++)
                        {
                            Console.WriteLine(row[i]);
                        }
                    }
                }

                //Extracting queries with no filters yet
                foreach (XElement element in doc.XPathSelectElement("//query").Descendants())
                {
                    switch (element.Name.ToString())
                    {
                        case "sql":
                            queries.Add(DBClient.getQueryResultset(element.Value));
                            break;
                    }
                }
                foreach (var query in queries)
                {
                    foreach (DataRow row in query.Rows)
                    {
                        for (var i = 0; i < query.Columns.Count; i++)
                        {
                            Console.WriteLine(row[i]);
                        }
                    }
                }
                return queries;
            }
            catch (Exception ex)
            {
                Utilities.Log("Query Builder, status:" + ex.Message.ToString() + ex.ToString(), "error");
                return queries;
            }
        }

        public static List<DataTable> getFilters()
        {
            List<DataTable> filters = new List<DataTable>();
            //Dictionary<String, String> filters = new Dictionary<String, String>();            
            //Extracting Filters
            foreach (XElement element in doc.XPathSelectElement("//filters").Descendants())
            {                
                filters.Add(DBClient.getQueryResultset(element.Value));                
            }
            return filters;
        }

    }
}
//XML STRUCTURE
//<queriesLibrary>
//	<filters>       
//		<sql name = "seller" > select abbr from sellers</sql>        
//		<sql name = "seller2" > select abbr2 from sellers2</sql>        
//		<sql name = "seller3" > select abbr2 from sellers3</sql>        
//	</filters>
//	<queries>
//        <query name = "items" >
//                < sql > select clientid, clientname, seller from clients</sql>
//                <filter name = "seller" field="3" includeInFile="false" />
//                <path>/ftp/{seller}/clients.csv</path>
//        </query>        
//        <query name = "providers" >
//                < sql > select itemid, description from items where balance &gt; 0 </sql>
//                <filter name = "seller" />
//                < path >/ ftp /{seller}/items.csv</path>
//        </query>        
//	</queries>
//</queriesLibrary>

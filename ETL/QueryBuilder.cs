using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ETL
{
    class QueryBuilder
    {
        public static Dictionary<String, String> ReadXML()
        {
            Dictionary<String, String> filters = new Dictionary<String, String>();
            Dictionary<String, String> queries = new Dictionary<String, String>();
            try
            {                              
            var keyname = "";
            XDocument doc = XDocument.Load(Utilities.BaseDirectory() + "queries.xml");
            //Extracting Filters
            foreach (XElement element in doc.XPathSelectElement("//filters").Descendants())
            {
                filters.Add(element.Attribute("name").Value,element.Value);                
            }

            //Extracting queries with no filters yet
            foreach (XElement element in doc.XPathSelectElement("//queries").Descendants())
            {
                switch(element.Name.ToString())
                {
                    case "query":
                        keyname = element.Attribute("name").Value;
                        break;
                    case "sql":
                        queries.Add(keyname + element.Name, element.Value);
                        break;
                    case "filter":
                        foreach (var pair in filters)
                        {
                            if (element.Attribute("name").Value == pair.Key)
                            {
                                queries.Add(keyname + element.Name, "WHERE " + pair.Key + " IN (" + pair.Value + ')');
                            }
                        }
                        break;
                    case "path":
                        queries.Add(keyname + element.Name, element.Value);
                        break;
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

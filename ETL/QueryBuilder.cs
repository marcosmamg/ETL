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
            //try
            //{                  
            Dictionary<String, String> filters = new Dictionary<String, String>();
            Dictionary<String, String> queries = new Dictionary<String, String>();
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
                if (element.Name.ToString() == "query")
                {
                    keyname = element.Attribute("name").Value;
                }

                if (element.Name.ToString() == "sql")
                {
                    queries.Add(keyname + element.Name, element.Value);
                }
                else if (element.Name.ToString() == "filter")
                {
                    foreach (var pair in filters)
                    {
                        if (element.Attribute("name").Value == pair.Key)
                        {
                            queries.Add(keyname + element.Name, "WHERE " + pair.Key + " IN (" + pair.Value + ')');
                        }
                    }                    
                }
                else if (element.Name.ToString() == "path")
                {
                    queries.Add(keyname + element.Name, element.Value);
                }
            }
            return queries;
            //}
            //catch(Exception ex)
            //{
            //    Utilities.Log(ex.https://www.google.com.ni/url?sa=t&rct=j&q=&esrc=s&source=web&cd=2&ved=0ahUKEwi26KD4gaTXAhWJwiYKHbRqCOcQFggtMAE&url=https%3A%2F%2Fstackoverflow.com%2Fquestions%2F6209841%2Fhow-to-use-xpath-with-xdocument&usg=AOvVaw3f4NpYcV_g9NcBvSUpiCWTMessage.ToString(), "error");
            //    return "";
            //}
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

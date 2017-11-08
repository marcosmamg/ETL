using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ETL
{
    class QueryBuilder
    {
        private static XElement doc = XElement.Load(Utilities.BaseDirectory() + "queries.xml");
        
        public static List<DataTable> GetDataFromSQL()
        {
            List<DataTable> queries = new List<DataTable>();
            try
            {                
                //Extracting queries with no filters yet
                foreach (XElement element in doc.XPathSelectElement("//queries").Descendants())
                {
                    switch (element.Name.ToString())
                    {
                        case "sql":
                            var datatable= DBClient.getQueryResultset(element.Value);
                            datatable.TableName = element.Parent.Attribute("name").Value + ".csv";
                            IEnumerable<XElement> filters = element.ElementsAfterSelf("filter");
                            foreach (XElement el in filters)
                            {
                                foreach (var datafiltered in ApplyFilter(el, datatable))                                
                                queries.Add(datafiltered);
                            }
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
        public static List<DataTable> GetDataForFilters(String FilterName)
        {
            List<DataTable> FiltersData = new List<DataTable>();

            //Extracting Filters            
            IEnumerable<XElement> filters =
                from el in doc.Elements("filters").Descendants()
                where (string)el.Attribute("name") == FilterName
                select el;

            foreach (XElement el in filters)
                FiltersData.Add(DBClient.getQueryResultset(el.Value));

            return FiltersData;
        }

        public static List<DataTable> ApplyFilter(XElement element, DataTable Data)
        {
            List<DataTable> filters = GetDataForFilters(element.Value.ToString());
            List<DataTable> dataFiltered = new List<DataTable>();
            foreach (var filter in filters)
            {
                foreach (DataRow row in filter.Rows)
                {                    
                    for (var i = 0; i < filter.Columns.Count; i++)
                    {
                        var CurrentFilter = row[i];
                        if (CurrentFilter.GetType() == typeof(String))
                        { 
                            var currentData = Data.AsEnumerable()
                            .Where(r => r.Field<string>(element.Value.ToString()) == CurrentFilter.ToString().Trim())
                            .CopyToDataTable();
                            currentData.ExtendedProperties.Add("Path", "/public_html/ETL/items/");
                            currentData.ExtendedProperties.Add("FileName", Data.TableName);
                            dataFiltered.Add(currentData);
                        }
                        else
                        {
                            dataFiltered.Add(Data.AsEnumerable()
                            .Where(r => r.Field<dynamic>(element.Value.ToString()) == CurrentFilter)
                            .CopyToDataTable());
                        }

                    }
                }                
            }
            return dataFiltered;
        }     

    }
}
//<queriesLibrary>
//	<filters>       
//		<sql name = "seller" > select name from sellers</sql>        		
//		<sql name = "seller2" > select name from sellers</sql>        		
//	</filters>
//	<queries>
//        <query name = "items" >

//            < sql > select itemid, skucode, seller from items</sql>
//            <filter field = "3" includeInFile="false">seller</filter>
//            <path>/ftp/{seller}/clients.csv</path>
//        </query>        
//        <query name = "providers" >
//            < sql > select itemid, skucode from items where balance &gt; 0 </sql>
//            <filter>seller2</filter>
//            <path>/ ftp /{seller}/items.csv</path>
//        </query>        
//	</queries>
//</queriesLibrary>

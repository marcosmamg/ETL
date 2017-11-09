using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ETL
{
    class QueryBuilder
    {
        //queries.xml file must be on the root of the .exe file
        private static XElement doc = XElement.Load(Utilities.BaseDirectory() + "queries.xml");
        
        //Method to get Data from SQL Queries configured in XML File
        public static List<DataTable> GetDataFromSQL()
        {
            List<DataTable> queries = new List<DataTable>();
            try
            {                
                //Extracting queries with no filters yet from XML
                foreach (XElement element in doc.XPathSelectElement("//queries").Descendants())
                {
                    switch (element.Name.ToString(
                        ))
                    {
                        case "sql":
                            var datatable= DBClient.getQueryResultset(element.Value);

                            IEnumerable<XElement> path = element.ElementsAfterSelf("path");
                            foreach (XElement el in path)
                            {
                                datatable.TableName = el.Attribute("filename").Value;
                            }

                            //Extracting filter of the current Node
                            IEnumerable<XElement> filters = element.ElementsAfterSelf("filter");

                            //Iterating over all found filters to apply it to the datatable
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
        private static List<DataTable> GetDataForFilters(String FilterName)
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

        private static List<DataTable> ApplyFilter(XElement element, DataTable Data)
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
                            var DataRow = Data.AsEnumerable()
                            .Where(r => r.Field<string>(element.Value.ToString()) == CurrentFilter.ToString().Trim());
                            if (DataRow.Count() > 0)
                            {
                                var currentData = DataRow.CopyToDataTable();
                                if (!bool.Parse(element.Attribute("includeInFile").Value.ToString()))
                                    currentData.Columns.RemoveAt(Int32.Parse(element.Attribute("field").Value) - 1);

                                currentData.ExtendedProperties.Add("Path", GetPath(element, CurrentFilter.ToString().Trim()));
                                currentData.ExtendedProperties.Add("FileName", Data.TableName);
                                dataFiltered.Add(currentData);
                            }
                            
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

        private static String GetPath(XElement element, String Filter)
        {
            String Path = null;
            IEnumerable<XElement> paths = element.ElementsAfterSelf("path");
            foreach (XElement el in paths)
            {
                Path = el.Value;
            }
            Regex rgx = new Regex("\\{\\w+\\}");            
            return rgx.Replace(Path, Filter);
        }
             

    }
}
//<queriesLibrary>
//	<!-- For filters the attribute "name" is required and it must be equal to the column name in the DB -->
//	<filters>       
//		<sql name = "customer" > select column0 from customers</sql>        				
//	</filters>
//	<queries>
//      <!-- The query node must have sql, filter and path children elements -->
//		<query name = "items" >
//          < !--The value of the SQL element must be the SQL Query -->
//          <sql>select column0, column1, customer from items</sql>
//			<!-- The attributes field and includeInFile are mandatory for filter,
//          the name must be in the value section and it must be equal to the column name in the DB -->
//			<filter field = "3" includeInFile= "true" > customer </ filter >

//          < !--The attribute filename is mandatory, the value must be the current path
//          the word in curly braces will be replaced by the values given by the filter of this query element -->
//          <path filename = "items.csv" >/ public_html / ETL /{ customer}/</path>
//        </query>        
//      <query name = "providers" >
//          < sql > select column0, column1, customer from items where column3 &gt; 0 </sql>
//          <filter field = "3" includeInFile="true">customer</filter>
//          <path filename = "providers.csv" >/ public_html / ETL /{customer}/</path>
//        </query>      
//	</queries>
//</queriesLibrary>
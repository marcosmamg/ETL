# ETL - Extrac, Tranform(CSV), Load Application

This project is a consoled application that uses C#. It reads an XML file where you define the queries and it will extract the information, convert to CSV and upload to a FTP.

## Define XML
```xml
<queriesLibrary>
	<!-- For filters the attribute "name" is required and it must be equal to the column name in the DB -->
	<filters>       
		<sql name = "customer" >select column0 from customers</sql>        				
	</filters>
	<queries>
        <!-- The query node must have sql, filter and path children elements -->
		<query name = "items" >
			<!-- The value of the SQL element must be the SQL Query -->
            <sql>select column0, column1, customer from items</sql>
			<!-- The attributes field and includeInFile are mandatory for filter,
			the name must be in the value section and it must be equal to the column name in the DB -->
			<filter field = "3" includeInFile="true">customer</filter>
			<!-- The attribute filename is mandatory, the value must be the current path
			the word in curly braces will be replaced by the values given by the filter of this query element -->
            <path filename = "items.csv" >/public_html/ETL/{customer}/</path>
        </query>        
        <query name = "providers" >
            <sql> select column0, column1, customer from items where column3 &gt; 0 </sql>
            <filter field = "3" includeInFile="true">customer</filter>
            <path filename = "providers.csv" >/public_html/ETL/{customer}/</path>
        </query>      
	</queries>
</queriesLibrary>
```xml
## Usage
*	Edit ETL.exe.config and set the values for the FTP and the ODBC Connection
*   Make sure the XML file is on the root of the .exe
*	Run the Exe and refer to the logs files: ETLErrorLog.txt and ETLLog.txt
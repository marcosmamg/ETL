using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ETL
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {        

        public void InstallerSetup()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {            
            base.Install(stateSaver);
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            try
            {
                AddConfigurationFileDetails();
            }
            catch (Exception e)
            {
                Utilities.Logger("Error updating connection string: " + e.Message, "error");
                base.Rollback(savedState);
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }

        private void AddConfigurationFileDetails()
        {            
            try
            {
                string dsnParameter = Context.Parameters["DSN"];
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Dsn=");
                stringBuilder.Append(dsnParameter);
                stringBuilder.Append(";Trusted_Connection=Yes");
                dsnParameter = stringBuilder.ToString();                

                ExeConfigurationFileMap map = new ExeConfigurationFileMap();                
                
                //Getting the path location 
                string configFile = string.Concat(Assembly.GetExecutingAssembly().Location, ".config");
                map.ExeConfigFilename = configFile;

                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.
                OpenMappedExeConfiguration(map, System.Configuration.ConfigurationUserLevel.None);

                string connectionsection = config.ConnectionStrings.ConnectionStrings.ToString();                
                ConnectionStringSettings connectionstring = null;

                if (connectionsection != null)
                {
                    config.ConnectionStrings.ConnectionStrings.Remove("ETL");                    
                }

                connectionstring = new ConnectionStringSettings("ETL", dsnParameter);
                connectionstring.ProviderName = "System.Data.Odbc";                
                config.ConnectionStrings.ConnectionStrings.Add(connectionstring);                

                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            catch
            {
                throw;
            }        
        }
    }
}

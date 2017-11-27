using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

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
                dsnParameter = "Dsn=" + dsnParameter + @";Trusted_Connection=" + "Yes\" providerName =\"System.Data.Odbc\"";
                MessageBox.Show("instance =" + dsnParameter);

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
                    MessageBox.Show("Removing existing Connection String and adding new");
                }

                connectionstring = new ConnectionStringSettings("ETL", dsnParameter);
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

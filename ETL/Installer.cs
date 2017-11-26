using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
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
                Utilities.Logger("Falló actualizando archivos de configuración: " + e.Message, "error");
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
                string dnsParameter = Context.Parameters["DSN"];

                // Get the path to the executable file that is being installed on the target computer  
                string assemblypath = Context.Parameters["assemblypath"];
                string appConfigPath = assemblypath + ".config";

                // Write the path to the app.config file  
                XmlDocument doc = new XmlDocument();
                doc.Load(appConfigPath);

                XmlNode connectionStrings = null;
                foreach (XmlNode node in doc.ChildNodes)
                    if (node.Name == "connectionStrings")
                        connectionStrings = node;

                if (connectionStrings != null)
                {                    
                    // Get the ‘appSettings’ node  
                    XmlNode settingNode = null;
                    foreach (XmlNode node in connectionStrings.ChildNodes)
                    {
                        if (node.Name == "appSettings")
                            settingNode = node;
                    }

                    if (settingNode != null)
                    {
                        //MessageBox.Show("settingNode != null");  
                        //Reassign values in the config file  
                        foreach (XmlNode node in settingNode.ChildNodes)
                        {
                            //MessageBox.Show("node.Value = " + node.Value);  
                            if (node.Attributes == null)
                                continue;
                            XmlAttribute attribute = node.Attributes["value"];
                            //MessageBox.Show("attribute != null ");  
                            //MessageBox.Show("node.Attributes['value'] = " + node.Attributes["value"].Value);  
                            if (node.Attributes["key"] != null)
                            {
                                //MessageBox.Show("node.Attributes['key'] != null ");  
                                //MessageBox.Show("node.Attributes['key'] = " + node.Attributes["key"].Value);  
                                switch (node.Attributes["key"].Value)
                                {
                                    case "TestParameter":
                                        //Dsn=ETL;Trusted_Connection=Yes
                                        attribute.Value = "Dsn=" + dnsParameter + ";Trusted_Connection=Yes";
                                        break;
                                }
                            }
                        }
                    }
                    doc.Save(appConfigPath);
                }
            }
            catch
            {
                throw;
            }        
        }
    }
}

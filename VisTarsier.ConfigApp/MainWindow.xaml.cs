using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisTarsier.Common;
using VisTarsier.Config;
using VisTarsier.Service;
using static VisTarsier.Config.DicomConfig;

namespace VisTarsier.ConfigApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Prep stuff needed to remove close button on window
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //private static string[] _aeDestVals;
        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Code to remove close box from window
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        public MainWindow()
        {
            Loaded += ToolWindow_Loaded;
            InitializeComponent();
            chkSameDest.Checked += ChkSameDest_Checked;
            chkSameDest.Unchecked += ChkSameDest_Unchecked;

            //Load in the current configuration.
            var conf = CapiConfig.GetConfig();
            if (conf.DicomConfig?.RemoteNodes != null && conf.DicomConfig.RemoteNodes.Count > 0)
            {
                var src = conf.DicomConfig.RemoteNodes[0];
                txtAETitle.Text = src.AeTitle;
                txtAEHost.Text = src.IpAddress;
                txtAEPort.Text = src.Port.ToString();

                if (conf.DicomConfig.RemoteNodes.Count > 1)
                {
                    var dest = conf.DicomConfig.RemoteNodes[1];
                    txtAETitle1.Text = dest.AeTitle;
                    txtAEHost.Text = dest.IpAddress;
                    txtAEPort.Text = dest.Port.ToString();
                    chkSameDest.IsChecked = false;
                }
            }

            // Parse the connection string and add to fields.
            ParseConnectionString(conf.AgentDbConnectionString, out var server, out var user, out var pass, out var timeout, out var dbname);
            txtDBServer.Text = server;
            txtDBUser.Text = user;
            txtDBPassword.Text = pass;
            txtDBTimeout.Text = timeout.ToString();
            txtDBName.Text = dbname;
        }

        private void log(string line)
        {
            line = $"[{new DateTime().ToString()}] - {line}";
            var lines = new List<string>();
            lines.Add(line);
            File.AppendAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\configlog.txt", lines);
                
        }

        /// <summary>
        /// This is a helper method to help parse the connection string values from the config.
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="timeout"></param>
        /// <param name="dbname"></param>
        private void ParseConnectionString(string dbConnectionString, out string server, out string user, out string pass, out int timeout, out string dbname)
        {
            server = null;
            user = null;
            pass = null;
            dbname = null;
            timeout = 999;

            foreach (var str in dbConnectionString.Split(';'))
            {
                var s = str.TrimEnd(';');
                if (s.StartsWith("Server=")) server = s.Substring(7);
                else if (s.StartsWith("User Id=")) user = s.Substring(8);
                else if (s.StartsWith("Password=")) pass = s.Substring(9);
                else if (s.StartsWith("Connection Timeout=")) timeout = int.Parse(s.Substring(19));
                else if (s.StartsWith("Database=")) dbname = s.Substring(9);
            }
        }

        /// <summary>
        /// Disable the destination fields if same as source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSameDest_Checked(object sender, RoutedEventArgs e)
        {
            txtAETitle1.IsEnabled = false;
            txtAEHost1.IsEnabled = false;
            txtAEPort1.IsEnabled = false;
        }

        /// <summary>
        /// Enable the destination fields if not the same as source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSameDest_Unchecked(object sender, RoutedEventArgs e)
        {
            txtAETitle1.IsEnabled = true;
            txtAEHost1.IsEnabled = true;
            txtAEPort1.IsEnabled = true;
        }

        /// <summary>
        /// Saves the config to the appropriate locations.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var conf = CapiConfig.GetConfig();

            // Just error checking that timeout is a number.
            try { int.Parse(txtDBTimeout.Text); }
            catch (FormatException) { ShowError("Connection Timeout must be a number"); return; }
            // Add database settings.
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = txtDBServer.Text,
                UserID = txtDBUser.Text,
                Password = txtDBPassword.Text,
                InitialCatalog = txtDBName.Text,
            };
            conf.AgentDbConnectionString = builder.ConnectionString;

            // Set run interval.
            try
            {
                conf.RunInterval = int.Parse(txtRunInterval.Text).ToString();
            }
            catch (FormatException)
            {
                ShowError("Run Interval must be a number");
                return;
            }
            // Setup remote nodes.
            var nodes = new List<IDicomNode>();
            try
            {
                nodes.Add(new DicomConfigNode()
                {
                    AeTitle = txtAETitle.Text,
                    IpAddress = txtAEHost.Text,
                    LogicalName = txtAETitle.Text,
                    Port = int.Parse(txtAEPort.Text)
                });
                if (!(bool)chkSameDest.IsChecked)
                {
                    nodes.Add(new DicomConfigNode()
                    {
                        AeTitle = txtAETitle1.Text,
                        IpAddress = txtAEHost1.Text,
                        LogicalName = txtAETitle1.Text,
                        Port = int.Parse(txtAEPort1.Text)
                    });
                }
                conf.DicomConfig.RemoteNodes = nodes;

                // Setup local node
                conf.DicomConfig.LocalNode = new DicomConfigNode()
                {
                    AeTitle = txtAETitleLocal.Text,
                    IpAddress = txtAEHostLocal.Text,
                    LogicalName = txtAETitleLocal.Text,
                    Port = int.Parse(txtAEPortLocal.Text)
                };
            }
            catch (FormatException)
            {
                ShowError("Port must be a number");
                return;
            }

            // Try writing the configuration to file. 
            try { CapiConfig.WriteConfig(conf); }
            catch (Exception) { ShowError("Could not write settings."); return; }
            // Create the default recipe
            try
            {
                var recipe = CapiConfig.GetDefaultRecipe();
                recipe.SourceAet = txtAETitle.Text;
                recipe.OutputSettings.DicomDestinations.Clear();
                recipe.OutputSettings.DicomDestinations.Add(txtAETitle.Text);
                recipe.OutputSettings.ReslicedDicomSeriesDescription = txtDescription.Text + " Resliced";
                recipe.OutputSettings.ResultsDicomSeriesDescription = txtDescription.Text + " Results";
                SetupDefaultRecipeInDb(recipe);
                CapiConfig.WriteRecipe(recipe); 
            }
            catch (Exception ex) 
            {
                ShowError("Could not write recipe." + ex.Message); return; 
            }
            try
            {
                log("Setting up web configs..");
                var webPort = int.Parse(txtWebPort.Text);

                var uiServerFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{System.IO.Path.DirectorySeparatorChar}web{System.IO.Path.DirectorySeparatorChar}nodejs{System.IO.Path.DirectorySeparatorChar}server.js"));

                // Setup Dicom backend
                var dicomServerFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{System.IO.Path.DirectorySeparatorChar}web{System.IO.Path.DirectorySeparatorChar}restfuldicom{System.IO.Path.DirectorySeparatorChar}rest-dicom.py"));
                var dsf = File.ReadAllLines(dicomServerFile);
                for (int i = 0; i < dsf.Length; ++i)
                {
                    if (dsf[i].StartsWith("IPLIST = "))
                    {
                        if (nodes.Count == 1)
                        {
                            dsf[i] = "IPLIST = { '" + nodes[0].AeTitle + "': '" + nodes[0].IpAddress + "'}";
                        }
                        else
                        {
                            dsf[i] = "IPLIST = { '" + nodes[0].AeTitle + "': '" + nodes[0].IpAddress + "', '" + nodes[1].AeTitle + "': '" + nodes[1].IpAddress + "'}";
                        }
                    }
                    else if (dsf[i].StartsWith("PORT = "))
                    {
                        //Note: this may cause problems if the PACS are on different ports
                        dsf[i] = "PORT = " + nodes[0].Port + "# Note: this may cause problems if the PACS are on different ports.";
                    }
                    else if (dsf[i].StartsWith("LOCAL_PORT = "))
                    {
                        // The (open) web port for the UI will be used by the node service
                        dsf[i] = "LOCAL_PORT = " + (webPort + 1);
                    }
                    else if (dsf[i].StartsWith("AE_TITLE = "))
                    {
                        // The (open) web port for the UI will be used by the node service
                        dsf[i] = "AE_TITLE = \"" + txtAETitleLocal.Text + "\"";
                    }
                }
                log("Writing dicom server file to " + dicomServerFile);
                File.WriteAllLines(dicomServerFile, dsf);

            }
            catch (Exception ex)
            {
                // TODO: Show error only when installing web components.
                log("ERROR: Could not write python settings: " + ex.Message);
            }

            var nodeDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{System.IO.Path.DirectorySeparatorChar}web{System.IO.Path.DirectorySeparatorChar}nodejs{System.IO.Path.DirectorySeparatorChar}"));
            try
            {
                var confJS = new string[]
                {
                    $"exports.LOG_PATH = '{System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../log/log.txt")).Replace(@"\", @"\\")}';",
                    $"exports.DEFAULT_RECIPE = '{nodeDir.Replace(@"\", @"\\")}' + 'default.recipe.json';",
                    $"exports.MANUAL_CASE_PATH = '{(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../cases/manual")).Replace(@"\", @"\\"))}';",
                    $"exports.SQL_CONFIG =",
                    "    {",
                    $"        user: '{txtDBUser.Text}',",
                    $"        password: '{txtDBPassword.Text}',",
                    $"        server: '{txtDBServer.Text.Replace(@"\", @"\\")}',",
                    $"        database: '{txtDBName.Text}',",
                    $"        parseJSON: true,",
                    "    };",
                    $"exports.LOCAL_PORT = {txtWebPort.Text};"
                };

                FileSystem.DirectoryExistsIfNotCreate(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../web/nodejs/")));
                log("writing node config file to: " + System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../web/nodejs/config.js")));
                File.WriteAllLines(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../web/nodejs/config.js")), confJS);
                if (File.Exists(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../web/nodejs/config.js"))))
                {
                    log("Wrote config file for web..");
                }
                else
                {
                    log("something went wrong writing file to " + (System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../web/nodejs/config.js"))));
                }
            }
            catch (Exception) { ShowError("Could not write JS settings."); return; }


            Application.Current.Shutdown();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public void CreateIfDBNotExists(string ConnectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            var databaseName = connectionStringBuilder.InitialCatalog;

            connectionStringBuilder.InitialCatalog = "master";

            using (var connection = new SqlConnection(connectionStringBuilder.ToString()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("select * from master.dbo.sysdatabases where name='{0}'", databaseName);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // exists
                            return;
                    }

                    command.CommandText = string.Format("CREATE DATABASE {0}", databaseName);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void TestDb(object sender, RoutedEventArgs e)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = txtDBServer.Text,
                UserID = txtDBUser.Text,
                Password = txtDBPassword.Text,
                InitialCatalog = txtDBName.Text,
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open(); // throws if invalid
                }
                MessageBox.Show("Connection okay!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void SetupDefaultRecipeInDb(Recipe recipe)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = txtDBServer.Text,
                UserID = txtDBUser.Text,
                Password = txtDBPassword.Text,
                InitialCatalog = txtDBName.Text,
            };

            try
            {
                DbBroker broker = new DbBroker(builder.ConnectionString);
                broker.Database.EnsureCreated();
                broker.Database.ExecuteSqlRaw("IF OBJECTPROPERTY(OBJECT_ID('dbo.StoredRecipes'), 'TableHasIdentity') = 1  SET IDENTITY_INSERT [dbo].[StoredRecipes] ON");
                broker.SaveChanges();
                broker.StoredRecipes.Add(new StoredRecipe()
                {
                    Id = -1,
                    UserEditable = false,
                    Name = "Manual Recipe",
                    RecipeString = "{}"
                });
                broker.StoredRecipes.Add(new StoredRecipe()
                {
                    Id = 1,
                    UserEditable = false,
                    Name = "MS Lesion Compare",
                    RecipeString = JsonConvert.SerializeObject(recipe)
                });
                broker.SaveChanges();
            }
            catch (Exception ex)
            {
                do
                {
                    log(ex.Message);
                    ex = ex.InnerException;
                } while (ex != null);
               
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Printing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using System.ServiceProcess;
using System.DirectoryServices;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
//using System.Xml.Linq;
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ITTools
{

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;


        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <returns></returns>
        public static bool Flash(System.Windows.Forms.Form form)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(System.Windows.Forms.Form form, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL, count, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// Start Flashing the specified Window (form)
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <returns></returns>
        public static bool Start(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// Stop Flashing the specified Window (form)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static bool Stop(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_STOP, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return System.Environment.OSVersion.Version.Major >= 5; }
        }


        BackgroundWorker backgroundWorker1;
        BackgroundWorker bg_Restore;

        string path = Environment.CurrentDirectory.ToString();
        string Checknull = "";
        string fileDBName = ""; string pathDBName = "";
        string DatabaseFileName = "";
        string readlog = "";
        string FolderBackup = "";
        string ThemeBackup = "";
        string AddressGiftIP = "";
        string AddressIP = "";
        string URLIP = "";
        string POSURL = "";
        string build_address = "";
        string GiftCardPOSURL = "";
        string configfilepath = "";
        string BLSIKey, HKey_POS, HKey_POSD, HKey_SQL, HKey_CPanel, HKey_Restore, HKey_Backup, HKey_Update, HKey_Printer, HKey_Build, Build_IP = "";
        string HKey_POS_Key, HKey_POSD_Key, HKey_SQL_Key, HKey_CPanel_Key, HKey_Restore_Key, HKey_Backup_Key, HKey_Update_Key, HKey_Printer_Key, HKey_Build_Key, Build_IP_Key = "";
        int counter = 0;
        int len = 0;
        string processing_text;

        //check before close form - 03.13.2019
        public static int BackupDB = 0, Restored_pos = 0, Restored_merchant = 0, Updated_pos = 0, Updated_merchant = 0;
        public static string BackupDB_act = "", Restored_pos_act = "", Restored_merchant_act = "", Updated_pos_act = "", Updated_merchant_act = "", printer_status = "", printer_name = "";

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, int message, int wparam, int lparam);

        private const int SbBottom = 0x7;
        private const int WmVscroll = 0x115;

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;


            //bg_Restore
            bg_Restore = new BackgroundWorker();
            bg_Restore.WorkerReportsProgress = true;
            bg_Restore.WorkerSupportsCancellation = true;

            bg_Restore.DoWork += bg_Restore_DoWork;
            bg_Restore.RunWorkerCompleted += bg_Restore_RunWorkerCompleted;
        }
        #region       SERVICES LOAD

        private void Form1_Load(object sender, EventArgs e)
        {
            //Clock
            System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer();
            tmr.Interval = 1000;//ticks every 1 second
            tmr.Tick += new EventHandler(timer1_Tick);
            tmr.Start();

            rtb1.Text = "";
            BLSIKey = @"SOFTWARE\WOW6432Node\IT Tool\Configurations"; ;
            ReadKey();
            CheckService();

            //rdbDefault.Checked = true;
            rdbCustom.Checked = true;
            //txtDBBK.Enabled = false;
            cbbDatabase.Focus();
            //loadingPictureBox.Hide();
            
            //set version info
            //Version version = Assembly.GetExecutingAssembly().GetName().Version;
            //this.lblStatus.Text = String.Format(this.lblStatus.Text, version.Major, version.Minor, version.Build, version.Revision);

            
        }
        #endregion

        public void CheckService()
        {
            ListDatabase();
            IISServiceStatus();
            SQLServiceStatus();
            CheckPOSURL();
            CheckPOSConfig();
            CheckIPLocal();
            CheckPrinterStatus();
        }

        public void ChangeDB()
        {
            string file = "C:\\inetpub\\wwwroot\\BLogicService\\Web.Config";
            //string textrp = "=" + txtDatabase.Text + ";";
            if (File.Exists(file))
            {
                int i = 0;
                string textcontent = "";
                textcontent = File.ReadAllText(file);
                string textrp = cbbDatabase.Text;
                //=blogicpos7;
                string[] lines = File.ReadAllLines(file);
                int flag = 0;
                if (textrp != "")
                {

                    for (i = 0; i <= lines.Length - 1; i++)
                    {
                        //if (lines[i].contains("=blogicpos7;"))
                        if (lines[31] == "BlogicPOS7")
                        {
                            textcontent = textcontent.Replace("=BlogicPOS7;", textrp);
                            File.WriteAllText("C:\\inetpub\\wwwroot\\BLogicService\\Web.Config", textcontent);
                            //MessageBox.Show("Done!");
                            rtb1.Text = "Database " + textrp + " was renamed"; ;
                            flag++;
                        }
                        else
                        {
                            string data = getBetween(lines[31], "SQLEXPRESS;Initial Catalog=", ";integrated");
                            if (data == "")
                            {
                                //data = "N/A";
                                MessageBox.Show("Database đang để rỗng!");
                                return;
                            }

                            //MessageBox.Show("Database name " + data + " is using.");
                            rtb1.Text = "Database name " + data + " is using.";
                            textcontent = textcontent.Replace(data, textrp);

                            File.WriteAllText("C:\\inetpub\\wwwroot\\BLogicService\\Web.Config", textcontent);
                            //MessageBox.Show("Đã thay đổi tên Database!");
                            MessageBox.Show("Database " + textrp + " was renamed");
                            Form1.Flash(this);
                            return;
                        }
                    }
                    if (flag == 0)
                    {
                        //MessageBox.Show("Không tìm thấy!");
                        rtb1.Text = "Không tìm thấy";
                    }
                    else rtb1.Text = "Không tìm thấy file";

                }
                else
                {
                    rtb1.ForeColor = Color.Red;
                    rtb1.Text = "Please enter the database name!";
                    //txtDatabase.BackColor = Color.FromArgb(65, 131, 215);
                    //txtDatabase.Focus();
                }



            }
        }

        public void ColorPanel_Error()
        {
            //btnHelp.BackColor = Color.FromArgb(211, 84, 0);
            btnHelp.Image = ITTools.Properties.Resources.warning;
        }

        public void ColorPanel_Fine()
        {
            //btnHelp.BackColor = Color.FromArgb(44, 62, 80);
            btnHelp.Image = ITTools.Properties.Resources.verified;
        }

        public void CheckIPLocal()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("10.0.1.20", 1337); // doesnt matter what it connects to
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    //Console.WriteLine(endPoint.Address.ToString()); //ipv4
                    //MessageBox.Show(endPoint.Address.ToString());
                    lblIP.Text = endPoint.Address.ToString();//
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed"); // If no connection is found
            }
        }

        private void LoadConfigFile()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                //Console.WriteLine("64bit");
                configfilepath = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";

            }
            else
            {
                //Console.WriteLine("32bit");
                configfilepath = @"C:\Program Files\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe";
            }
            var fileconfig = ConfigurationManager.OpenExeConfiguration(configfilepath);
            
            var pst = fileconfig.GetSectionGroup("system.serviceModel").ToString();
            string p = pst;
            var PostSetting = fileconfig.GetSection("system.serviceModel/client").ToString();
           // PostSetting = fileconfig.Sections["client"];
            string p2 = PostSetting;
            //PostSetting["name"].ToString();

            GiftCardPOSURL = fileconfig.Sections.Get("WSHttpBinding_IGiftServiceWcf").ToString();
            //txtPOSConfig.Text = PostSetting;
        }

        private void CheckPOSURL()
        {
            string subKey = @"SOFTWARE\WOW6432Node\BLogic Systems\BLogic PointOfSale";
            string str = ReadSubKeyValue(subKey, "URL");
            txtURL.Text = str;
        }
        static string ReadSubKeyValue(string subKey, string key)

        {

            string str = string.Empty;

            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(subKey))

            {

                if (registryKey != null)

                {

                    str = registryKey.GetValue(key).ToString();

                    registryKey.Close();

                }

            }

            return str;

        }

        private void CheckPOSConfig()
        {
            string file = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";
            if (File.Exists(file))
            {
                int i = 0;
                string textcontent = "";
                textcontent = File.ReadAllText(file);
                //XDocument doc = XDocument.Parse(textcontent);
                //doc.Save("2.xml");
                //string address_value = txtPOSConfig.Text;
                //doc.Root.Element("configuration").Element("system.serviceModel").Element("client").Element("endpoint").Attribute("address").SetValue(address_value);
                //doc.Save("3.xml");
                //string textrp = txtDatabase.Text;
                string[] lines = File.ReadAllLines(file);

                //Console.WriteLine(lines[248]);
                //int flag = 0;
                for (i = 0; i <= lines.Length - 1; i++)
                {
                    string data = getBetween(lines[247], "endpoint address=\"", "\" binding"); //GiftCard address
                    AddressGiftIP = lines[247];
                    txtPOSConfig.Text = data;

                }
                //MessageBox.Show(AddressGiftIP);
            }
            else
                txtPOSConfig.Text = "BLOGICPOS.exe.config file not exists!";


            //if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            //{
            //    //Console.WriteLine("64bit");
            //    configfilepath = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";

            //}
            //else
            //{
            //    //Console.WriteLine("32bit");
            //    configfilepath = @"C:\Program Files\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe";
            //}
            //var fileconfig = ConfigurationManager.OpenExeConfiguration(configfilepath);
            //var PostSetting = fileconfig.GetSection("client/endpoint").ToString();
            ////PostSetting["name"].ToString();

            //GiftCardPOSURL = fileconfig.Sections.Get("WSHttpBinding_IGiftServiceWcf").ToString();
            //txtPOSConfig.Text = PostSetting;
        }
        private void CheckGiftConfig()
        {
            string file = @"C:\Program Files (x86)\BLogic Systems\BLogicGiftCard\GiftGUI\GiftClient\BLogicGift.exe.config";
            if (File.Exists(file))
            {
                int i = 0;
                string textcontent = "";
                textcontent = File.ReadAllText(file);
                //string textrp = txtDatabase.Text;
                string[] lines = File.ReadAllLines(file);

                //Console.WriteLine(lines[248]);
                //int flag = 0;
                for (i = 0; i <= lines.Length - 1; i++)
                {
                    string data = getBetween(lines[106], "endpoint address=\"", "\" binding"); //GiftCard address
                    txtPOSConfig.Text = data;
                }

            }
            else
                txtPOSConfig.Text = "BLOGICPOS.exe.config file not exists!";
        }

        private void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            txtPOSConfig.Text = arrLine[line_to_edit - 1];
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        private void EditGiftAdress()
        {
            string POSConfigfile = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";
            if (File.Exists(POSConfigfile))
            {
                int i = 0;
                string textcontent = "";

                textcontent = File.ReadAllText(POSConfigfile);
                string textrp = txtPOSConfig.Text;
                //=;
                string[] lines = File.ReadAllLines(POSConfigfile);
                int flag = 0;
                

                //if (lines[i].contains("=blogicpos7;"))
                if(lines[247] == "localhost")
                {
                    textcontent = textcontent.Replace("localhost", textrp);
                    File.WriteAllText(POSConfigfile, textcontent);
                    rtb1.Text = "Database " + textrp + " was renamed";
                }
                else
                {
                    string data = getBetween(lines[247], "endpoint address=\"http://", ":8200/BLogicGiftService\" binding");
                    textcontent = textcontent.Replace(data, textrp);
                    File.WriteAllText(POSConfigfile, textcontent);
                    rtb1.Text = "Database " + textrp + " was renamed";
                }


                
                    
                

            }

                
            
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }

        string serviceshortcut_str = "";
        string service_cmd = "";
        string r_service_cmd = "";
        string DFPrinter;
        PrintServer PrintSV;
        PrintQueue PrintQ;
        string PrinterStatus = "";
        public void Printer()
        {
            PrinterSettings PrinterDFName = new PrinterSettings();

            DFPrinter = PrinterDFName.PrinterName;
            PrintSV = new PrintServer();
            //PrintQ = new PrintQueue(PrintSV, DFPrinter, PrintSystemDesiredAccess.UsePrinter);
            PrintQ = new PrintQueue(PrintSV, DFPrinter, PrintSystemDesiredAccess.AdministratePrinter);
            

        }

        private void CheckPrinterStatus()
        {
            Printer();

            if (PrintQ == null)
            {
                lblDFPrinterName.Text = "None";
            }
            else
            {
                if (PrintQ.IsPaused == true)
                {
                    
                    lblDFPrinter.ForeColor = Color.OrangeRed;
                    PrinterStatus = " PAUSED";
                }
                else if (PrintQ.IsOffline == true)
                {
                    
                    lblDFPrinter.ForeColor = Color.PaleVioletRed;
                    PrinterStatus = " OFFLINE";
                }
                else
                {
                    PrinterStatus = " ON";
                    lblDFPrinter.ForeColor = Color.ForestGreen;
                }
                
            }

            lblDFPrinterName.Text = DFPrinter;
            lblDFPrinter.Text = PrinterStatus;
            printer_status = PrinterStatus;
            printer_name = DFPrinter;


        }

        private void Stop_Service()
        {
            switch (serviceshortcut_str)
            {
                
                case "IIS Service":
                    service_cmd = "/C iisreset /stop";
                    try
                    {
                        
                        ExecuteCommand2(service_cmd);
                        


                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {

                        //Form1.Flash(this);
                        IISServiceStatus();
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("IIS Service is processed.");
                    }
                    break;
                case "SQL Service":
                    service_cmd = "/C net stop MSSQL$SQLEXPRESS";
                    try
                    {
                        
                        ExecuteCommand2(service_cmd);
                        
                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {
                        //Form1.Flash(this);
                        SQLServiceStatus();
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("SQL Service is processed.");
                    }
                    break;
            }
        }

        private void Restart_Service()
        {
            switch (serviceshortcut_str)
            {
                
                case "IIS Service":
                    service_cmd = "/C iisreset /stop";
                    r_service_cmd = "/C iisreset /start";
                    try
                    {
                        
                        ExecuteCommand2(service_cmd);
                        ExecuteCommand2(r_service_cmd);
                        

                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {
                        
                        IISServiceStatus();
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("IIS Service is processed.");
                    }
                    break;
                case "SQL Service":
                    service_cmd = "/C net stop MSSQL$SQLEXPRESS";
                    r_service_cmd = "/C net start MSSQL$SQLEXPRESS";
                    try
                    {
                        
                        ExecuteCommand2(service_cmd);
                        ExecuteCommand2(r_service_cmd);
                        
                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {
                        //Form1.Flash(this);
                        SQLServiceStatus();
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("SQL Service is processed.");
                    }
                    break;
            }
        }

        private void Restore()
        {
            
                try
                {
                    string reDB1 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"ALTER DATABASE " + DatabaseFileName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE\"";
                    ExecuteCommand2(reDB1);


                    string reDB2 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"RESTORE DATABASE " + DatabaseFileName + " FROM DISK = \'" + pathDBName + "\' WITH REPLACE\"";

                    ExecuteCommand2(reDB2);
                }
                catch (Exception objException)
                {
                    //rtb1.Text = objException;
                }
                finally
                {
                    Form1.Flash(this);
                    MessageBox.Show("Database '"+ DatabaseFileName + "' is processed.");
                }
            
        }

        private void UpdateDB()
        {
            try
            {
                //DatabaseFileName = cbbDatabase.Text;
                string reDB2 = @"/C sqlcmd -S .\SQLExpress -d " + DatabaseFileName + " -i " +pathDBName+ "";
                ExecuteCommand2(reDB2);


            }
            catch (Exception objException)
            {
                //rtb1.Text = objException;
            }
            finally
            {
                Form1.Flash(this);
                MessageBox.Show("Update database '" + DatabaseFileName + "' is processed.");
            }
        }

        private void RestoreDB()
        {
            try
            {
                string currentpath = Environment.CurrentDirectory.ToString();
                string fileDBName = ""; string pathDBName = "";
                OpenFileDialog bDialog = new OpenFileDialog();
                bDialog.Title = "Choose Your Backup Database File";
                bDialog.Filter = "BAK Files|*.bak";
                bDialog.InitialDirectory = currentpath;
                //Kiem tra txt Database Restore Name da co hay chua

                if (cbbDatabase.Text == "")
                {
                    rtb1.ForeColor = Color.Red;
                    rtb1.Text = "Please enter the DATA RESTORE NAME!";

                    cbbDatabase.Focus();
                }
                else
                {
                    if (bDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileDBName = bDialog.SafeFileName;
                        pathDBName = bDialog.FileName;
                    }
                    else return;
                    string DatabaseFileName = cbbDatabase.Text;
                    rtb1.Text = DatabaseFileName;
                    try
                    {


                        string reDB1 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"ALTER DATABASE " + DatabaseFileName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE\"";
                        ExecuteCommand2(reDB1);


                        string reDB2 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"RESTORE DATABASE " + DatabaseFileName + " FROM DISK = \'" + pathDBName + "\' WITH REPLACE\"";

                        ExecuteCommand2(reDB2);




                    }
                    catch (Exception objException)
                    {
                        //rtb1.Text = objException;
                    }
                    finally
                    {
                        //loadingPictureBox.Hide();
                        rtb1.ForeColor = Color.Green;
                        lblStatus.Text = "Database restore successful!!!";
                    }
                }
            }
            catch
            {

            }
        }

        

        private void bg_Restore_DoWork(object sender, DoWorkEventArgs e)
        {
            //RestoreDB();
            try
            {
                string currentpath = Environment.CurrentDirectory.ToString();
                string fileDBName = ""; string pathDBName = "";
                OpenFileDialog bDialog = new OpenFileDialog();
                bDialog.Title = "Choose Your Backup Database File";
                bDialog.Filter = "BAK Files|*.bak";
                bDialog.InitialDirectory = currentpath;
                //Kiem tra txt Database Restore Name da co hay chua

                if (cbbDatabase.Text == "")
                {
                    rtb1.ForeColor = Color.Red;
                    rtb1.Text = "Please enter the DATA RESTORE NAME!";

                    cbbDatabase.Focus();
                }
                else
                {
                    if (bDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileDBName = bDialog.SafeFileName;
                        pathDBName = bDialog.FileName;
                    }
                    else return;
                    string DatabaseFileName = cbbDatabase.Text;
                    rtb1.Text = DatabaseFileName;
                    try
                    {


                        string reDB1 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"ALTER DATABASE " + DatabaseFileName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE\"";
                        ExecuteCommand2(reDB1);


                        string reDB2 = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"RESTORE DATABASE " + DatabaseFileName + " FROM DISK = \'" + pathDBName + "\' WITH REPLACE\"";

                        ExecuteCommand2(reDB2);




                    }
                    catch (Exception objException)
                    {
                        
                    }
                    finally
                    {
                        
                    }
                }
            }
            catch
            {

            }
        }

        private void bg_Restore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //loadingPictureBox.Hide();
            readlog = File.ReadAllText(@path + @"\log.txt");
            rtb1.Text = readlog;
        }

        //CMD
        public void ExecuteCommand(string command)
        {

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

            //strCommand is path and file name of command to run
            pProcess.StartInfo.FileName = "CMD.exe";

            //strCommandParameters are parameters to pass to program
            pProcess.StartInfo.Arguments = command;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pProcess.StartInfo.UseShellExecute = false;

            //Set output of program to be written to process output stream
            pProcess.StartInfo.RedirectStandardOutput = true;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using-statement will close.

                pProcess.Start();
                {
                    pProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thực hiện backup " + ex.Message);
            }
            finally
            {
                string path = Environment.CurrentDirectory.ToString();
                string strOutput = pProcess.StandardOutput.ReadToEnd();
                rtb1.Text += strOutput + "\n";
                File.WriteAllText(path + @"\logBIT.txt", rtb1.Text);
            }


        }

        
        //Check Status Service
        public void IISServiceStatus()
        {

            ServiceController sc = new ServiceController("W3SVC");
            if(sc == null)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    lbl_IIS_stt.Text = "NONE";
                    lbl_IIS_stt.ForeColor = Color.Gray;
                    //MessageBox.Show("ON");
                });
            }
            else
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            lbl_IIS_stt.Text = "ON";
                            lbl_IIS_stt.ForeColor = Color.Green;
                            //MessageBox.Show("ON");
                        });
                        
                        break;
                    case ServiceControllerStatus.Stopped:
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            lbl_IIS_stt.Text = "OFF";
                            lbl_IIS_stt.ForeColor = Color.Red;
                            //MessageBox.Show("OFF");
                        });
                        
                        break;
                    case ServiceControllerStatus.Paused:
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            lbl_IIS_stt.Text = "PAUSE";
                            lbl_IIS_stt.ForeColor = Color.Red;
                            //MessageBox.Show("PAUSE");
                        });
                        
                        break;
                }
            }
            
        }
        public void SQLServiceStatus()
        {

            ServiceController sc = new ServiceController("MSSQL$SQLEXPRESS");

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        lbl_SQL_stt.Text = "ON";
                        lbl_SQL_stt.ForeColor = Color.Green;
                        //MessageBox.Show("ON");
                    });
                    break;
                case ServiceControllerStatus.Stopped:
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        lbl_SQL_stt.Text = "OFF";
                        lbl_SQL_stt.ForeColor = Color.Red;
                        //MessageBox.Show("OFF");
                    });
                    break;
                case ServiceControllerStatus.Paused:
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        lbl_SQL_stt.Text = "PAUSE";
                        lbl_SQL_stt.ForeColor = Color.Red;
                        //MessageBox.Show("PAUSE");
                    });
                    break;
            }
        }

        public void ExecuteCommand2(string command)
        {

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "CMD.exe";
            pProcess.StartInfo.Arguments = command;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            // Set event handler
            pProcess.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            pProcess.Start();

            // Start the asynchronous read
            pProcess.BeginOutputReadLine();

            pProcess.WaitForExit();
            pProcess.Close();



        }
        void SortOutputHandler(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine(e.Data);
            this.BeginInvoke(new MethodInvoker(() =>
            {
                rtb1.AppendText(e.Data + Environment.NewLine ?? string.Empty);
                SendMessage(rtb1.Handle, WmVscroll, SbBottom, 0x0);
            }));
        }

        public void RunPOS()
        {
            string pathPOS = "";
            string POS = "";

            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                Console.WriteLine("64bit");
                POS = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.EXE";
                if (!File.Exists(POS))
                {
                    MessageBox.Show("Can not find POS.");
                    return;
                }
                else
                    pathPOS = POS;
                
            }
            else
            {
                Console.WriteLine("32bit");
                POS = @"C:\Program Files\BLogic Systems\BLogic PointOfSale\BLOGICPOS.EXE";
                if (!File.Exists(POS))
                {
                    MessageBox.Show("Can not find POS.");
                    return;
                }
                else
                    pathPOS = POS;
            }

            try
            {
                //Create process
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.FileName = pathPOS;
                //pProcess.StartInfo.Arguments = command;
                pProcess.StartInfo.UseShellExecute = true;
                pProcess.Start();
            }
            catch (Exception e)
            {
                rtb1.Text = "IOException source: {0}"+ e;
            }
            
        }
        public void RunPOSDashboard()
        {
            string pathPOS = "";
            string POSD = "";
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                POSD = @"C:\Program Files (x86)\BLogic Systems\BLogic POSDashboard\BLogicPOSDashboard.exe";
                if (!File.Exists(POSD))
                {
                    MessageBox.Show("Can not find POS Dashboard.");
                    return;
                }
                else
                    pathPOS = POSD;
            }
            else
            {
                POSD = @"C:\Program Files\BLogic Systems\BLogic POSDashboard\BLogicPOSDashboard.exe";
                if (!File.Exists(POSD))
                {
                    MessageBox.Show("Can not find POS Dashboard.");
                    return;
                }
                else
                    pathPOS = POSD;
            }
                

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = pathPOS;
            //pProcess.StartInfo.Arguments = command;
            pProcess.StartInfo.UseShellExecute = true;
            pProcess.Start();
        }



        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            const int kNotFound = -1;

            int startIdx = strSource.IndexOf(strStart);
            if (startIdx != kNotFound)
            {
                startIdx += strStart.Length;
                int endIdx = strSource.IndexOf(strEnd, startIdx);
                if (endIdx > startIdx)
                {
                    return strSource.Substring(startIdx, endIdx - startIdx);
                }
            }
            return String.Empty;
        }
        // Dark Theme
        public void darktheme()
        {
            //Dark Theme Form
            this.BackColor = Color.Black;

            // Dark Theme Textbox
            //txtDBO.ForeColor = Color.White; txtDBO.BackColor = Color.Black;
            //txtDBBK.ForeColor = Color.White; txtDBBK.BackColor = Color.Black;
            //txtDatabase.ForeColor = Color.White; txtDatabase.BackColor = Color.Black;
            //txtReDB.ForeColor = Color.White; txtReDB.BackColor = Color.Black;
            //txtIP.ForeColor = Color.White; txtIP.BackColor = Color.Black;


            // Dark Theme Label
            //label1.ForeColor = Color.White;
            label2.ForeColor = Color.White;
            //label3.ForeColor = Color.White;
            //label4.ForeColor = Color.White;
            //label6.ForeColor = Color.White;
            label7.ForeColor = Color.White;
            //label9.ForeColor = Color.White;

            //label11.ForeColor = Color.White;


            //Dark Theme Radio Button
            rdbCustom.ForeColor = Color.White;
            rdbDefault.ForeColor = Color.White;

            //Dark Theme Button
            //btnReplace.BackColor = Color.FromArgb(64, 64, 64); btnReplace.ForeColor = Color.White;
            //btnReset.BackColor = Color.FromArgb(64, 64, 64); btnReset.ForeColor = Color.White;
            btnBackup.BackColor = Color.FromArgb(64, 64, 64); btnBackup.ForeColor = Color.White;
            btnRestore.BackColor = Color.FromArgb(64, 64, 64); btnRestore.ForeColor = Color.White;
            //btnFolder.BackColor = Color.FromArgb(64, 64, 64); btnFolder.ForeColor = Color.White;
            //btnOptimize.BackColor = Color.FromArgb(64, 64, 64); btnOptimize.ForeColor = Color.White;
            btnListData.BackColor = Color.FromArgb(64, 64, 64); btnListData.ForeColor = Color.White;
            btnUpdateScript.BackColor = Color.FromArgb(64, 64, 64); btnUpdateScript.ForeColor = Color.White;
            //btnCheckDBName.BackColor = Color.FromArgb(64, 64, 64); btnCheckDBName.ForeColor = Color.White;

            //Dark Theme RichText Box
            rtb1.BackColor = Color.Black; rtb1.ForeColor = Color.White;
        }

        public void DBConnect(string strDB)
        {
            //string str = "";
            SqlConnection myConn = new SqlConnection(@"Data source=.\SQLEXPRESS;Initial Catalog=master;integrated security = SSPI");
            //strDB = str;
            

            SqlCommand myCommand = new SqlCommand(strDB, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                rtb1.Text = "DataBase is Created Successfully";
                //MessageBox.Show("DataBase is Created Successfully", "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                rtb1.Text = ex.ToString();
                //MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }
        #region LIST SERVICE LOAD
        public void ListDatabase()
        {
            ServiceController sc = new ServiceController("MSSQL$SQLEXPRESS");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                string ListDB = "";
                string _List = "";
                string connetionString = null;
                SqlConnection connection;
                SqlCommand command;
                string sql = null;
                SqlDataReader dataReader;
                connetionString = @"Data source=.\SQLEXPRESS;Initial Catalog=master;integrated security = SSPI";
                sql = @"SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";
                connection = new SqlConnection(connetionString);
                try
                {
                    connection.Open();
                    command = new SqlCommand(sql, connection);
                    dataReader = command.ExecuteReader();
                    //rtb1.Text = "";

                    while (dataReader.Read())
                    {
                        //MessageBox.Show(dataReader.GetValue(0) + " - " + dataReader.GetValue(1) + " - " + dataReader.GetValue(2));
                        //rtb1.Text = (dataReader.GetValue(0) + " \n ");

                        _List = dataReader.GetString(0);
                        ListDB = _List + "\n";
                        cbbDatabase.Items.Add(_List);

                    }

                    //rtb1.Text = ListDB;
                    //cbbDatabase.Items.Add(ListDB);

                    dataReader.Close();
                    command.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    rtb1.Text = "Can not open connection ! ";
                }
            }
            else
            {
                rtb1.Text = "Database list can not load because SQL Server service is not running.";
            }

            
        }

        #endregion

        /// <summary>
        /// This utility function displays all the IP (v4, not v6) addresses of the local computer.
        /// </summary>
        public static void DisplayIPAddresses()
        {
            StringBuilder sb = new StringBuilder();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network   ndjdk
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addressesdf
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1)
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }

            MessageBox.Show(sb.ToString());
        }



        private void getIPv4()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("10.0.1.20", 1337); // doesnt matter what it connects to
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    //Console.WriteLine(endPoint.Address.ToString()); //ipv4
                    //MessageBox.Show(endPoint.Address.ToString());
                    //txtIP2.Text = endPoint.Address.ToString();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed"); // If no connection is found
            }
        }

        //private void btnReplace_Click(object sender, EventArgs e)
        //{
        //    string file = "C:\\inetpub\\wwwroot\\BLogicService\\Web.Config";
        //    //string textrp = "=" + txtDatabase.Text + ";";
        //    if (File.Exists(file))
        //    {
        //        //string filedata = File.ReadAllText("C:\\AMD\\1.Config");
        //        //string textreplace = txtDatabase.Text;
        //        //StringBuilder b = new StringBuilder(filedata);
        //        //MessageBox.Show("Sẽ bắt đầu đổi tên Database "+textreplace);
        //        //string Final = b.Replace("=BlogicPOS7", textreplace);
        //        //File.WriteAllText("C:\\AMD\\Web2.Config", Final);
        //        //MessageBox.Show("Done!");
        //        int i = 0;
        //        //int j = 0;
        //        //int k = 0;
        //        string textcontent = "";

        //        textcontent = File.ReadAllText(file);
        //        //    string sTextChange = "=BLogicPOS7;";
        //        //    if (textcontent.IndexOf(sTextChange) >= 0)
        //        //    {
        //        //        textcontent = textcontent.Replace(sTextChange, textrp);
        //        //        File.WriteAllText("C:\\inetpub\\wwwroot\\BLogicService\\Web.Config", textcontent);
        //        //        MessageBox.Show("Done!");
        //        //    }
        //        //    else
        //        //    {
        //        //        MessageBox.Show("Không tìm thấy!");
        //        //        string data = getBetween(textcontent, "SQLEXPRESS;Initial Catalog=", ";integrated");
        //        //        MessageBox.Show(data);
        //        //        return;
        //        //    }
        //        //}
        //        //else MessageBox.Show("Không tìm thấy file!");



        //        //string textrp = "=" + txtDatabase.Text + ";";
        //        string textrp = txtDatabase.Text;
        //        //=blogicpos7;
        //        string[] lines = File.ReadAllLines(file);
        //        int flag = 0;
        //        if (textrp != "")
        //        {

        //            for (i = 0; i <= lines.Length - 1; i++)
        //            {
        //                //if (lines[i].contains("=blogicpos7;"))
        //                if (lines[31] == "BlogicPOS7")
        //                {
        //                    textcontent = textcontent.Replace("=BlogicPOS7;", textrp);
        //                    File.WriteAllText("C:\\inetpub\\wwwroot\\BLogicService\\Web.Config", textcontent);
        //                    //MessageBox.Show("Done!");
        //                    rtb1.Text = "Database " + textrp + " was renamed"; ;
        //                    flag++;
        //                }
        //                else
        //                {
        //                    string data = getBetween(lines[31], "SQLEXPRESS;Initial Catalog=", ";integrated");
        //                    if (data == "")
        //                    {
        //                        //data = "N/A";
        //                        MessageBox.Show("Database đang để rỗng!");
        //                    }

        //                    //MessageBox.Show("Database name " + data + " is using.");
        //                    lblStatus.Text = "Database name " + data + " is using.";
        //                    textcontent = textcontent.Replace(data, textrp);

        //                    File.WriteAllText("C:\\inetpub\\wwwroot\\BLogicService\\Web.Config", textcontent);
        //                    //MessageBox.Show("Đã thay đổi tên Database!");
        //                    rtb1.Text = "Database " + textrp + " was renamed";
        //                    return;
        //                }
        //            }
        //            if (flag == 0)
        //            {
        //                //MessageBox.Show("Không tìm thấy!");
        //                rtb1.Text = "Không tìm thấy";
        //            }
        //            else rtb1.Text = "Không tìm thấy file";

        //        }
        //        else
        //        {
        //            rtb1.ForeColor = Color.Red;
        //            rtb1.Text = "Please enter the database name!";
        //            //txtDatabase.BackColor = Color.FromArgb(65, 131, 215);
        //            //txtDatabase.Focus();
        //        }



        //    }
        //}

        //private void txtDatabase_KeyDown(object sender, KeyEventArgs e)
        //{
        //    txtDatabase.BackColor = Color.White;
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        // When the user presses both the 'Alt' key and 'F' key,
        //        // KeyPreview is set to False, and a message appears.
        //        // This message is only displayed when KeyPreview is set to False.
        //        btnReplace_Click(sender, e);

        //    }
        //}

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                //MessageBox.Show("Test test");
                //rtb1.Text = "";
                string currentpath = Environment.CurrentDirectory.ToString();
                if (rdbDefault.Checked == true)
                {

                    try
                    {



                        string DATESTAMP = DateTime.Now.ToString("yyyyMMdd_hhmmss");
                        string BACKUPFILENAME = "_" + DATESTAMP + ".bak";
                        FolderBackup = currentpath+@"\DBBackup_" + DATESTAMP;
                        ThemeBackup = FolderBackup + @"\Theme";
                        Directory.CreateDirectory(FolderBackup);
                        Directory.CreateDirectory(ThemeBackup);

                        string POS7Cmd = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"BACKUP DATABASE BLOGICPOS7 TO DISK = '" + FolderBackup + "\\BLOGICPOS7" + BACKUPFILENAME + "\' WITH INIT , NOUNLOAD , NAME = \'BLOGICPOS7 backup\', NOSKIP , STATS = 10, NOFORMAT\"";
                        string MERCHANTCmd = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"BACKUP DATABASE MERCHANT TO DISK = '" + FolderBackup + "\\MERCHANT" + BACKUPFILENAME + "\' WITH INIT , NOUNLOAD , NAME = \'MERCHANT backup\', NOSKIP , STATS = 10, NOFORMAT\"";
                        string GIFTSERVERCmd = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"BACKUP DATABASE GIFTSERVER TO DISK = '" + FolderBackup + "\\GIFTSERVER" + BACKUPFILENAME + "\' WITH INIT , NOUNLOAD , NAME = \'GIFTSERVER backup\', NOSKIP , STATS = 10, NOFORMAT\"";
                        string GIFTCLINETCmd = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"BACKUP DATABASE GIFTCLIENT TO DISK = '" + FolderBackup + "\\GIFTCLIENT" + BACKUPFILENAME + "\' WITH INIT , NOUNLOAD , NAME = \'GIFTCLIENT backup\', NOSKIP , STATS = 10, NOFORMAT\"";
                        ExecuteCommand2(POS7Cmd);
                        //lblStatus.Text = POS7Cmd;
                        ExecuteCommand2(MERCHANTCmd);
                        ExecuteCommand2(GIFTSERVERCmd);
                        ExecuteCommand2(GIFTCLINETCmd);

                        //Backup Theme form POS
                        string sourcePath = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\Theme";
                        string BKFolder = "/C xcopy \""+sourcePath+"\" \""+ThemeBackup+"\" /O /X /E /H /K";
                        ExecuteCommand2(BKFolder);

                        ////string fileName = "Dict.xml";

                        
                        //string targetPath = FolderBackup;

                        //// Use Path class to manipulate file and directory paths.
                        //string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        //string destFile = System.IO.Path.Combine(targetPath, fileName);

                        //// To copy a folder's contents to a new location:
                        //// Create a new target folder, if necessary.
                        //if (!System.IO.Directory.Exists(targetPath))
                        //{
                        //    System.IO.Directory.CreateDirectory(targetPath+@"\Theme");
                        //}

                        //// To copy a file to another location and 
                        //// overwrite the destination file if it already exists.
                        ////System.IO.File.Copy(sourceFile, destFile, true);

                        //if (System.IO.Directory.Exists(sourcePath))
                        //{
                        //    string[] files = System.IO.Directory.GetFiles(sourcePath);

                        //    // Copy the files and overwrite destination files if they already exist.
                        //    foreach (string s in files)
                        //    {
                        //        // Use static Path methods to extract only the file name from the path.
                        //        fileName = System.IO.Path.GetFileName(s);
                        //        destFile = System.IO.Path.Combine(targetPath, fileName);
                        //        System.IO.File.Copy(s, destFile, true);
                        //    }
                        //}
                        //else
                        //{
                        //    Console.WriteLine("Source path does not exist!");
                        //}


                    }
                    catch (Exception ex)
                    {
                        throw new Exception("An error has occurred. " + ex.Message);
                    }
                    finally
                    {
                        Form1.Flash(this);
                        if (MessageBox.Show(@"Backup successfully created. Do you want open backup folder?", "Backup Database", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Process.Start("explorer.exe", FolderBackup);
                        }
                    }

                }
                else if (rdbCustom.Checked == true)
                {
                    try
                        {

                            string DBBK = cbbDatabase.Text;//Name to Backup File
                            string DATESTAMP2 = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                            string BACKUPFILENAME2 = "_" + DATESTAMP2 + ".bak";
                        FolderBackup = currentpath + @"\DBBackup_" + DATESTAMP2;
                        Directory.CreateDirectory(FolderBackup);

                            string bkcmd = "/C sqlcmd -E -S .\\SQLExpress -d master -Q \"BACKUP DATABASE " + DBBK + " TO DISK = \'" + FolderBackup + @"\" + DBBK + "" + BACKUPFILENAME2 + "\' WITH INIT , NOUNLOAD , NAME = \'" + DBBK + " backup\', NOSKIP , STATS = 10, NOFORMAT\"";
                            ExecuteCommand2(bkcmd);
                            string path = Environment.CurrentDirectory.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("An error has occurred " + ex.Message);
                        }
                        finally
                        {

                            string path = Environment.CurrentDirectory.ToString();
                            if (MessageBox.Show(@"Backup successfully created. Do you want open backup folder?", "Backup Database.", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Process.Start("explorer.exe", @"" + FolderBackup + "");
                            }
                        }
                    }

                
            }
            catch
            {

            }
            backgroundWorker1.ReportProgress(100);

        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
            
            
            if (rdbCustom.Checked == true)
            {
                BackupDB = 1;
                BackupDB_act = "Backup database " + cbbDatabase.Text;
            }
            if(rdbDefault.Checked == true)
            {
                BackupDB = 2;
                BackupDB_act = "Backup All database";
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            if (rdbDefault.Checked == true)
            {
                Start_AniText();
                backgroundWorker1.RunWorkerAsync();
            }
            else if (rdbCustom.Checked == true)
            {
                if (cbbDatabase.Text == "")
                {

                    rtb1.Text = "Please enter or select the DATA BACKUP NAME!";
                    cbbDatabase.Focus();
                }

                //if (backgroundWorker1.IsBusy)
                //{
                //    backgroundWorker1.CancelAsync();

                //}
                else
                {
                    Start_AniText();
                    rtb1.Text = "";
                    backgroundWorker1.RunWorkerAsync();

                }
            }
            

        }

        //private void btnOptimize_Click(object sender, EventArgs e)
        //{
        //    if (txtDBO.Text == "")
        //    {
        //        rtb1.Text = "Please enter the database name!";
        //        txtDBO.Focus();
        //        txtDBO.SelectAll();

        //    }
        //    else
        //    {
        //        string dbNAMEO = txtDBO.Text;
        //        string strConnect = @"Data source=.\SQLEXPRESS;Initial Catalog=" + dbNAMEO + ";integrated security = SSPI";
        //        SqlConnection _conn = new SqlConnection(strConnect);
        //        try
        //        {


        //            _conn.Open();
        //            //string sqlUpdate = "ALTER DATABASE "+dbNAMEO
        //            //                    +" SET RECOVERY SIMPLE "
        //            //                    +" DBCC SHRINKFILE (" + dbNAMEO + "_log, 1)"
        //            //                    +" ALTER DATABASE " + dbNAMEO  
        //            //                    +" SET RECOVERY FULL";

        //            string sqlUpdate = @"ALTER DATABASE " + dbNAMEO + @"
        //                                SET RECOVERY SIMPLE
        //                                DBCC SHRINKFILE (" + dbNAMEO + @"_log, 1);
        //                                ALTER DATABASE " + dbNAMEO + @"
        //                                SET RECOVERY FULL";
        //            //string sqlUpdate = "SELECT * FROM Items";
        //            SqlCommand cmd = new SqlCommand(sqlUpdate, _conn);
        //            //cmd.Connection = _conn;
        //            cmd.ExecuteNonQuery();
        //            rtb1.Text = "Database BlogicPOS7 đã được optimize!";

        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Lỗi khi thực hiện câu lệnh sql " + ex.Message);
        //        }
        //        finally
        //        {
        //            _conn.Close();
        //        }
        //    }


        //}

        //private void btnFolder_Click(object sender, EventArgs e)
        //{
        //    string SQLPath = @"C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\";
        //    FileAttributes attributes = File.GetAttributes(SQLPath);

        //    switch (attributes)
        //    {
        //        case FileAttributes.Directory:
        //            if (Directory.Exists(SQLPath))
        //            {
        //                //Console.WriteLine("This directory exists.");
        //                Process.Start("explorer.exe", @"C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\");
        //            }

        //            else
        //                MessageBox.Show("This directory does not exist.");
        //            break;
        //        default:
        //            if (File.Exists(SQLPath))
        //                Console.WriteLine("This file exists.");
        //            else
        //                Console.WriteLine("This file does not exist.");
        //            break;
        //    }

        //}

        //private void txtDBO_MouseClick(object sender, MouseEventArgs e)
        //{
        //    txtDBO.SelectAll();
        //}

        //private void txtDBO_MouseDown(object sender, MouseEventArgs e)
        //{
        //    txtDBO.Focus();
        //}

        private void btnReset_Click(object sender, EventArgs e)
        {
            //Vid_Database frm2 = new Vid_Database();
            //frm2.Show();
            //this.Hide();
            //txtDBBK.Text = "";
            //txtReDB.BackColor = Color.White; //txtReDB.Text = "";
            rtb1.BackColor = Color.FromArgb(250, 250, 203);
            rtb1.ForeColor = Color.Black;
            rtb1.Font = new Font(rtb1.Font, FontStyle.Bold);
            //tabControl1.Hide();
            //rtb1.SizeChanged.
            rtb1.Text = @"_____________DATABASE_____________

BACKUP DB:
1. DEFAULT radio checked: backup all database like BLOGICPOS7, MERCHANT, GIFTSERVER, GIFCLIENT
2. CUSTOM radio checked: Type name at DATA BACKUP NAME and hit the BACKUP button.

RESTORE DB:
1. Hit the LIST DATA to show list datbase.
1. Enter name at RESTORE textbox.
2. Hit the RESTORE button to restore database.
3. Choose the backup file and hit the OK button to restore.

UPDATE SCRIPT:
1. Enter name at RESTORE textbox.
2. Hit the UPDATE button to restore database.
3. Choose the updatesript file and hit the OK button to update sript.


_____________PRINTER_____________

The PRINTER button to shortcut to open Printer windows.
The PAUSE button to Pause the default printer.
The RESUME button to Resume the default printer.
The CLEAR button to Clear all document queue in the default printer.


_____________NETWORK_____________

The LIST IP button to show all ip in network.
IP ADDRESS to show this pc's ip.

_____________SHORTCUT_____________

The CPANEL button to open Uninstall Program windows.
The POS button to open POS program.
The DASHBOARD button to open POS DASHBOARD program.
The EXIT button to close this tool.";

                rtb1.ForeColor = Color.FromArgb(1, 50, 67);
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListDatabase();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            //Check before close form 03.13.2019
            if(cbbDatabase.Text == "BLogicPOS7")
            {
                Restored_pos = 1;
                Updated_pos = 0;
                Restored_pos_act = "Retored database BlogicPOS7";
                ColorPanel_Error();
            }
            if (cbbDatabase.Text == "Merchant")
            {
                Restored_merchant = 1;
                Updated_merchant = 0;
                Restored_merchant_act = "Retore database Merchant";
                ColorPanel_Error();
            }

            Checknull = cbbDatabase.Text;
            OpenFileDialog bDialog = new OpenFileDialog();
            bDialog.Title = "Choose Your Backup Database File";
            bDialog.Filter = "BAK Files|*.bak";
            bDialog.InitialDirectory = path;

            //Kiem tra txt Database Restore Name da co hay chua

            if (Checknull == "")
            {
                
                rtb1.Text = "Please enter the DATA RESTORE NAME!";

                cbbDatabase.Focus();
            }
            else
            {
                if (bDialog.ShowDialog() == DialogResult.OK)
                {
                    fileDBName = bDialog.SafeFileName;
                    pathDBName = bDialog.FileName;
                }
                else return;
                
                DatabaseFileName = cbbDatabase.Text;
                Start_AniText();
                rtb1.Text = "";
                bgw_Restore.RunWorkerAsync();
                
            }
            
        }

        private void rdbCustom_CheckedChanged(object sender, EventArgs e)
        {
            //txtDBBK.Enabled = true;
            cbbDatabase.Enabled = true;
        }

        private void rdbDefault_CheckedChanged(object sender, EventArgs e)
        {
            //txtDBBK.Enabled = false; txtDBBK.Text = "";
            cbbDatabase.Enabled = false;
            
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnPause_Click(object sender, EventArgs e)
        {

            Printer();
            if (PrintQ.IsPaused == false)
            {
                PrintQ.Pause();
                CheckPrinterStatus();
            }
            else
            {
                CheckPrinterStatus();
            }

            
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            Printer();
            if (PrintQ.IsPaused)
            {
                PrintQ.Resume();
                rtb1.Text = DFPrinter + " is resume";
            }
            else
            {
                rtb1.Text = DFPrinter + " is running.";
            }
            CheckPrinterStatus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Printer();
            PrintQ.Purge();
            rtb1.Text = DFPrinter + " is clear";
            CheckPrinterStatus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //DisplayIPAddresses();
            getIPv4();



        }

        private void btnSQLStart_Click(object sender, EventArgs e)
        {
            string startsql = "/C net start MSSQL$SQLEXPRESS";
            ServiceController sc = new ServiceController("MSSQL$SQLEXPRESS");
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                ExecuteCommand(startsql);
            }
            else
                MessageBox.Show("Running");
            //switch (sc.Status)
            //{
            //    case ServiceControllerStatus.Running:
            //        MessageBox.Show("MSSQL Service is Running");
            //        break;
            //    case ServiceControllerStatus.Stopped:
            //        ExecuteCommand(startsql);
            //        break;
            //    case ServiceControllerStatus.Paused:
            //        MessageBox.Show("MSSQL Service is Paused");
            //        break;
            //    case ServiceControllerStatus.StopPending:
            //        MessageBox.Show("MSSQL Service is Stopping");
            //        break;
            //    case ServiceControllerStatus.StartPending:
            //        MessageBox.Show("MSSQL Service is Starting");
            //        break;
            //    default:
            //        MessageBox.Show("Status Changing");
            //        break;
            //}


        }

        private void btnSQLStop_Click(object sender, EventArgs e)
        {
            string stopsql = "/C net stop MSSQL$SQLEXPRESS";

            ServiceController sc = new ServiceController("MSSQL$SQLEXPRESS");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                ExecuteCommand(stopsql);
            }
            else
                MessageBox.Show("Stopped");

            //switch (sc.Status)
            //{
            //    case ServiceControllerStatus.Running:
            //        ExecuteCommand(stopsql);
            //        break;
            //    case ServiceControllerStatus.Stopped:
            //        MessageBox.Show("MSSQL Service is Stopped");
            //        break;
            //    case ServiceControllerStatus.Paused:
            //        MessageBox.Show("MSSQL Service is Paused");
            //        break;
            //    case ServiceControllerStatus.StopPending:
            //        MessageBox.Show("MSSQL Service is Stopping");
            //        break;
            //    case ServiceControllerStatus.StartPending:
            //        MessageBox.Show("MSSQL Service is Starting");
            //        break;
            //    default:
            //        MessageBox.Show("Status Changing");
            //        break;
            //}

        }

        private void btnUpdateScript_Click(object sender, EventArgs e)
        {
            //Check before close form
            if(cbbDatabase.Text == "BLogicPOS7")
            {
                Restored_pos = 1;
                Updated_pos = 1;
                Updated_pos_act = "Updated database BlogicPOS7";
                if (Restored_merchant == 1 && Updated_merchant == 0) //Restore merchant, but no update merchant
                {
                    ColorPanel_Error();
                }
                else
                    ColorPanel_Fine();

            }
            if(cbbDatabase.Text == "Merchant")
            {
                Restored_merchant = 1;
                Updated_merchant = 1;
                Updated_merchant_act = "Updated database Merchant";
                if (Restored_pos == 1 && Updated_pos == 0) //Restore pos, but no update pos
                {
                    ColorPanel_Error();
                }
                else
                    ColorPanel_Fine();
            }

            OpenFileDialog bDialog2 = new OpenFileDialog();
            bDialog2.Title = "Choose your SQL file";
            bDialog2.Filter = "SQL Files|*.sql";
            bDialog2.InitialDirectory = path;
            //Kiem tra txt Database Restore Name da co hay chua

            if (cbbDatabase.Text == "")
            {
                
                rtb1.Text = "Please enter the DATA RESTORE NAME!";
                
                cbbDatabase.Focus();
            }
            else
            {
                if (bDialog2.ShowDialog() == DialogResult.OK)
                {
                    fileDBName = bDialog2.SafeFileName;
                    pathDBName = bDialog2.FileName;
                }
                else return;
                DatabaseFileName = cbbDatabase.Text;
                
                rtb1.Text = "";
                Start_AniText();
                bgw_UpdateDB.RunWorkerAsync();
                

                

            }
        }

        

        //private void btnCheckDBName_Click(object sender, EventArgs e)
        //{
        //    string file = "C:\\inetpub\\wwwroot\\BLogicService\\Web.Config";
        //    if (File.Exists(file))
        //    {
        //        int i = 0;
        //        string textcontent = "";
        //        textcontent = File.ReadAllText(file);
        //        string textrp = txtDatabase.Text;
        //        string[] lines = File.ReadAllLines(file);
        //        //int flag = 0;
        //        for (i = 0; i <= lines.Length - 1; i++)
        //        {
        //            string data = getBetween(lines[31], "SQLEXPRESS;Initial Catalog=", ";integrated");
        //            rtb1.Text = "Database name using is: " + data;
        //        }

        //    }
        //}


        private void txtReDB_MouseDown(object sender, MouseEventArgs e)
        {
            //txtReDB.SelectAll();

        }

        private void txtReDB_MouseClick(object sender, MouseEventArgs e)
        {
            //txtReDB.Focus();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void btnPOSDB_Click(object sender, EventArgs e)
        {
            RunPOSDashboard();
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            RunPOS();
        }

        

        private void button1_Click_1(object sender, EventArgs e)
        {
            string cmdARP = "/C arp -a";
            //txtListIP.Text = "";
            ExecuteCommand(cmdARP);
            //readlog = File.ReadAllText(@path + @"\log.txt");
            //rtb1.Text = readlog;
        }

        private void rtb1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {   //click event
                //MessageBox.Show("you got it!");
                ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
                MenuItem menuItem = new MenuItem("Cut");
                menuItem.Click += new EventHandler(CutAction);
                contextMenu.MenuItems.Add(menuItem);
                menuItem = new MenuItem("Copy");
                menuItem.Click += new EventHandler(CopyAction);
                contextMenu.MenuItems.Add(menuItem);
                menuItem = new MenuItem("Paste");
                menuItem.Click += new EventHandler(PasteAction);
                contextMenu.MenuItems.Add(menuItem);

                rtb1.ContextMenu = contextMenu;
            }
        }
        void CutAction(object sender, EventArgs e)
        {
            rtb1.Cut();
        }

        void CopyAction(object sender, EventArgs e)
        {
            Clipboard.SetText(rtb1.SelectedText);
        }

        void PasteAction(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                rtb1.Text
                    += Clipboard.GetText(TextDataFormat.Text).ToString();
            }
        }

        //private void label10_Click(object sender, EventArgs e)
        //{
        //    txtReDB.Text = "BlogicPOS7";
        //    if (rdbCustom.Checked == true)
        //    {
        //        txtDBBK.Text = "BlogicPOS7";
        //    }

        //}

        //private void label12_Click(object sender, EventArgs e)
        //{
        //    txtReDB.Text = "Merchant";
        //    if (rdbCustom.Checked == true)
        //    {
        //        txtDBBK.Text = "Merchant";
        //    }

        //}

        //private void label13_Click(object sender, EventArgs e)
        //{
        //    txtReDB.Text = "GiftServer";
        //    if (rdbCustom.Checked == true)
        //        txtDBBK.Text = "GiftServer";
        //}

        //private void label14_Click(object sender, EventArgs e)
        //{
        //    txtReDB.Text = "GiftClient";
        //    if (rdbCustom.Checked == true)
        //        txtDBBK.Text = "GiftClient";
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("control", "/name Microsoft.ProgramsAndFeatures");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("control", "/name Microsoft.DevicesAndPrinters");
            System.Diagnostics.Process.Start("ms-settings:printers");
        }

        private void rdbDefault_Click(object sender, EventArgs e)
        {
            
            //txtDBBK.Text = "";
        }

        private void btnBuild102_Click(object sender, EventArgs e)
        {
            string currentpath = @"\\192.168.0.102\share\build";
            Process.Start("explorer.exe", currentpath);
        }

        private void btnCreateNewDB_Click(object sender, EventArgs e)
        {
            if (cbbDatabase.Text == "")
            {
                
                rtb1.Text = "Please type New Database Name";
                cbbDatabase.Focus();
            }
            else
            {
                string NewDBName = cbbDatabase.Text;
                string NewDBstr = "CREATE DATABASE " + NewDBName.Replace(" ", "_") + "";
                DBConnect(NewDBstr);
                Form1.Flash(this);
                MessageBox.Show("Database " + NewDBName + " successfully created.");
                cbbDatabase.Text = "";
                cbbDatabase.Items.Clear();
                ListDatabase();
            }
            

        }

        private void rdbIIS_CheckedChanged(object sender, EventArgs e)
        {
            serviceshortcut_str = rdbIIS.Text;
        }

        private void rdbSQL_CheckedChanged(object sender, EventArgs e)
        {
            serviceshortcut_str = rdbSQL.Text;
        }

        private void btnSTOP_Click(object sender, EventArgs e)
        {
            if (serviceshortcut_str == "")
            {
                rtb1.Text = "Please choose Service to stop!";
            }
            else
            {
                try
                {
                    Start_AniText();
                    rtb1.Text = "";
                    bgw_Stop_Service.RunWorkerAsync();
                }
                catch
                {

                }
                finally
                {
                    //readlog = File.ReadAllText(@path + @"\log.txt");
                    //rtb1.Text = readlog;
                }
                
                
            }


        }

        private void btnSTART_Click(object sender, EventArgs e)
        {
            if (serviceshortcut_str == "")
            {
                rtb1.Text = "Please choose Service to start!";
            }
            else
            {
                try
                {
                    Start_AniText();
                    rtb1.Text = "";
                    bgw_Start_Service.RunWorkerAsync();
                }
                catch
                {

                }
                finally
                {
                    //readlog = File.ReadAllText(@path + @"\log.txt");
                    //rtb1.Text = readlog;
                }
            }
        }

        private void btnRESTART_Click(object sender, EventArgs e)
        {
            if(serviceshortcut_str=="")
            {
                rtb1.Text = "Please choose Service to restart!";
            }
            else
            {
                try
                {
                    Start_AniText();
                    rtb1.Text = "";
                    bgw_IIS_Service.RunWorkerAsync();
                }
                catch
                {

                }
                finally
                {
                    //readlog = File.ReadAllText(@path + @"\log.txt");
                    //rtb1.Text = readlog;
                    
                }
            }
            
        }

        private void pbReLoadDB_Click(object sender, EventArgs e)
        {
            cbbDatabase.Items.Clear();
            ListDatabase();
            cbbDatabase.Text = "BLogicPOS7";
            MessageBox.Show("List database was loaded.");
        }

        private void pbDeleteDB_Click(object sender, EventArgs e)
        {
            if (cbbDatabase.Text == "")
            {
                rtb1.Text = "Please select Database Name";
                cbbDatabase.Focus();
            }
            else
            {
                string DelDBName = cbbDatabase.Text;
                if (MessageBox.Show(@"Are you sure you want to delete this database? - " + DelDBName, "Detele Database", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string NewDBstr = "DROP DATABASE " + DelDBName.Replace(" ", "_") + "";
                    DBConnect(NewDBstr);
                    Form1.Flash(this);
                    MessageBox.Show("Database " + DelDBName + " successfully deleted.");
                    //rtb1.Text = "Database " + DelDBName + " successfully deleted.";
                    cbbDatabase.Text = "";
                    cbbDatabase.Items.Clear();
                    ListDatabase();
                }


            }
        }

        private void pbCreateNewDB_MouseHover(object sender, EventArgs e)
        {
            lblSTT.Text = "Create new database";
        }

        private void pbCreateNewDB_MouseLeave(object sender, EventArgs e)
        {
            lblSTT.Text = "";
        }

        public delegate void ThreadStart();
        private void a()
        {
            //loadingPictureBox.Show();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //Thread thread_Retore = new Thread(new ThreadStart(a));
            RestoreDB();
        }

        private void pbDeleteDB_MouseHover(object sender, EventArgs e)
        {
            lblSTT.Text = "Detele database";
        }

        private void pbCheckStatusSercvice_Click(object sender, EventArgs e)
        {
            IISServiceStatus();
            SQLServiceStatus();
        }

        private void bgw_Restore_DoWork(object sender, DoWorkEventArgs e)
        {
            Restore();
            
        }

        private void bgw_Restore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
        }

        private void bgw_UpdateDB_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateDB();
        }

        private void bgw_UpdateDB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
            
        }

        private void bgw_IIS_Service_DoWork(object sender, DoWorkEventArgs e)
        {
            Restart_Service();
        }

        private void bgw_IIS_Service_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
        }

        private void bgw_Stop_Service_DoWork(object sender, DoWorkEventArgs e)
        {
            Stop_Service();
        }

        

        private void bgw_Stop_Service_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //pbServiceStop.Enabled = false;
            Stop_AniText();
        }

        private void bgw_Start_Service_DoWork(object sender, DoWorkEventArgs e)
        {
            Start_Serice();

        }

        private void Start_Serice()
        {
            switch (serviceshortcut_str)
            {
                
                case "IIS Service":
                    service_cmd = "/C iisreset /start";
                    try
                    {
                        
                        ExecuteCommand2(service_cmd);
                        IISServiceStatus();
                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {
                        //Form1.Flash(this);
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("IIS Service is processed.");
                    }
                    break;
                case "SQL Service":
                    service_cmd = "/C net start MSSQL$SQLEXPRESS";
                    try
                    {
                       
                        ExecuteCommand2(service_cmd);
                        SQLServiceStatus();
                    }
                    catch (Exception objException)
                    {

                    }
                    finally
                    {
                        //Form1.Flash(this);
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            Form1.Flash(this);
                        });
                        MessageBox.Show("SQL Service is processed.");
                    }
                    break;
            }
        }

        private void bgw_Start_Service_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
        }

        private void bgw_CMD_DoWork(object sender, DoWorkEventArgs e)
        {
            ///C sqlcmd -S .\\SQLEXPRESS -d BLogicPOS7 -q \"UPDATE dbo.SaleReceiptDetails SET TPTime = GETDATE() WHERE TPTime IS NULL\"
            //string cmd_str = "/C sqlcmd -S .\\SQLEXPRESS -q \"" + txtCMD.Text + "\"";
            //ExecuteCommand2(cmd_str);
        }

        private void bgw_CMD_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop_AniText();
        }



        private void button5_Click(object sender, EventArgs e)
        {
            string subKey = @"SOFTWARE\WOW6432Node\BLogic Systems\BLogic PointOfSale";
            string str = ReadSubKeyValue(subKey, "URL");
            txtURL.Text = str;
        }

        private void pbDeleteDB_MouseLeave(object sender, EventArgs e)
        {
            lblSTT.Text = "";
        }

        private void pbReLoadDB_MouseHover(object sender, EventArgs e)
        {
            lblSTT.Text = "Reload list database";
        }

        private void pbReLoadDB_MouseLeave(object sender, EventArgs e)
        {
            lblSTT.Text = "";
        }

        private void pbAddressEdit_Click(object sender, EventArgs e)
        {
            txtPOSConfig.ReadOnly = false;
            AddressIP = getBetween(AddressGiftIP, "endpoint address=\"http://", ":8200/BLogicGiftService");
            txtPOSConfig.Text = AddressIP;
            txtPOSConfig.Focus();
            txtPOSConfig.SelectAll();
            pbAddressSave.Visible = true;
        }

        private void btn_SAVE_Click(object sender, EventArgs e)
        {
            CreateReg();
            ReadKey();

        }

        public void Start_AniText()
        {
            lblProcessing.Visible = true;
            lblprocessing_1.Visible = true;
            processing_text = lblProcessing.Text;
            len = processing_text.Length;
            lblProcessing.Text = "";
            timer_text_processing.Start();
        }

        public void Stop_AniText()
        {
            lblProcessing.Visible = false;
            lblprocessing_1.Visible = false;
            timer_text_processing.Stop();
        }

        

        private void timer_text_processing_Tick(object sender, EventArgs e)
        {
            counter++;

            if (counter > len)
            {
                counter = 0;
                lblProcessing.Text = "";
            }
            else
            {
                lblProcessing.Text = processing_text.Substring(0, counter);
            }
        }

        

        private void pbAddressSave_Click(object sender, EventArgs e)
        {
            //string POSConfigfile = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";
            //string GiftConfigfile = @"C:\Program Files (x86)\BLogic Systems\BLogicGiftCard\GiftGUI\GiftClient\BLogicGift.exe.config";

            //string newline = AddressGiftIP;
            //newline = newline.Replace(AddressIP, txtPOSConfig.Text);
            //int linePOS = 248;
            //int lineGift = 107;
            //lineChanger(newline, POSConfigfile, linePOS);//Update POS Config File
            //lineChanger(newline, GiftConfigfile, lineGift);//Update Gift Config File

            //if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            //{
            //    Console.WriteLine("64bit");
            //    pathPOS = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.EXE";

            //}
            //else
            //{
            //    Console.WriteLine("32bit");
            //    pathPOS = @"C:\Program Files\BLogic Systems\BLogic PointOfSale\BLOGICPOS.EXE";
            //}

            rtb1.Text = "Changed.";
            pbAddressSave.Visible = false;
            CheckPOSConfig();

        }

        

        private void txtPOSConfig_MouseClick(object sender, MouseEventArgs e)
        {
            
            txtPOSConfig.ReadOnly = false;
            AddressIP = getBetween(AddressGiftIP, "endpoint address=\"http://", ":8200/BLogicGiftService");
            txtPOSConfig.Text = AddressIP;
            txtPOSConfig.Focus();
            txtPOSConfig.SelectAll();
            pbAddressSave.Visible = true;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((Restored_pos == 1 && Updated_pos == 0) || (Restored_merchant == 1 && Updated_merchant == 0) || lblDFPrinter.Text==" PAUSED")
            {
                
                if (MessageBox.Show("Are you sure? Do you want check again?", "Check Again Before Close", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Form2 frm2 = new Form2();
                    frm2.Show();
                }
                
            }
        }

        private void lblDFPrinter_TextChanged(object sender, EventArgs e)
        {
            if (lblDFPrinter.Text == " PAUSED")
            {
                ColorPanel_Error();
            }
            else
            {
                ColorPanel_Fine();
            }
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            rtb1.Text = @"
            

            _____________DATABASE_____________

            BACKUP DB:
            1. DEFAULT radio checked: backup all database like BLOGICPOS7, MERCHANT, GIFTSERVER, GIFCLIENT
            2. CUSTOM radio checked: Type name at DATA BACKUP NAME and hit the BACKUP button.

            RESTORE DB:
            1. Hit the LIST DATA to show list datbase.
            1. Enter name at RESTORE textbox.
            2. Hit the RESTORE button to restore database.
            3. Choose the backup file and hit the OK button to restore.

            UPDATE SCRIPT:
            1. Enter name at RESTORE textbox.
            2. Hit the UPDATE button to restore database.
            3. Choose the updatesript file and hit the OK button to update sript.


            _____________PRINTER_____________

            The PRINTER button to shortcut to open Printer windows.
            The PAUSE button to Pause the default printer.
            The RESUME button to Resume the default printer.
            The CLEAR button to Clear all document queue in the default printer.


            _____________NETWORK_____________

            The LIST IP button to show all ip in network.
            IP ADDRESS to show this pc's ip.

            _____________SHORTCUT_____________

            The CPANEL button to open Uninstall Program windows.
            The POS button to open POS program.
            The DASHBOARD button to open POS DASHBOARD program.
            The EXIT button to close this tool.";
        }

        private void txtURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                pbSavePosAddress_Click(sender, e);
            }
        }

        private void btnNetwork_Refresh_Click(object sender, EventArgs e)
        {
            CheckPOSURL();
            CheckPOSConfig();
            CheckIPLocal();
        }

        private void Re_Printer_Click(object sender, EventArgs e)
        {
            CheckPrinterStatus();
        }

        private void btnChangeDB_Click(object sender, EventArgs e)
        {
            ChangeDB();
        }

        private void btnCMD_Click(object sender, EventArgs e)
        {
            //if (txtCMD.Text == "")
            //{
            //    rtb1.Text = "No commands supplied";
            //    txtCMD.Focus();
            //}
            //else
            //{
            //    loadingPictureBox.Show();
            //    rtb1.Text = "";
            //    loadingPictureBox.Update();
            //    bgw_CMD.RunWorkerAsync();
            //}
            
        }

        private void txtURL_MouseHover(object sender, EventArgs e)
        {
            lblSTT.Text = "Double click to edit!";
        }

        private void txtURL_MouseLeave(object sender, EventArgs e)
        {
            lblSTT.Text = "";
        }

        private void txtPOSConfig_MouseHover(object sender, EventArgs e)
        {
            lblSTT.Text = "Double click to edit!";
        }

        private void txtPOSConfig_MouseLeave_1(object sender, EventArgs e)
        {
            lblSTT.Text = "";
        }

        

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Keys));
        //    Keys POSKey == (Keys)converter.ConvertFromString(HKey_POS_Key);
        //    switch (keyData)
        //    {
        //        //case Keys.Control | Keys.S:
        //        // do something...
        //        //return true;
        //        //case Keys.Control | Keys.Alt | Keys.S:
        //        // do something...
        //        //return true;
        //        //(Keys)Enum.Parse(typeof(Keys), HKey_POS_Key, true);
                
                

        //        case POSKey:
        //            RunPOS();
        //            return true;
        //        //case Keys.F2:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F3:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F4:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F5:
        //        //    btnBackup.PerformClick();
        //        //    return true;
        //        //case Keys.F6:
        //        //    // do something...
        //        //    btnRestore.PerformClick();
        //        //    return true;
        //        //case Keys.F7:
        //        //    // do something...
        //        //    btnUpdateScript.PerformClick();
        //        //    return true;
        //        //case Keys.F8:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F9:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F10:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F11:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        //case Keys.F12:
        //        //    RunPOSDashboard();
        //        //    return true;
        //        default:
        //            return base.ProcessCmdKey(ref msg, keyData);
        //    }
        //}

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            RunSQL();
        }

        public void RunSQL()
        {
            string path_sql = "";
            string sql = "";

            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                Console.WriteLine("64bit");
                sql = @"C:\Program Files (x86)\Microsoft SQL Server\120\Tools\Binn\ManagementStudio\Ssms.exe";
                if (!File.Exists(sql))
                {
                    MessageBox.Show("Can not find POS.");
                    return;
                }
                else
                    path_sql = sql;

            }
            else
            {
                Console.WriteLine("32bit");
                sql = @"C:\Program Files\Microsoft SQL Server\120\Tools\Binn\ManagementStudio\Ssms.exe";
                if (!File.Exists(sql))
                {
                    MessageBox.Show("Can not find POS.");
                    return;
                }
                else
                    path_sql = sql;
            }

            try
            {
                //Create process
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.FileName = path_sql;
                //pProcess.StartInfo.Arguments = command;
                pProcess.StartInfo.UseShellExecute = true;
                pProcess.Start();
            }
            catch (Exception e)
            {
                rtb1.Text = "IOException source: {0}" + e;
            }
        }

        private void txtPOSConfig_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPOSConfig_Leave(object sender, EventArgs e)
        {
            txtPOSConfig.ReadOnly = true;
            CheckPOSConfig();
            pbAddressSave.Visible = false;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            //frm.Show();
            frm.ShowDialog();
        }

        private void txtPOSConfig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string POSConfigfile = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale\BLOGICPOS.exe.config";
                string GiftConfigfile = @"C:\Program Files (x86)\BLogic Systems\BLogicGiftCard\GiftGUI\GiftClient\BLogicGift.exe.config";

                string newline = AddressGiftIP;
                newline = newline.Replace(AddressIP, txtPOSConfig.Text);
                int linePOS = 248;
                int lineGift = 100;
                lineChanger(newline, POSConfigfile, linePOS);//Update POS Config File
                lineChanger(newline, GiftConfigfile, lineGift);//Update Gift Config File
                rtb1.Text = "Changed.";
                pbAddressSave.Visible = false;
                CheckPOSConfig();
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
            
        }

        private void cbbDatabase_Click(object sender, EventArgs e)
        {
            cbbDatabase.DroppedDown = true;
        }

        private void txtURL_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            txtURL.ReadOnly = false;
            URLIP = getBetween(txtURL.Text, "http://", "/BLogicService");
            txtURL.Text = URLIP;
            txtURL.Focus();
            txtURL.SelectAll();
            pbSavePosAddress.Visible = true;
            
        }

        private void pbSavePosAddress_Click(object sender, EventArgs e)
        {
            POSURL = "http://" + txtURL.Text + "/BLogicService/Service.asmx";
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\BLogic Systems\BLogic PointOfSale", true);
            if (myKey != null)
            {
                myKey.SetValue("URL", POSURL, RegistryValueKind.String);
                rtb1.Text = "Done.";
                myKey.Close();
            }
            txtURL.ReadOnly = true;
            CheckPOSURL();
            pbSavePosAddress.Visible = false;
        }

        private void txtURL_Leave(object sender, EventArgs e)
        {
            txtURL.ReadOnly = true;
            CheckPOSURL();
            pbSavePosAddress.Visible = false;
        }

        public void CreateReg()
        {
            HKey_POS = txt_HKey_POS.Text;
            HKey_POSD = txt_HKey_POSD.Text;
            HKey_SQL = txt_HKey_SQL.Text;
            HKey_CPanel = txt_HKey_CPanel.Text;
            HKey_Restore = txt_HKey_Restore.Text;
            HKey_Backup = txt_HKey_Backup.Text;
            HKey_Update = txt_HKey_Update.Text;
            HKey_Printer = txt_HKey_Printer_List.Text;
            HKey_Build = txt_HKey_Build.Text;
            Build_IP = txt_Build_IP.Text;


            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(BLSIKey, true);
            if (myKey == null)
            {
                //MessageBox.Show("Ton tai.");
                myKey = Registry.LocalMachine.CreateSubKey(BLSIKey);

                myKey.SetValue("POS_Hotkey", HKey_POS);
                myKey.SetValue("POS_Dashboard_Hotkey", HKey_POSD);
                myKey.SetValue("SQL_Hotkey", HKey_SQL);
                myKey.SetValue("Program_List_Hotkey", HKey_CPanel);
                myKey.SetValue("Restore_Hotkey", HKey_Restore);
                myKey.SetValue("Backup_Hotkey", HKey_Backup);
                myKey.SetValue("Update_Script_Hotkey", HKey_Update);
                myKey.SetValue("Printer_List_Hotkey", HKey_Printer);
                myKey.SetValue("Build_Server_Hotkey", HKey_Build);
                myKey.SetValue("Build_Server_IP", txt_Build_IP.Text);

                //MessageBox.Show("Done");
                myKey.Close();
            }
            else
            {
                myKey.SetValue("POS_Hotkey", HKey_POS);
                myKey.SetValue("POS_Dashboard_Hotkey", HKey_POSD);
                myKey.SetValue("SQL_Hotkey", HKey_SQL);
                myKey.SetValue("Program_List_Hotkey", HKey_CPanel);
                myKey.SetValue("Restore_Hotkey", HKey_Restore);
                myKey.SetValue("Backup_Hotkey", HKey_Backup);
                myKey.SetValue("Update_Script_Hotkey", HKey_Update);
                myKey.SetValue("Printer_List_Hotkey", HKey_Printer);
                myKey.SetValue("Build_Server_Hotkey", HKey_Build);
                myKey.SetValue("Build_Server_IP", Build_IP);

                //MessageBox.Show("Done");
                this.BeginInvoke((MethodInvoker)delegate
                {
                    Form1.Flash(this);
                });
                MessageBox.Show("Saved.");

                myKey.Close();
            }
        }

        public void ReadKey()
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(BLSIKey, true);
            if (myKey == null)
            {
                CreateReg();
            }
            else
            {
                HKey_POS_Key = ReadSubKeyValue(BLSIKey, "POS_Hotkey");
                HKey_POSD_Key = ReadSubKeyValue(BLSIKey, "POS_Dashboard_Hotkey");
                HKey_SQL_Key = ReadSubKeyValue(BLSIKey, "SQL_Hotkey");
                HKey_CPanel_Key = ReadSubKeyValue(BLSIKey, "Program_List_Hotkey");
                HKey_Restore_Key = ReadSubKeyValue(BLSIKey, "Restore_Hotkey");
                HKey_Backup_Key = ReadSubKeyValue(BLSIKey, "Backup_Hotkey");
                HKey_Update_Key = ReadSubKeyValue(BLSIKey, "Update_Script_Hotkey");
                HKey_Printer_Key = ReadSubKeyValue(BLSIKey, "Printer_List_Hotkey");
                HKey_Build_Key = ReadSubKeyValue(BLSIKey, "Build_Server_Hotkey");
                Build_IP_Key = ReadSubKeyValue(BLSIKey, "Build_Server_IP");

                txt_HKey_POS.Text = HKey_POS_Key;
                txt_HKey_POSD.Text = HKey_POSD_Key;
                txt_HKey_SQL.Text = HKey_SQL_Key;
                txt_HKey_CPanel.Text = HKey_CPanel_Key;
                txt_HKey_Restore.Text = HKey_Restore_Key;
                txt_HKey_Backup.Text = HKey_Backup_Key;
                txt_HKey_Update.Text = HKey_Update_Key;
                txt_HKey_Printer_List.Text = HKey_Printer_Key;
                txt_HKey_Build.Text = HKey_Build_Key;
                txt_Build_IP.Text = Build_IP_Key;
            }

            
        }

        private void lblPOS_Click(object sender, EventArgs e)
        {
            RunPOS();
        }

        private void lblBackOffice_Click(object sender, EventArgs e)
        {
            RunPOSDashboard();
        }

        private void lblSQL_Click(object sender, EventArgs e)
        {
            RunSQL();
        }

        private void lblProgram_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("control", "/name Microsoft.ProgramsAndFeatures");
        }

        private void lblPrinterList_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("ms-settings:printers");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Hotkey run POS
            if (e.KeyCode.ToString() == HKey_POS_Key)
            {
                RunPOS();
            }

            //Hotkey run POS Dashboard
            if (e.KeyCode.ToString() == HKey_POSD_Key)
            {
                RunPOSDashboard();
            }

            //Hotkey run SQL Management
            if (e.KeyCode.ToString() == HKey_SQL_Key)
            {
                RunSQL();
            }

            //Hotkey run Uninstall Program
            if (e.KeyCode.ToString() == HKey_CPanel_Key)
            {
                System.Diagnostics.Process.Start("control", "/name Microsoft.ProgramsAndFeatures");
            }

            //Hotkey run Restore
            if (e.KeyCode.ToString() == HKey_Restore_Key)
            {
                btnRestore.PerformClick();
            }

            //Hotkey run Backup
            if (e.KeyCode.ToString() == HKey_Backup_Key)
            {
                btnBackup.PerformClick();
            }

            //Hotkey run Update Script
            if (e.KeyCode.ToString() == HKey_Update_Key)
            {
                btnUpdateScript.PerformClick();
            }

            //Hotkey run Printer List
            if (e.KeyCode.ToString() == HKey_Printer_Key)
            {
                System.Diagnostics.Process.Start("ms-settings:printers");
            }

            //Hotkey run Builder Server
            if (e.KeyCode.ToString() == HKey_Build_Key)
            {
                //string currentpath = @"\\192.168.0.102\share\build";
                build_address = @Build_IP_Key;
                Process.Start("explorer.exe", build_address);
            }
        }

    }
}


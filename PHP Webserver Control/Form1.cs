using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Collections.Generic;


namespace PHP_Webserver_Control
{
    public partial class Form1 : Form
    {

        String Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PHPWebserverControl.xml");
        
        public Form1()
        {
            InitializeComponent();
            LoadList();
            CheckStatus(new Object(), new EventArgs());
        }

        /*
         * Method: SaveList (And its overloads)
         *   Desc: Saves all the items in the listbox
         */
        private void SaveList()
        {
            // Header and Options
            //
            XDocument saveList = new XDocument(
                new XDeclaration("1.0", "utf-16", "true"),
                new XComment("PHP Webserver Control Data File"),
                new XElement("WebserverControl", 
                    new XElement("serverOptions",
                        new XElement("php", tbLocation.Text),
                        new XElement("host", tbHost.Text),
                        new XElement("port", tbPort.Value.ToString()),
                        new XElement("browser", cbBrowser.Checked.ToString()),
                        new XElement("hide", cbHide.Checked.ToString())
                    )
                )
            );

            // Items
            //
            XElement DocRoot = new XElement("serverRoot");

            foreach (string item in lbRoot.Items)
                DocRoot.Add(new XElement("path", item));

            // Save
            //
            saveList.Element("WebserverControl").Add(DocRoot);
            saveList.Save(Filename);
        }
        private void SaveList(object sender, FormClosingEventArgs e)
        {
            SaveList();
        }
        private void SaveList(object sender, EventArgs e)
        {
            SaveList();
        }

        /*
         * Method: LoadList
         *   Desc: Loads the XML into the listbox
         */
        private void LoadList()
        {
            if (File.Exists(Filename))
            {
                XDocument DocRoot = XDocument.Load(Filename);

                // Load List
                //
                lbRoot.Items.AddRange(
                    (from r in DocRoot.Elements("WebserverControl").Elements("serverRoot").Descendants()
                     select r.Value).ToArray()
                );

                // Load settings into dictionary
                //
                Dictionary<string, string> CfgSettings = (from i in DocRoot.Elements("WebserverControl").Elements("serverOptions").Descendants()
                                                          select (new { i.Name, i.Value }))
                                                          .ToDictionary(i => i.Name.ToString(), i => i.Value);

                // Populate Controls
                //
                tbLocation.Text   = CfgSettings["php"];
                tbHost.Text       = CfgSettings["host"];
                tbPort.Value      = Convert.ToInt16(   CfgSettings["port"] );
                cbBrowser.Checked = Convert.ToBoolean( CfgSettings["browser"] );
                cbHide.Checked    = Convert.ToBoolean( CfgSettings["hide"] );
            }

        }

        /*
         * Method: CheckStatus
         *   Desc: Determins if php is running
         */
        private void CheckStatus(object sender, EventArgs e)
        {
            // Opdate Controls
            //
            tbStatus.ForeColor = Color.Red;
            btnStart.BackColor = SystemColors.Control;
            tbStatus.Text = "Offline";
            btnStart.Text = "&Start Server";

            if (this.cbBrowser.Checked)
                btnStart.Text += " and Browser";

            // PHP Running?
            //
            foreach (Process thisProcess in Process.GetProcesses())
            {
                if (thisProcess.ProcessName.Contains("php"))
                {
                    tbStatus.ForeColor = Color.Green;
                    tbStatus.Text = "Active";
                    btnStart.Text = "&Stop Server";
                    btnStart.BackColor = Color.Red;
                }
            }
        }

        /*
         * Method: RunServer
         *   Desc: Start / Stop Server
         */
        private void RunServer(object sender, EventArgs e)
        {
           // Are we starting or stopping?
           //
           if (btnStart.Text.Contains("Start"))
           {
               if (File.Exists(tbLocation.Text))
               {
                   string cmd = tbLocation.Text;
                   string arg = "-S " + tbHost.Text + ":" + tbPort.Value + " -t " + lbRoot.SelectedItem;

                   ProcessStartInfo Start = new ProcessStartInfo(cmd, arg);
                   Start.WindowStyle = ProcessWindowStyle.Minimized;

                   if (cbHide.Checked)
                       Start.WindowStyle = ProcessWindowStyle.Hidden;

                   Process.Start(Start);

                   if (cbBrowser.Checked)
                       Process.Start("http://" + tbHost.Text + ":" + tbPort.Value);
               }
               else
                   MessageBox.Show("Unable to find PHP.EXE", "Error");
           }
           else
           {
               // Kill all php processes
               //
               foreach (Process thisProcess in Process.GetProcesses())
               {
                   if (thisProcess.ProcessName.Contains("php"))
                       thisProcess.Kill();
               }
           }
        }

        /*
         * Method: AddFolder
         *   Desc: Adds a folder to the LB then saves
         */
        private void AddFolder(object sender, EventArgs e)
        {
            if (dirBrowse.ShowDialog() == DialogResult.OK)
                lbRoot.Items.Add(dirBrowse.SelectedPath.ToString());
            SaveList();
       }

        /*
         * Method: RemoveFolder
         *   Desc: Removes a folder to the LB then saves
         */
        private void RemoveFolder(object sender, EventArgs e)
        {
            lbRoot.Items.RemoveAt(lbRoot.SelectedIndex);
            SaveList();
        }

        /*
         * Method: ItemSelected
         *   Desc: Enable the start / stop button
         */
        private void ItemSelected(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
        }

        /*
         * Method: LocatePHP
         *   Desc: Browse for PHP
         */
        private void LocatePHP(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                tbLocation.Text = openFileDialog1.FileName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DB_Change_Alert
{
    public partial class Form1 : Form
    {
        Timer startTr = new Timer();
        public int clientW = 0, clientH = 0, gap = 5, db_colNum = 4;
        bool isRun = false;
        bool isSound = true;
        string ConnectionString = string.Empty;
        SqlConnection _conn = new SqlConnection();
        string path = AppDomain.CurrentDomain.BaseDirectory + "config.ini";
        string defaultConfig = "[Essential]\r\nserver_name=localhost\r\ndb_name=\r\nuser_id=admin\r\nuser_password=Password1\r\ninterval=1000\nisSound=no\r\nsoundType=\r\n[sql]\r\nsqlCommand=\r\n";
        Dictionary<string, string> ConfigDic = new Dictionary<string, string>();
        List<string> configList = new List<string>() { "server_name", "db_name", "user_id", "user_password", "interval","isSound", "soundType" };
        string announce1 = "1. Check DB Setting. \r\n 2. Insert SQL Query. \r\n 3. Start.";
        string announce2 = "No Matching Data Found...";

        public Form1()
        {
            InitializeComponent();
            ReadConfig();
            InitialDisplay();
            SetDisplayValues();
            InitiateTimer();
        }

        private void InitialDisplay()
        {
            this.Text = "Data Detector";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources._1447508502_db_status;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = Screen.PrimaryScreen.WorkingArea.Width / 3;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height * 3 / 5;
            clientW = ClientSize.Width;
            clientH = ClientSize.Height;

            Panel statusPanel = new Panel();
            statusPanel.Name = "statusPanel";
            statusPanel.Width = clientW;
            statusPanel.Height = clientH / 5;
            statusPanel.Location = new Point(0,0);
            statusPanel.BackColor = Color.Beige;
            statusPanel.Parent = this;
            {//Elements inside of statusPanel
                Label time = new Label();
                time.Name = "time";
                time.Size = new Size(statusPanel.Width, statusPanel.Height * 2 / 3 - gap);
                time.Location = new Point(0, 2);
                time.TextAlign = ContentAlignment.MiddleCenter;
                time.Text = "";
                time.Font = new Font("tahoma", 15);
                //time.BackColor = Color.Red;
                time.Parent = statusPanel;

                PictureBox db_pic = new PictureBox();
                db_pic.Size = new Size(statusPanel.Height - time.Height - gap, statusPanel.Height - time.Height - gap * 2);
                db_pic.Location = new Point(statusPanel.Left + gap, time.Bottom + gap);
                db_pic.Image = Properties.Resources._1447504999_db;
                db_pic.SizeMode = PictureBoxSizeMode.Zoom;
                db_pic.Parent = statusPanel;
                db_pic.BringToFront();

                Label db_address = new Label();
                db_address.Name = "db_address";
                db_address.Width = statusPanel.Width - db_pic.Width - gap * 3;
                db_address.Height = statusPanel.Height - time.Height - gap * 2;
                db_address.Location = new Point(db_pic.Right + gap, db_pic.Top );
                db_address.BorderStyle = BorderStyle.None;
                //db_address.BackColor = Color.Silver;
                db_address.ForeColor = Color.DarkBlue;
                db_address.Font = new Font("tahoma", 13);
                db_address.Parent = statusPanel;
                db_address.BringToFront();
                {//status bar movement
                }
            }

            DataGridView db_display = new DataGridView();
            db_display.Name = "db_display";
            db_display.Width = clientW;
            db_display.Height = (clientH * 3 / 4) / 2;
            db_display.Location = new Point(0, statusPanel.Bottom);
            db_display.BorderStyle = BorderStyle.None;
            db_display.Parent = this;

            Label db_display_hide = new Label();
            db_display_hide.Name = "db_display_hide";
            db_display_hide.Size = db_display.Size;
            db_display_hide.Location = db_display.Location;
            db_display_hide.BackColor = Color.LightGray;
            db_display_hide.Text = announce1;
            db_display_hide.Font = new Font("tahoma", 30);
            db_display_hide.TextAlign = ContentAlignment.MiddleCenter;
            db_display_hide.Parent = this;
            db_display_hide.BringToFront();

            Panel controlBox = new Panel();
            controlBox.Name = "controlBox";
            controlBox.Width = clientW;
            controlBox.Height = (clientH * 3 / 4) / 5;
            controlBox.Location = new Point(0, db_display.Bottom);
            controlBox.BackColor = Color.Beige;
            controlBox.Parent = this;
            {
                Button startSwitch = new Button();
                startSwitch.Name = "startSwitch";
                startSwitch.Size = new Size(controlBox.Height - gap * 2, controlBox.Height - gap * 2);
                startSwitch.Location = new Point(gap, gap);
                startSwitch.BackgroundImage = Properties.Resources._1447511458_play_circle_outline;
                startSwitch.BackgroundImageLayout = ImageLayout.Stretch;
                startSwitch.FlatStyle = FlatStyle.Flat;
                startSwitch.FlatAppearance.BorderSize = 0;
                startSwitch.MouseClick += MouseClick;
                startSwitch.Parent = controlBox;
                startSwitch.BringToFront();

                Button soundSwitch = new Button();
                soundSwitch.Name = "soundSwitch";
                soundSwitch.Size = new Size(startSwitch.Width, startSwitch.Height);
                soundSwitch.Location = new Point(((controlBox.Width - gap * 2) - soundSwitch.Width) / 2, gap);
                soundSwitch.BackgroundImage = ConfigDic["isSound"]=="yes" ? Properties.Resources._1447511542_volume_high : Properties.Resources._1447511534_30_Sound_off;
                soundSwitch.BackgroundImageLayout = ImageLayout.Stretch;
                soundSwitch.FlatStyle = FlatStyle.Flat;
                soundSwitch.FlatAppearance.BorderSize = 0;
                soundSwitch.MouseClick += MouseClick;
                soundSwitch.Parent = controlBox;
                soundSwitch.BringToFront();

                Button settingButton = new Button();
                settingButton.Name = "settingButton";
                settingButton.Size = new Size(startSwitch.Width, startSwitch.Height);
                settingButton.Location = new Point(controlBox.Right - gap - settingButton.Width, gap);
                settingButton.BackgroundImage = Properties.Resources._1447512331_editor_setting_gear_glyph;
                settingButton.BackgroundImageLayout = ImageLayout.Stretch;
                settingButton.FlatStyle = FlatStyle.Flat;
                settingButton.FlatAppearance.BorderSize = 0;
                settingButton.MouseClick += MouseClick;
                settingButton.Parent = controlBox;
                settingButton.BringToFront();
            }
            GroupBox sqlBox = new GroupBox();
            sqlBox.Name = "sqlBox";
            sqlBox.Width = clientW;
            sqlBox.Height = clientH - controlBox.Bottom;
            sqlBox.Location = new Point(0, controlBox.Bottom);
            sqlBox.BackColor = Color.White;
            sqlBox.Text = "SQL Input Box";
            sqlBox.Parent = this;
            {
                RichTextBox sqlCommand = new RichTextBox();
                sqlCommand.Name = "sqlCommand";
                sqlCommand.Size = new Size(sqlBox.Width - gap * 2, sqlBox.Height - gap * 4);
                sqlCommand.Location = new Point(gap, gap * 3);
                sqlCommand.Font = new Font("tahoma", 13);
                sqlCommand.Parent = sqlBox;
            }

        }
        private void SetDisplayValues()
        {
            //get server name
            Controls["statusPanel"].Controls["db_address"].Text = ConfigDic["server_name"];
            //get sound setting
            Button ctrl = (Button)Controls["controlBox"].Controls["soundSwitch"];
            //set sound image
            ctrl.BackgroundImage = ConfigDic["isSound"] == "yes" ? Properties.Resources._1447511542_volume_high : Properties.Resources._1447511534_30_Sound_off;

            //get previous sql command
            Controls["sqlBox"].Controls["sqlCommand"].Text = ConfigDic["sqlCommand"];
                
        }

        private void ReadConfig()
        {
            if (!Func.IsConfigExist(path)) Func.CreateConfig(path, defaultConfig);

            configList.ForEach(x =>
            {
                ConfigDic[x] = Func.GetEntryValue("Essential", x, path);
            });
            ReadSQL();
        }

        private void InitiateTimer()
        {
            Timer clockTr = new Timer();
            clockTr.Interval = 1000;
            clockTr.Tick += new EventHandler(clockTr_Tick);
            clockTr.Start();

            startTr.Interval = int.Parse(ConfigDic["interval"]);
            startTr.Tick += new EventHandler(startTr_Tick);
        }
        private void clockTr_Tick(object sender, EventArgs e)
        {
            Label time = (Label)this.Controls["statusPanel"].Controls["time"];
            time.Text = DateTime.Now.ToString().Substring(10);
            while (time.Height > System.Windows.Forms.TextRenderer.MeasureText(time.Text, new Font(time.Font.FontFamily, time.Font.Size)).Height)
            {
                time.Font = new Font(time.Font.FontFamily, time.Font.Size + 0.5f);
            }
        }

        private void startTr_Tick(object sender, EventArgs e)
        {
            //if table exist...
            ExecuteSQLCommand();
        }

        private void ExecuteSQLCommand()
        {
            string sql = Controls["sqlBox"].Controls["sqlCommand"].Text;
            DataTable dt = Func.SQL_DataTable(_conn, ConnectionString, sql);
            if (dt.Rows.Count <= 0)
            {
                Controls["db_display_hide"].Visible = true;
            }
            else
            {
                DataGridView dgv = (DataGridView)Controls["db_display"];
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
                Controls["db_display_hide"].Visible = false;
                Func.PlaySystemSound(ConfigDic["soundType"]);
                dgv.DataSource = dt;
            }
        }

        private void getSetting()
        { 
            using (Config con = new Config(path, configList))
            {
                con.ShowDialog();
                if (con.SettingResult)
                {
                    ConfigDic = con.ConfigDicVal;
                    ReadConfig();
                    SetDisplayValues();
                    startTr.Interval = int.Parse(ConfigDic["interval"]);
                }
            }
        }
        //1. first start, hceck db connection then if not connected, notice and stop
        //2. thread run at the same time. It stops while connects db
        //3. display shurink and stretch
        //4. better display design.
        //5. Better flow
        //6. 

        private void SaveSQL()
        {
            RichTextBox rt = (RichTextBox)Controls["sqlBox"].Controls["sqlCommand"];
            string cmd = "";
            for (int i = 0; i < rt.Lines.Length; i++)
            {
                if (i != rt.Lines.Length - 1) cmd += rt.Lines[i] + "\\r\\n";
                else cmd += rt.Lines[i]; 
            }
            Func.SetIniValue("sql", "sqlCommand", cmd, path);
        }

        private void ReadSQL()
        {
            //using StringBuilder replace \r\n to new line
            StringBuilder sb = new StringBuilder(Func.GetEntryValue("sql", "sqlCommand", path));
            sb.Replace("\\r\\n", System.Environment.NewLine);
            ConfigDic["sqlCommand"] = sb.ToString();
        }

        private void MouseClick(object sender, MouseEventArgs e)
        {
            Control ctrl = (Control)sender;
            switch (ctrl.Name)
            {
                case "startSwitch":
                    if (isRun)
                    {
                        startTr.Stop();
                        isRun = false;
                        Controls["controlBox"].Controls["startSwitch"].BackgroundImage = Properties.Resources._1447511458_play_circle_outline;
                        Controls["db_display_hide"].Text = announce1;
                    }
                    else
                    {
                        //save sql command to config.ini
                        SaveSQL();
                        ReadConfig();
                        Controls["db_display_hide"].Text = announce2;
                        ConnectionString = "Data Source=" + ConfigDic["server_name"] + ";Initial Catalog=" + ConfigDic["db_name"] + ";User id=" + ConfigDic["user_id"] + ";Password=" + ConfigDic["user_password"] + ";";
                        try
                        {
                            //start sql timer tick
                            startTr.Start();
                            isRun = true;
                            //change button image
                            Controls["controlBox"].Controls["startSwitch"].BackgroundImage = Properties.Resources._1447511454_24_Stop;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to get SQL Result.");
                        }
                    }
                    break;
                case "soundSwitch":
                    if (isSound)
                    {
                        isSound = false;
                        Controls["controlBox"].Controls["soundSwitch"].BackgroundImage = Properties.Resources._1447511534_30_Sound_off;
                        Func.SetIniValue("Essential", "isSound", "no", path);
                    }
                    else
                    {
                        isSound = true;
                        Controls["controlBox"].Controls["soundSwitch"].BackgroundImage = Properties.Resources._1447511542_volume_high;
                        Func.SetIniValue("Essential", "isSound", "yes", path);
                        Func.PlaySystemSound(ConfigDic["soundType"]);
                    }
                    break;
                case "settingButton":
                    getSetting();
                    break;
                default:

                    break;
            }
        }
    }
}

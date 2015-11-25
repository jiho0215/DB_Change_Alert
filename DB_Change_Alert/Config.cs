using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DB_Change_Alert
{                    //2. set sound system.
                     //3. set sql
                     //1.1 check anything on config
                     //1.2 set messagebox message.
    public partial class Config : Form
    {
        List<string> configList = new List<string>();
        Dictionary<string, string> ConfigDic = new Dictionary<string, string>();
        List<string> soundTypeList = new List<string>() { "Asterisk", "Beep", "Exclamation", "Hand", "Question" };
        int gap = 3;
        int configNum = 0;
        bool SettingChanged = false;
        string path = string.Empty;
        public Config()
        {
            InitializeComponent();
            InitialDisplay();
        }

        public Config(string path, List<string> configList)
        {
            this.configList = configList;
            this.path = path;
            InitializeComponent();
            GetConfig(path);
            InitialDisplay();
        }

        private void InitialDisplay()
        {
            configNum = configList.Count;
            this.Text = "Config Setting";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources._1447522579_editor_setting_gear_glyph;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = Screen.PrimaryScreen.WorkingArea.Width * 23 / 100;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height / 2;

            Panel config = new Panel();
            config.Name = "config";
            config.Size = new Size(ClientSize.Width, ClientSize.Height * 6 / 8);
            config.Location = new Point(0, 0);
            config.BackColor = Color.Beige;
            config.Parent = this;

            int cnt = configList.Count();
            configList.ForEach(x =>
            {
                Label lbl = new Label();
                lbl.Name = x + "_lbl";
                lbl.Size = new Size((config.Width - gap * 3) * 5 / 10, (config.Height - (gap * cnt - 1)) / cnt);
                lbl.Location = new Point(gap, gap + (lbl.Height + gap) * configList.IndexOf(x));
                lbl.Text = x + ":";
                lbl.Font = new Font("tahoma", 13);
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Parent = config;

                TextBox tb = new TextBox();
                tb.Name = x;
                tb.Font = new Font("tahoma", 13);
                tb.Size = new Size((config.Width - gap * 3) * 5 / 10, lbl.Height);
                tb.Location = new Point(lbl.Right + gap, lbl.Top + (lbl.Height - tb.Height) / 2);
                tb.Text = ConfigDic[x];
                tb.BorderStyle = BorderStyle.Fixed3D;
                tb.Parent = config;
                if (x.IndexOf("pw") > 0 || x.IndexOf("password") > 0) tb.PasswordChar = '*';
                if (x == "isSound")
                {
                    tb.Visible = false;
                    CheckBox cb = new CheckBox();
                    cb.Location = tb.Location;
                    cb.Name = x +"cb";
                    cb.Parent = config;
                    cb.Checked = true;
                    if (ConfigDic["isSound"] != "yes") cb.Checked = false;
                    cb.CheckedChanged += ValueChanged;
                }
                if (x == "soundType")
                {
                    tb.Visible = false;
                    ComboBox cb = new ComboBox();
                    cb.Name = x + "cb";
                    cb.Font = new Font("tahoma", 13);
                    cb.Size = tb.Size;
                    cb.Location = tb.Location;
                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    soundTypeList.ForEach(z =>
                    {
                        cb.Items.Add(z);
                    });
                    cb.SelectedItem = ConfigDic["soundType"];
                    cb.SelectedValueChanged += ValueChanged;
                    cb.Parent = config;
                }
            });
        
            Label message = new Label();
            message.Name = "message";
            message.Size = new Size(ClientSize.Width - gap * 2, ClientSize.Height / 8 - gap);
            message.Location = new Point(gap, (ClientSize.Height * 6 / 8));
            message.BorderStyle = BorderStyle.None;
            message.ForeColor = Color.Navy;
            message.Text = "";
            message.Font = new Font("tahoma", 10);
            message.Parent = this;
                       
            Button save = new Button();
            save.Name = "save";
            save.Size = new Size((ClientSize.Width - gap) / 2 - gap, ClientSize.Height / 8 - gap);
            save.Location = new Point(gap, ClientSize.Height - save.Height - gap);
            save.Font = new Font("tahoma", 13);
            save.Text = "Save";
            save.FlatStyle = FlatStyle.Popup;
            save.FlatAppearance.BorderSize = 0;
            save.MouseClick += BtnClick;
            save.Parent = this;

            Button cancel = new Button();
            cancel.Name = "cancel";
            cancel.Size = new Size((ClientSize.Width - gap) / 2 - gap, ClientSize.Height / 8 - gap);
            cancel.Location = new Point(save.Right + gap, save.Top);
            cancel.Font = new Font("tahoma", 13);
            cancel.Text = "Cancel";
            cancel.FlatStyle = FlatStyle.Popup;
            cancel.FlatAppearance.BorderSize = 0;
            cancel.MouseClick += BtnClick;
            cancel.Parent = this;
        }

        private void BtnClick(object sender, MouseEventArgs e)
        {
            Button ctrl = (Button)sender;
            switch (ctrl.Name)
            {
                case "save":
                        DialogResult dr = MessageBox.Show("Are you sure?", "Save", MessageBoxButtons.YesNo);
                        if (dr == DialogResult.Yes)
                        {
                            SaveConfig();
                        }               
                    break;
                case "cancel":
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        private void GetConfig(string path)
        {
            if (Func.IsConfigExist(path))
            {
                configList.ForEach(x =>
                {
                    ConfigDic[x] = Func.GetEntryValue("Essential", x, path);
                });
            }else
            {
                MessageBox.Show("Config file does not exist.");
            }
        }

        private void SaveConfig()
        {
            string ConnctionString = "Data Source=" + Controls["config"].Controls["server_name"].Text + ";Initial Catalog=" + Controls["config"].Controls["db_name"].Text + ";User id=" + Controls["config"].Controls["user_id"].Text + ";Password=" + Controls["config"].Controls["user_password"].Text + ";";
            if (Func.checkConn(ConnctionString))
            {
                MessageBox.Show("DB Connection : Succeed.");
                MessageBox.Show("Setting has been saved.", "Saved", MessageBoxButtons.OK);
                SettingChanged = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("DB Connection : Failed, please check DB setting.");
                string msg = "Please check your DB setting and try again.";
                DisplayMessage(msg);
            }

            configList.ForEach(x => 
            {
                try
                {
                    Func.SetIniValue("Essential", x, Controls["config"].Controls[x].Text, path);
                }catch(Exception e)
                {

                }
            });
        }
        private void ValueChanged(object sender, EventArgs e)
        {
            Control ctrl = (Control)sender;
            if(ctrl.Name == "isSoundcb")
            {
                CheckBox cb = (CheckBox)ctrl;
                Controls["config"].Controls["isSound"].Text = cb.Checked ? "yes" : "no";
                if (cb.Checked) Func.PlaySystemSound(Controls["config"].Controls["soundType"].Text);
                return;
            }
            else if(ctrl.Name == "soundTypecb")
            {
                ComboBox cb = (ComboBox)ctrl;
                string str = cb.SelectedItem.ToString();
                Controls["config"].Controls["soundType"].Text = str;
                Func.PlaySystemSound(str);
                return;
            }
            else
            {
                return;
            }
        }

        private void DisplayMessage(string msg)
        {
            Controls["message"].Text = msg;
        }

        public Dictionary<string, string> ConfigDicVal
        {
            get { return ConfigDic; }
        }

        public bool SettingResult
        {
            get { return SettingChanged; }
        }
    }
}

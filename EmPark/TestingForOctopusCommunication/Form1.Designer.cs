using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace TestingForOctopusCommunication
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sqlbackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.StartServieBtn = new System.Windows.Forms.Button();
            this.StopServicebtn = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button10 = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.CardEnquirybtn = new System.Windows.Forms.Button();
            this.OctGUIGoBack = new System.Windows.Forms.Button();
            this.OctPressPoll = new System.Windows.Forms.Button();
            this.Status = new System.Windows.Forms.TabControl();
            this.sqlResultTextBox = new System.Windows.Forms.RichTextBox();
            this.DisplayTxtbox = new System.Windows.Forms.RichTextBox();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.Status.SuspendLayout();
            this.SuspendLayout();
            // 
            // sqlbackgroundWorker
            // 
            this.sqlbackgroundWorker.WorkerSupportsCancellation = true;
            this.sqlbackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.sqlbackgroundWorker_DoWork);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.StartServieBtn);
            this.tabPage3.Controls.Add(this.StopServicebtn);
            this.tabPage3.Location = new System.Drawing.Point(4, 37);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(442, 576);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "工程師應用";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // StartServieBtn
            // 
            this.StartServieBtn.Location = new System.Drawing.Point(3, 6);
            this.StartServieBtn.Name = "StartServieBtn";
            this.StartServieBtn.Size = new System.Drawing.Size(208, 56);
            this.StartServieBtn.TabIndex = 12;
            this.StartServieBtn.Text = "Start Service";
            this.StartServieBtn.UseVisualStyleBackColor = true;
            this.StartServieBtn.Click += new System.EventHandler(this.StartServieBtn_Click);
            // 
            // StopServicebtn
            // 
            this.StopServicebtn.Location = new System.Drawing.Point(217, 6);
            this.StopServicebtn.Name = "StopServicebtn";
            this.StopServicebtn.Size = new System.Drawing.Size(181, 56);
            this.StopServicebtn.TabIndex = 13;
            this.StopServicebtn.Text = "Stop Services";
            this.StopServicebtn.UseVisualStyleBackColor = true;
            this.StopServicebtn.Click += new System.EventHandler(this.StopServicebtn_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button10);
            this.tabPage2.Location = new System.Drawing.Point(4, 37);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(442, 576);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "特別功能";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(19, 121);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(336, 50);
            this.button10.TabIndex = 11;
            this.button10.Text = "Show Last 4 Trasnaction";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.CardEnquirybtn);
            this.tabPage1.Controls.Add(this.OctGUIGoBack);
            this.tabPage1.Controls.Add(this.OctPressPoll);
            this.tabPage1.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabPage1.Location = new System.Drawing.Point(4, 37);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(442, 576);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "日常應用";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // CardEnquirybtn
            // 
            this.CardEnquirybtn.Enabled = false;
            this.CardEnquirybtn.Location = new System.Drawing.Point(15, 500);
            this.CardEnquirybtn.Name = "CardEnquirybtn";
            this.CardEnquirybtn.Size = new System.Drawing.Size(378, 59);
            this.CardEnquirybtn.TabIndex = 12;
            this.CardEnquirybtn.Text = "未能查詢八達通";
            this.CardEnquirybtn.UseVisualStyleBackColor = true;
            this.CardEnquirybtn.Click += new System.EventHandler(this.CardEnquirybtn_Click_1);
            // 
            // OctGUIGoBack
            // 
            this.OctGUIGoBack.BackColor = System.Drawing.Color.Red;
            this.OctGUIGoBack.Font = new System.Drawing.Font("新細明體", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.OctGUIGoBack.Location = new System.Drawing.Point(15, 261);
            this.OctGUIGoBack.Name = "OctGUIGoBack";
            this.OctGUIGoBack.Size = new System.Drawing.Size(378, 221);
            this.OctGUIGoBack.TabIndex = 1;
            this.OctGUIGoBack.Text = "。。返回";
            this.OctGUIGoBack.UseVisualStyleBackColor = false;
            this.OctGUIGoBack.Click += new System.EventHandler(this.OctGUIGoBack_Click);
            // 
            // OctPressPoll
            // 
            this.OctPressPoll.BackColor = System.Drawing.Color.Gray;
            this.OctPressPoll.Enabled = false;
            this.OctPressPoll.Font = new System.Drawing.Font("新細明體", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.OctPressPoll.Location = new System.Drawing.Point(15, 33);
            this.OctPressPoll.Name = "OctPressPoll";
            this.OctPressPoll.Size = new System.Drawing.Size(378, 208);
            this.OctPressPoll.TabIndex = 0;
            this.OctPressPoll.Text = "八達通未能使用";
            this.OctPressPoll.UseVisualStyleBackColor = false;
            this.OctPressPoll.Click += new System.EventHandler(this.OctPressPoll_Click);
            // 
            // Status
            // 
            this.Status.Controls.Add(this.tabPage1);
            this.Status.Controls.Add(this.tabPage2);
            this.Status.Controls.Add(this.tabPage3);
            this.Status.Font = new System.Drawing.Font("新細明體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Status.Location = new System.Drawing.Point(8, 12);
            this.Status.Name = "Status";
            this.Status.SelectedIndex = 0;
            this.Status.Size = new System.Drawing.Size(450, 617);
            this.Status.TabIndex = 10;
            // 
            // sqlResultTextBox
            // 
            this.sqlResultTextBox.Location = new System.Drawing.Point(488, 208);
            this.sqlResultTextBox.Name = "sqlResultTextBox";
            this.sqlResultTextBox.Size = new System.Drawing.Size(469, 417);
            this.sqlResultTextBox.TabIndex = 21;
            this.sqlResultTextBox.Text = "";
            // 
            // DisplayTxtbox
            // 
            this.DisplayTxtbox.Font = new System.Drawing.Font("新細明體", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.DisplayTxtbox.Location = new System.Drawing.Point(488, 12);
            this.DisplayTxtbox.Name = "DisplayTxtbox";
            this.DisplayTxtbox.Size = new System.Drawing.Size(469, 177);
            this.DisplayTxtbox.TabIndex = 22;
            this.DisplayTxtbox.Text = "";
            // 
            // Form1
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.DisplayTxtbox);
            this.Controls.Add(this.sqlResultTextBox);
            this.Controls.Add(this.Status);
            this.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "Form1";
            this.Text = "Octopus Application Release 1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.Status.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BackgroundWorker sqlbackgroundWorker;
        private NotifyIcon notifyIcon;
        private TabPage tabPage3;
        private Button StartServieBtn;
        private Button StopServicebtn;
        private TabPage tabPage2;
        private Button button10;
        private TabPage tabPage1;
        private Button CardEnquirybtn;
        private Button OctGUIGoBack;
        private Button OctPressPoll;
        private TabControl Status;
        private RichTextBox sqlResultTextBox;
        private RichTextBox DisplayTxtbox;
    }
}


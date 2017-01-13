using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using DataLayer;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
//using MessageBoxExLib;
using Tamir.SharpSsh;
using Timer = System.Windows.Forms.Timer;
using Utils.MessageBoxExLib;


namespace TestingForOctopusCommunication
{
    public partial class Form1 : Form
    {
        //Here is the once-per-class call to initialize the log object
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static Timer timer = new Timer();
        public OctopusLibrary.DevVerRec DevVerRec = new OctopusLibrary.DevVerRec();
        private static IScheduler _scheduler;
        private int _maxWidth;
        private int _maxHeight;
        private static Boolean isBusy=false;
        public DateTime TransDataTime;
        public static Timer XfileTimer = new Timer();


        //readonly string conString = DataLayer.Db.ConnectionString;
        public Form1()
        {
            InitializeComponent();
            this.Resize += new EventHandler(this.Form1_Resize);
            //_maxWidth = (int)(SystemInformation.WorkingArea.Width * 0.60);
            //_maxHeight = (int)(SystemInformation.WorkingArea.Height * 0.90);

        }

        public SqlClient SqlClient
        {
            get { return new SqlClient(); }
        }

        public static void UnloadModule(string moduleName)
        {
            foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
            {
                if (mod.ModuleName == moduleName)
                {
                    OctopusLibrary.FreeLibrary(mod.BaseAddress);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {


            this.Size = new Size(_maxWidth, _maxHeight);



            SetOptimumSize();



            base.OnLoad(e);
        }

        private void SetOptimumSize()
        {
            int ncWidth = this.Width - this.ClientSize.Width;
            int ncHeight = this.Height - this.ClientSize.Height;

            //int iconAndMessageRowWidth = rtbMessage.Width + ICON_MESSAGE_PADDING + panelIcon.Width;
            //int saveResponseRowWidth = chbSaveResponse.Width + (int)(panelIcon.Width / 2);
            //int buttonsRowWidth = GetWidthOfAllButtons();
            //int captionWidth = GetCaptionSize().Width;

            //int maxItemWidth = Math.Max(saveResponseRowWidth, Math.Max(iconAndMessageRowWidth, buttonsRowWidth));

            //int requiredWidth = LEFT_PADDING + maxItemWidth + RIGHT_PADDING + ncWidth;
            ////Since Caption width is not client width, we do the check here
            //if (requiredWidth < captionWidth)
            //    requiredWidth = captionWidth;

            //int requiredHeight = TOP_PADDING + Math.Max(rtbMessage.Height, panelIcon.Height) + ITEM_PADDING + chbSaveResponse.Height + ITEM_PADDING + GetButtonSize().Height + BOTTOM_PADDING + ncHeight;

            ////Fix the bug where if the message text is huge then the buttons are overwritten.
            ////Incase the required height is more than the max height then adjust that in the
            ////message height
            //if (requiredHeight > _maxHeight)
            //{
            //    rtbMessage.Height -= requiredHeight - _maxHeight;
            //}

            //int height = Math.Min(requiredHeight, _maxHeight);
            //int width = Math.Min(requiredWidth, _maxWidth);
            //this.Size = new Size(width, height);
        }

        private Size MeasureString(string str, int maxWidth, Font font)
        {
            Graphics g = this.CreateGraphics();
            SizeF strRectSizeF = g.MeasureString(str, font, maxWidth);
            g.Dispose();

            return new Size((int)Math.Ceiling(strRectSizeF.Width), (int)Math.Ceiling(strRectSizeF.Height));
        }

        /// <summary>
        /// Measures a string using the Graphics object for this form and the
        /// font of this form
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        private Size MeasureString(string str, int maxWidth)
        {
            return MeasureString(str, maxWidth, this.Font);
        }

        private void SQLChanges(object sender, EventArgs e)
        {
            DetechSQLChanges();
        }

        public void DetechSQLChanges()
        {

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString)) //Old Version
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Close();
                    sqlConnection.Open();
                }
                
                using (var command = new SqlCommand(@"SELECT  
       [REF_NO]
      ,[CAR_NO]
      ,HOUR_PARK_OCTOPUS.[PARK_ID]
      ,HOUR_PARK_OCTOPUS.[PAY_AMT]
      ,[DEVICE_ID]
      ,[OCTOPUS_CARD_NO]
      ,[REMAIN_VALUE]
      ,[TRANS_DATE_TIME]
      ,[TRANS_NO]
      ,[CREATE_DATE]
      ,[EXPIRY_DATE]
  FROM [CARPARK_CLIENT].[dbo].[HOUR_PARK_OCTOPUS], [CARPARK_CLIENT].[dbo].[HOUR_PARK]
  WHERE HOUR_PARK.ID = PARK_ID AND HOUR_PARK_OCTOPUS.STATUS_ID = 0", sqlConnection))
                {
                    command.CommandTimeout = 55;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            timer.Stop();
                            var OctValue = (int)reader["PAY_AMT"];
                            var sqltransaction = (string)reader["CAR_NO"];
                            var Ref_no = (string)reader["REF_NO"];
                            log.Info(string.Format("Car ID {0}) Payment Amount {1} Invoice Number {2} in progress...........",
                                sqltransaction, Convert.ToDecimal(OctValue).ToString("#,##0.00"), Ref_no));

                            this.TopMost = true;
                            this.Show();
                            this.WindowState = FormWindowState.Maximized;
                            this.Activate();

                           DisplayTxtbox.Clear();
                           DisplayTxtbox.BackColor = Color.White;
                           DisplayTxtbox.Text += "車牌號碼: " + sqltransaction+Environment.NewLine;
                           DisplayTxtbox.Text += "    付款: $" + Convert.ToDecimal(OctValue).ToString("#,##0.00")+Environment.NewLine;
                           DisplayTxtbox.Text += "交易進行中........"+Environment.NewLine;
                            sqlResultTextBox.Clear();
                            sqlResultTextBox.BackColor = Color.White;

                            //DueAmountDisplaylabelText = "";
                            //DueAmountDisplaylabel.Text = "$ " + Convert.ToDecimal(OctValue).ToString("#,##0.00");
                            //CarIDisplaylaberl.Text = "";
                            //CarIDisplaylaberl.Text = sqltransaction;
                            //TransactionStatuslabel.Text = "交易進行中........";


                            var communicateStatus = CheckQctopusConnection();
                            log.Info("Check Communication Status " + GetErrorMessage(communicateStatus));
                            if (communicateStatus == 0)
                            {
                                log.Info("Enable Transaction");
                                OctGUINormalState();
                            }

                            #region PayAmountOver900

                            if (OctValue > 900) //If Pay Amount is over 900 HKD
                            {
                                timer.Stop();
                                log.Info(string.Format("!!!!!!!!!!!!----  Payment Over HKD $900 "));

                                OctPressPoll.Text = "超越八達通交易金額上限 HKD $900";
                                OctPressPoll.Enabled = false;

                                using (Form newForm = new Form())
                                {
                                    newForm.TopMost = true;
                                    newForm.Activate();
                                    MessageBox.Show(newForm, "超越八達通交易金額上限\n HKD $900\n" +
                                                             "只可以用現金支付!");
                                }
                                OctPressPoll.Text = "八達通不接受";
                                OctPressPoll.Enabled = false;
                               

                            }
                            #endregion


                            var sqlforEnableTransaction = new SqlClient();
                            log.Info("Change Transaction to Pending.......");
                            sqlforEnableTransaction.ErrorUpdateTable();

                            reader.Close();




                        }

                    }
                }
            }
        }

        private void OctGUINormalState()
        {
            OctopousInitDiplay();
            OctPressPoll.BackColor = Color.Lime;
            OctPressPoll.Text = "請拍八達通";
            OctPressPoll.Enabled = true;
            CardEnquirybtn.Text = "查詢八達通";
            CardEnquirybtn.BackColor = Color.Yellow;
            CardEnquirybtn.Enabled = true;
        }

        private string GetDeviceId()
        {
            string DeviceID = string.Format("{0:x}", DevVerRec.DevID).ToUpper();
            return DeviceID;
        }

        private static void OctopousIdleDisplay()
        {
            OctopusLibrary.TxnAmt(0, -30000, 0, 0);
        }

        private static void OctopousNotInServiceDisplay()
        {
            OctopusLibrary.TxnAmt(-30001, -30001, 0, 0);
        }

        private static void OctopousDisplayBalanace(int RV)
        {
            OctopusLibrary.TxnAmt(-30000, RV, 0, 0);
        }

        private static void OctopousInitDiplay()
        {
            OctopusLibrary.TxnAmt(-30000, -30000, 0, 0);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //            Private Declare Function InitComm Lib "rwl" Alias "_InitComm@8" (ByVal cPort As Byte, ByVal 
            //cBaud As Long) As Long

            //[DllImport("rwl.dll", SetLastError=true, CharSet = CharSet.Unicode)];
            //static extern long InitComm(string cPort, string cBaud);


            Int32 CommunicateStatus = OctopusLibrary.InitComm(0, 115200);
            MessageBox.Show(CommunicateStatus.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int TxnAmtStatus = OctopusLibrary.TxnAmt(0, 9999, 2, 0);
            MessageBox.Show(TxnAmtStatus.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //var myrecord = new OctopusLibrary.DevVerRec();
            //int DevVerRecStatus = OctopusLibrary.TimeVer(ref myrecord);
            MessageBox.Show("Device ID" + DevVerRec.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StringBuilder PollData = new StringBuilder(256);
            int PollStatus = OctopusLibrary.Poll(0, 200, PollData);
            //  string cardID = PollData.ToString().Substring(0,8);

            //String s = PollData.ToString();

            log.Info(string.Format("StandAlone For Poll Function Status {0} , Poll Date :{1}", PollStatus, PollData));


            MessageBox.Show(PollData.ToString());

            log.Info("PollData :" + PollData);


        }

        private void button6_Click(object sender, EventArgs e)
        {
            char[] transactionCode = { '0', '0', '1', '2', '3', '4', '0', '0', '0', '0', '0', '0' };
            int result = OctopusLibrary.AddValue(10000, 1, transactionCode);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // log4net.ILog log = log4net.LogManager.GetLogger(this.GetType());
            log.Info("_____The program loaded______");

            log.Info("Error Checking++++++++++++++++");
            
            string path = Application.StartupPath + "/" + "CriticalError.txt";
            if (File.Exists(path))
            {
                log.Info("-----Error Found and Alert Users----");
                Form2 f2 = new Form2(); //{ this.richTextBox1 = "I like big butts; } 
                //f2.WindowState=FormWindowState.Maximized;
                //f2.TopMost = true;
                //f2.Activate();
                f2.Show();
            }
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory(); //Add job
            _scheduler = schedulerFactory.GetScheduler();
            _scheduler.Start();
            log.Info("Communication Status starting Scheduler");
            AddJob();

        FirstCommunication:

            int formloadCommunicateStatus = CheckQctopusConnection();    //
            //int formloadCommunicateStatus = 0;                  //For Debug
            DataLayer.Db.ApplicationName = "Win Octopus Reader";
            DataLayer.Db.ConnectionTimeout = 900000000;

            if (formloadCommunicateStatus == 0)
            {
                var sql = new SqlClient();

                sql.RemoveCurrentRecord();
                OctopusLibrary.TimeVer(ref DevVerRec);
                OctopousIdleDisplay();
                OctPressPoll.BackColor = Color.Lime;
                OctPressPoll.Text = "八達通已連線";
                OctPressPoll.Enabled = true;
                
                //Thread.Sleep(100);
                //Enable SQL detechation
                timer.Tick += SQLChanges;
                timer.Interval = 2000;
                timer.Start();

                XfileTimer.Interval = 7200000;    // milliseconds
                XfileTimer.Tick += XfileSFTP_Click;
                XfileTimer.Start();
            }             
               

                
            


        }

        private int CheckQctopusConnection()
        {

            CheckOctConnection:

            var CommunicateStatus = OctopusLibrary.InitComm(0, 115200);

            log.Info("Octopus Communication Status " + CommunicateStatus);


            if (CommunicateStatus > 0 && isBusy==false)//Check if not busy and Communication>0
            {
                log.Warn("XXXXX Octopous Reader is Dead !!!! Communication Status ---" + CommunicateStatus +
                         GetErrorMessage(CommunicateStatus));

                using (Form newForm = new Form()) //MessageBoxTemplate
                {
                    newForm.TopMost = true;
                   // newForm.Visible = true;
                    newForm.Activate();

                    MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                    msgBox.Caption = "八達通發生錯誤";
                    msgBox.Text = GetErrorMessage(CommunicateStatus) + "\n錯誤號碼 :" + CommunicateStatus;

                    // msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                    MessageBoxExButton btnRetry = new MessageBoxExButton();
                    btnRetry.Text = "重試";
                    btnRetry.Value = "Retry";


                    MessageBoxExButton btnNo = new MessageBoxExButton();
                    btnNo.Text = "放棄";
                    btnNo.Value = "Cancel";

                    msgBox.AddButton(btnRetry);
                    msgBox.AddButton(btnNo);
                    msgBox.Icon = MessageBoxExIcon.Question;

                    msgBox.Font = new Font("Microsoft YaHei", 10);

                    var result = msgBox.Show(newForm);
                    switch (result)
                    {
                        case MessageBoxExResult.Retry:
                        {

                            goto CheckOctConnection;
                        }

                        case MessageBoxExResult.Cancel:

                            this.OctPressPoll.BackColor = Color.Gray;
                            this.OctPressPoll.Text = "八達通發生錯誤 :" + GetErrorMessage(CommunicateStatus);
                            this.OctPressPoll.Enabled = false;
                            break;

                    }

                }
                
            }

            return CommunicateStatus;
        }

        private int CheckQctopusConnectionForNormalOperation()
        {

        CheckOctConnection:

            var CommunicateStatus = OctopusLibrary.InitComm(0, 115200);

            log.Info("Octopus Communication Status " + CommunicateStatus);


            if (CommunicateStatus > 0)//Check if not busy and Communication>0
            {
                log.Warn("XXXXX Octopous Reader is Dead !!!! Communication Status ---" + CommunicateStatus +
                         GetErrorMessage(CommunicateStatus));

                using (Form newForm = new Form()) //MessageBoxTemplate
                {
                    newForm.TopMost = true;
                    // newForm.Visible = true;
                    newForm.Activate();

                    MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                    msgBox.Caption = "八達通發生錯誤";
                    msgBox.Text = GetErrorMessage(CommunicateStatus) + "\n錯誤號碼 :" + CommunicateStatus;

                    // msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                    MessageBoxExButton btnRetry = new MessageBoxExButton();
                    btnRetry.Text = "重試";
                    btnRetry.Value = "Retry";


                    MessageBoxExButton btnNo = new MessageBoxExButton();
                    btnNo.Text = "放棄";
                    btnNo.Value = "Cancel";

                    msgBox.AddButton(btnRetry);
                    msgBox.AddButton(btnNo);
                    msgBox.Icon = MessageBoxExIcon.Question;

                    msgBox.Font = new Font("Microsoft YaHei", 10);

                    var result = msgBox.Show(newForm);
                    switch (result)
                    {
                        case MessageBoxExResult.Retry:
                            {

                                goto CheckOctConnection;
                            }

                        case MessageBoxExResult.Cancel:

                            this.OctPressPoll.BackColor = Color.Gray;
                            this.OctPressPoll.Text = "八達通發生錯誤 :" + GetErrorMessage(CommunicateStatus);
                            this.OctPressPoll.Enabled = false;
                            break;

                    }

                }

            }

            return CommunicateStatus;
        }

        private void AddJob()
        {
            Form1.IMyJob myJob = new Form1.MyJob(); //This Constructor needs to be parameterless
            JobDetailImpl jobDetail = new JobDetailImpl("Job1", "Group1", myJob.GetType());
            CronTriggerImpl trigger = new CronTriggerImpl("Trigger1", "Group1", "0 0/1 * * * ?"); //
            //run every minute between the hours of 8am and 5pm
            _scheduler.ScheduleJob(jobDetail, trigger);
            DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
            log.Info("Next Fire Time:" + nextFireTime.Value.ToLocalTime());
        }

        public static string GetErrorMessage(int communicateStatus)
        {
            switch (communicateStatus)
            {
                case 0:
                    return "正常";
                case 100001:
                    return "未能接駁八達通收費器";
                case 100002:
                    return "System file error";
                case 100003:
                    return "Invalid input parameters (e-Purse) ";
                case 100005:
                    return "未能接駁八達通收費器";
                case 100016:
                    return "讀卡錯誤，請重試";
                case 100017:
                    return "Card write error";
                case 100101:
                    return "Failed to create AR (SaveLog)";
                case 100102:
                    return "Failed to create UD (SaveLog)";
                case 100099:
                    return "Firmware upgrade has performed due to HouseKeeping()";
                case 100007:
                    return "讀卡錯誤，請重試";
                case 100019:
                    return "此卡失效";
                case 100020:
                    return "請再拍卡";
                case 100021:
                    return "此八達通卡或產品已失效，請聯絡港鐵客務中心";
                case 100022:
                    return "+++ 請勿取消交易 +++ \n 交易未能完成 請通知顧客用同一張卡 \n再次拍卡，\n 以確保交易無誤";
                case 100023:
                    return "Transaction Log full ";
                case 100024:
                    return "此卡失效";
                case 100025:
                    return "交易未完成，請重試";
                case 100032:
                    return "請再拍卡";
                case 100034:
                    return "讀卡錯誤，請重試";
                case 100035:
                    return "Card recover error";
                case 100048:
                    return "餘額不足";
                case 100049:
                    return "儲值額超出上限 ";
                case 100050:
                    return "Quota exceeded ";
                case 100051:
                    return "控制台識別號碼不正確 ";
                case 100055:
                    return "e-Purse go negative";
                case 100056:
                    return "The calling sequence of DeferDeduct is wrong";
                case 100066:
                    return "POS system time is invalid";
                default:
                    return "發生錯誤(編號 " + communicateStatus + ")";

            }
        } //All Complete list of Messsage

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Info("________Program Shut down, Octopus Reset and Port Close_____");

            OctopusLibrary.Rest();
            OctopusLibrary.PortClose();

            UnloadModule("rwl");

            System.Environment.Exit(0);
        }





        private void sqlbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void StopServicebtn_Click(object sender, EventArgs e)
        {
            timer.Stop();
            log.Info("Monitor SQL Changes Stop");
        }

        private void StartServieBtn_Click(object sender, EventArgs e)
        {
            timer.Tick += SQLChanges;
            timer.Interval = 2000;
            timer.Start();
            log.Info("Monitor SQL Changes Re- Start");
        }

        private void HouseKeeping_Click(object sender, EventArgs e)
        {
            int status = OctopusLibrary.HouseKeeping();
            MessageBox.Show(string.Format("House Keeping Status : {0}", status));
        }

        private void button10_Click(object sender, EventArgs e) //Display last 4 transaction from the SQL
        {
            sqlResultTextBox.Clear();
            using (SqlConnection conn =
                    new SqlConnection(
                        ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString))
            {
                if (conn.State!=ConnectionState.Open)
                {
                    conn.Close();
                    conn.Open();
                }   
                
                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(
                    @"SELECT TOP 4 
       [REF_NO]
      ,HOUR_PARK_OCTOPUS.[PAY_AMT]
      ,[DEVICE_ID]
      ,[OCTOPUS_CARD_NO]
      ,[REMAIN_VALUE]
      ,[TRANS_DATE_TIME]
      ,[TRANS_NO]
      ,[CREATE_DATE]
      ,HOUR_PARK_OCTOPUS.[UPDATE_DATE]
  FROM [CARPARK_CLIENT].[dbo].[HOUR_PARK_OCTOPUS], [CARPARK_CLIENT].[dbo].[HOUR_PARK]
  WHERE HOUR_PARK.ID = PARK_ID AND HOUR_PARK_OCTOPUS.STATUS_ID =1
  Order By [TRANS_DATE_TIME] DESC", conn);
                adapter.Fill(ds);
                sqlResultTextBox.Text += "Invoice #           Date Time         Amount   OCTOPUS #  Device ID" +
                                         Environment.NewLine;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    //for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    sqlResultTextBox.Text += "     " + row[0].ToString().Substring(10, 4) + "  "; // Invoice #
                    sqlResultTextBox.Text += "       " + row[5].ToString() + "   "; // Date time
                    sqlResultTextBox.Text += "$" + Convert.ToDecimal(row[1].ToString()).ToString("#,#0.0") + "    ";
                    //Amount
                    sqlResultTextBox.Text += "" + row[3].ToString() + "    ";
                    sqlResultTextBox.Text += "" + row[2].ToString().ToUpper() + "  ";
                    sqlResultTextBox.Text += Environment.NewLine;
                }
            }

            

            log.Info(" User Request last Four Transaction");
        }



        private void XfileSFTP_Click(object sender, EventArgs e)
        {
            isBusy = true;
            log.Info("---- User Request XFile and Upload the EOD----");
            OctopousNotInServiceDisplay();
            OctPressPoll.Text = "八達通正進行結數";
            OctPressPoll.Enabled = false;

            string _ftpURL = "sftp.impark.com.hk"; //Host URL or address of the SFTP server
            string _UserName = "sftp_user"; //User Name of the SFTP server
            string _Password = "e1media"; //Password of the SFTP server
            int _Port = 22; //Port No of the SFTP server (if any)
            string _ftpDirectory = "Upload"; //The directory in SFTP server where the files will be uploaded
            string LocalDirectory = @"C:\Temp"; //Local directory from where the files will be uploaded
            //File name, which one will be uploaded
             CheckQctopusConnectionForNormalOperation();
            StringBuilder XFileName = new StringBuilder(256); //Working example in here
            int Octstatus = OctopusLibrary.XFile(XFileName);    
            string[] FileName = XFileName.ToString().Split(" ").ToArray(); //Working example
            //int Octstatus = 0;
            sqlResultTextBox.Clear();
            log.Info("---- User Request XFile---");
            sqlResultTextBox.Text += DateTime.Now + "  ---- User Request XFile---" + Environment.NewLine;
            if (Octstatus == 0)
            {
                //sqlResultTextBox.Text += "File Name : " + XFileName.ToString() + Environment.NewLine;
                sqlResultTextBox.Text += DateTime.Now + "  ---XFile  Genereated Sucessfully" + Environment.NewLine;
                foreach (var s in FileName)
                {
                    sqlResultTextBox.Text += DateTime.Now + "  ---XFile Name :" + s + Environment.NewLine;
                    log.Info("---XFile  File Name : " + s);
                }
                sqlResultTextBox.Text += DateTime.Now + "---XFile  Genereated Sucessfully----" + Environment.NewLine;
                log.Info("---XFile  Genereated Sucessfully----");

                sqlResultTextBox.Refresh();
                //log.Info("File Name : " + XFileName.ToString());
                OctGUINormalState();
            }
            else
            {
                sqlResultTextBox.Text += DateTime.Now + "  ---XFile Failed-- Error Code" + Octstatus + Environment.NewLine;
                log.Info("---XFile Failed-- Error Code" + Octstatus);

            }


            //#region Upload Exchange

            //while (true)
            //{
            //    LocalDirectory = @"C:\RWL\Upload";
            //    _ftpDirectory = "Upload";
            //    try
            //    {
            //        log.Info("---Sftp Connecting----");
            //        sqlResultTextBox.Text += DateTime.Now + " ---Sftp Connecting----" + Environment.NewLine;
            //        Sftp oSftp = new Sftp(_ftpURL, _UserName, _Password);

            //        oSftp.Connect(_Port);
            //        log.Info("---Sftp Connected and Start Upload----");
            //        sqlResultTextBox.Text += DateTime.Now + "---Sftp Connected and Start Upload-----" + Environment.NewLine;

            //        foreach (var s in FileName)
            //        {
            //            sqlResultTextBox.Text += DateTime.Now + "---XFile Name" + s + " Start Upload" + Environment.NewLine;
            //            oSftp.Put(LocalDirectory + "/" + Path.GetFileName(s), _ftpDirectory + "/" + Path.GetFileName(s));
            //            sqlResultTextBox.Text += DateTime.Now + "---XFile Name" + s + " Finishe Upload" + Environment.NewLine;
            //        }


            //        log.Info("---Sftp Connected and Finished Upload----");

            //        log.Info("---Sftp Finished----");
            //        sqlResultTextBox.Text += "---Sftp Finished----" + Environment.NewLine;
            //        oSftp.Close();

            //        log.Info("---Sftp Connection Close----");

            //        break;

            //    }
            //    catch (Exception r)
            //    {
                    
            //        log.Error("XXXXXXXXXSFTP ERROR" + "\n" + r);
            //        sqlResultTextBox.Text += "---XXXXXXXXXSFTP ERROR----" + Environment.NewLine;
            //        sqlResultTextBox.Text += "------八達通上數發生錯誤---" + Environment.NewLine;
            //        sqlResultTextBox.Text += "------請致電Donnie--------" + Environment.NewLine;
            //        sqlResultTextBox.Text += "------電話: 91698541------" + Environment.NewLine;
            //        sqlResultTextBox.Text += r + Environment.NewLine;
            //        File.WriteAllText("CriticalError.txt", sqlResultTextBox.Text); 
            //        using (Form newForm = new Form())
            //        {
            //            newForm.TopMost = true;
            //            newForm.Activate();
            //            MessageBox.Show(newForm, "八達通上數發生錯誤 " +
            //                                     "\n請致電Donnie" +
            //                                     "\n電話: 91698541 \n" + r, "Error",
            //                MessageBoxButtons.OK);
            //        }

            //        log.Error("---SFTp Upload Retry Restart");
            //        sqlResultTextBox.Text += "---XXXXXXXXXSFTP ERROR----" + Environment.NewLine;
            //    }
            //}

            //#endregion


            //#region Download Black list

            //while (true)
            //{
            //    try
            //    {
            //        _ftpDirectory = "Download";
            //        log.Info("---Sftp Connecting foe Blacklist----");
            //        Sftp oSftp = new Sftp(_ftpURL, _UserName, _Password);
            //        oSftp.Connect(_Port);
            //        log.Info("---Sftp Connected----");
            //        ArrayList FileList = oSftp.GetFileList(_ftpDirectory);
            //        FileList.Remove(".");
            //        FileList.Remove(".."); //Remove . from the file list
            //        FileList.Remove("Processed");   //Remove folder name from the file list. If there is no folder name remove the code.

            //        for (int i = 0; i < FileList.Count; i++)
            //        {
            //            if (!File.Exists(LocalDirectory + "/" + FileList[i]))
            //            {
            //                log.Info("---Sftp downloading----");
            //                oSftp.Get(_ftpDirectory + "/" + FileList[i], LocalDirectory + "/" + FileList[i]);
            //                //oSftp.Get(_ftpDirectory + "/" + FileList[i], LocalDirectory + "/" + FileList[i]);
            //                Thread.Sleep(100);
            //            }
            //        }
            //        oSftp.Close();
            //        log.Info("---Sftp Connection Close----");
            //        break;
            //    }
            //    catch (Exception exception)
            //    {
            //        log.Error("XXXXXXXXXSFTP ERROR" + "\n" + exception);
            //        sqlResultTextBox.Text += "---XXXXXXXXXSFTP ERROR----" + Environment.NewLine;
            //        using (Form newForm = new Form())
            //        {
            //            newForm.TopMost = true;
            //            newForm.Activate();
            //            newForm.BackColor = Color.Gray;
            //            newForm.WindowState = FormWindowState.Maximized;
            //            newForm.Visible = true;
            //            MessageBox.Show(newForm, "八達通下載發生錯誤 " +
            //                                     "\n請致電Donnie先生" +
            //                                     "\n電話: 91698541 \n", "Error",
            //                MessageBoxButtons.OK);
            //        }

            //        log.Error("---SFTp Download Retry Restart");
            //        sqlResultTextBox.Text += "---XXXXXXXXXSFTP Download ERROR----" + Environment.NewLine;
            //        sqlResultTextBox.Text += exception + Environment.NewLine;
            //        File.WriteAllText("CriticalError.txt", sqlResultTextBox.Text); 
            //    }
            //}

            //#endregion

            log.Info("Hose Keeping call");
            var housekeepingstatus = OctopusLibrary.HouseKeeping();
            sqlResultTextBox.Text += DateTime.Now + "House keeping Call Status" + GetErrorMessage(housekeepingstatus);
            log.Info("Hose Keeping call status" +housekeepingstatus);

            
          //  sqlResultTextBox.Clear();
            OctopousIdleDisplay();
            OctopousInitDiplay();
        }



        //}

        private class MyJob : Form1.IMyJob
        {
            public void Execute(IJobExecutionContext context)
            {
                if (Form1.isBusy!=true)
                {

                    log.Info("Scheduled Communication Check");
                ScheduleCheckOctConnection:
                    var CommunicateStatus = OctopusLibrary.InitComm(0, 115200);

                    log.Info("Octopus Communication Status " + CommunicateStatus);


                    if (CommunicateStatus > 0)//Check if not busy and Communication>0
                    {
                        log.Warn("XXXXX Octopous Reader is Dead !!!! Communication Status ---" + CommunicateStatus +
                                 GetErrorMessage(CommunicateStatus));

                        using (Form newForm = new Form()) //MessageBoxTemplate
                        {
                            newForm.TopMost = true;
                            // newForm.Visible = true;
                            newForm.Activate();

                            MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                            msgBox.Caption = "八達通發生錯誤";
                            msgBox.Text = GetErrorMessage(CommunicateStatus) + "\n錯誤號碼 :" + CommunicateStatus;

                            // msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                            MessageBoxExButton btnRetry = new MessageBoxExButton();
                            btnRetry.Text = "重試";
                            btnRetry.Value = "Retry";


                            MessageBoxExButton btnNo = new MessageBoxExButton();
                            btnNo.Text = "放棄";
                            btnNo.Value = "Cancel";

                            msgBox.AddButton(btnRetry);
                            msgBox.AddButton(btnNo);
                            msgBox.Icon = MessageBoxExIcon.Question;

                            msgBox.Font = new Font("Microsoft YaHei", 10);

                            var result = msgBox.Show(newForm);
                            switch (result)
                            {
                                case MessageBoxExResult.Retry:
                                    {

                                        goto ScheduleCheckOctConnection;
                                    }

                                case MessageBoxExResult.Cancel:

                                    //Form1.OctPressPoll.BackColor = Color.Gray;
                                    //Form1.OctPressPoll.Text = "八達通發生錯誤 :" + GetErrorMessage(CommunicateStatus);
                                    //Form1.OctPressPoll.Enabled = false;
                                    break;

                            }

                        }

                    }    
                }
                
                
            }
        }

        internal interface IMyJob : IJob
        {
        }




        private void Form1_Resize(object sender, EventArgs e)
        {
            MainFormMinimize();
        }

        private void MainFormMinimize()
        {
            notifyIcon.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon.BalloonTipText = "You have successfully minimized your form.";

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            this.Activate();
        }


        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(1, 2), 10);//base 10 on the presentation
            return bytes;

        }

        private void OctPressPoll_Click(object sender, EventArgs e)
        {
        OctPressPoll:
            isBusy = true;
            CardEnquirybtn.Enabled = false;
            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString))
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Close();
                    sqlConnection.Open();
                }
               

                using (var command = new SqlCommand(@"SELECT  
       [REF_NO]
      ,[CAR_NO]
      ,HOUR_PARK_OCTOPUS.[PARK_ID]
      ,HOUR_PARK_OCTOPUS.[PAY_AMT]
      ,[DEVICE_ID]
      ,[OCTOPUS_CARD_NO]
      ,[REMAIN_VALUE]
      ,[TRANS_DATE_TIME]
      ,[TRANS_NO]
      ,[CREATE_DATE]
      ,[EXPIRY_DATE]
  FROM [CARPARK_CLIENT].[dbo].[HOUR_PARK_OCTOPUS], [CARPARK_CLIENT].[dbo].[HOUR_PARK]
  WHERE HOUR_PARK.ID = PARK_ID AND HOUR_PARK_OCTOPUS.STATUS_ID = 2", sqlConnection))
                {
                    command.CommandTimeout = 55;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            var OctValue = (int)reader["PAY_AMT"];
                            var sqltransaction = (string)reader["CAR_NO"];
                            var Ref_no = (string)reader["REF_NO"];
                            log.Info(string.Format("Car ID {0}) Payment Amount {1} Invoice Number {2}in progress...........",
                                sqltransaction, Convert.ToDecimal(OctValue).ToString("#,##0.00"), Ref_no));

                            //this.TopMost = true;
                            //this.Show();
                            //this.WindowState = FormWindowState.Maximized;
                            // this.Activate()


                        FirstPoll:
                            OctopousInitDiplay();
                            StringBuilder PollData = new StringBuilder(128);
                            int PollStatus = 0;

                            var TxnAmtStatus = OctDisplayPayAmount(OctValue);
                            PollStatus = OctopusLibrary.Poll(0, 30.ToByte(), PollData);


                            if (PollStatus < 100000) //if poll first do not have error
                            {

                                string cardId = PollData.ToString().Substring(0, 8);
                                log.Info(string.Format("Sucessful Poll..Balance {0} CardID {1}",
                                    (Convert.ToDecimal(PollStatus) / 10).ToString("#,##.0"), cardId));
                            Deduct:

                                string InvoiceNuberForAI = Ref_no.Substring(12, 2);

                                //var additionalInformationForTransaction = InvoiceNuberForAI.ToCharArray();
                                
                               
                                //char[] additionalInformationForTransaction = InvoiceNuberForAI.ToCharArray();;
                            byte[] additionalInformationForTransaction = ASCIIEncoding.ASCII.GetBytes(InvoiceNuberForAI);

                               Array.Resize(ref additionalInformationForTransaction,7);// double ensure that is 7bytes array

                                //additionalInformationForTransaction[0] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(InvoiceNuberForAI.Length- 4, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(InvoiceNuberForAI.Length-3, 1)));
                                //additionalInformationForTransaction[1] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(InvoiceNuberForAI.Length- 2, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(InvoiceNuberForAI.Length-1, 1)));
                               // additionalInformationForTransaction[4] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(4, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(5, 1)));
                                //additionalInformationForTransaction[3] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(6, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(7, 1)));
                                //additionalInformationForTransaction[4] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(8, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(9, 1)));
                                //additionalInformationForTransaction[5] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(10, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(11, 1)));
                                //additionalInformationForTransaction[6] = Convert.ToChar(Convert.ToInt16(InvoiceNuberForAI.Substring(12, 1)) * 16 + Convert.ToInt16(InvoiceNuberForAI.Substring(13, 1)));


                              //byte[] someBytes = StringToByteArray(additionalInformationForTransaction);

                                int balance = 0;
                                balance = OctopusLibrary.Deduct(OctValue * 10,additionalInformationForTransaction);
                                TransDataTime = DateTime.Now;

                                if (balance == 100022 || balance == 100048 || balance ==100021)
                                {
                                    log.Info(string.Format(" Deduct Value {0} , Error Code {1}",
                Convert.ToDecimal(OctValue).ToString("#,##.0"),
                balance + "   " + GetErrorMessage(balance)));

                                }


                                else
                                {
                                    log.Info(string.Format(" Deduct Value {0} , Remain Value {1}",
Convert.ToDecimal(OctValue).ToString("#,##.0"),
(Convert.ToDecimal(balance) / 10).ToString("#,##.0")));
                                }



                                #region 100022

                                if (balance == 100022) // Handle 100022
                                {
                                    timer.Stop();
                                    log.Warn("XXX Error 100022 Occur " + GetErrorMessage(balance));

                                    sqlResultTextBox.BackColor = Color.Red;

                                    using (Form newForm = new Form())
                                    {
                                        newForm.TopMost = true;
                                        newForm.Activate();
                                        //newForm.BackColor = Color.Gray;
                                        newForm.WindowState = FormWindowState.Maximized;
                                        //newForm.Visible = true;
                                        var result = MessageBox.Show(newForm,
                                            GetErrorMessage(balance) + "\n 八達通號碼:" + cardId,
                                            "錯誤碼" + balance, MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                                        switch (result)
                                        {
                                            case DialogResult.OK:
                                                {

                                                    int PollStatus2 = 0;
                                                    StringBuilder PollData2 = new StringBuilder(256);

                                                    DateTime startTime = DateTime.Now;

                                                    for (; ; )
                                                    {
                                                        var TxnAmtStatus1 = OctDisplayPayAmount(OctValue);
                                                        PollStatus2 = OctopusLibrary.Poll(0, 30.ToByte(), PollData2);
                                                        log.Info("PollStatus : " + PollStatus2);
                                                        if (PollStatus2 < 100000) //Normal poll not the same card
                                                        {
                                                            var TxnAmtStatus2 = OctDisplayPayAmount(OctValue);
                                                            string cardId2 = PollData2.ToString().Substring(0, 8);
                                                            if (cardId == cardId2)
                                                            {
                                                                log.Info("***1000222 Same Card Found****");
                                                                //exit = true;
                                                                goto Deduct;
                                                            }
                                                            else //Not the same card
                                                            {
                                                                log.Info("*****100022 Not the Same Card Found*****");
                                                                var TxnAmtStatus3 = OctDisplayPayAmount(OctValue);

                                                                sqlResultTextBox.Clear();
                                                                sqlResultTextBox.BackColor = Color.Red;
                                                                sqlResultTextBox.Text += "發生錯誤!!! " +
                                                                                         Environment.NewLine;
                                                                sqlResultTextBox.Text += "請重試(八達通號碼 :" + cardId +
                                                                                         ")"+Environment.NewLine;
                                                                sqlResultTextBox.ScrollToCaret();
                                                                sqlResultTextBox.Refresh();
                                                                //boxErrowNotSameCard.Caption =
                                                            }
                                                        }

                                                        else//with 100022 Poll >100000
                                                        {
                                                            var TxnAmtStatus3 = OctDisplayPayAmount(OctValue);
                                                            
                                                            sqlResultTextBox.Clear();
                                                            sqlResultTextBox.BackColor = Color.Red;
                                                            sqlResultTextBox.Text += "發生錯誤!!!" + PollStatus2 + Environment.NewLine;
                                                            sqlResultTextBox.Text += GetErrorMessage(PollStatus2) +
                                                                                     Environment.NewLine;
                                                            sqlResultTextBox.Text +="請重試(八達通號碼 :" + cardId + ")";
                                                            //sqlResultTextBox.ScrollToCaret();
                                                            sqlResultTextBox.Refresh();


                                                            OctopousInitDiplay();
                                                            var TxnAmtStatus2 = OctDisplayPayAmount(OctValue);
                                                            log.Info("XXXX 100022 Poll Error in First 20 secs" +
                                                                     GetErrorMessage(PollStatus2));
                                                        }

                                                        if (DateTime.Now.Subtract(startTime).Seconds >= 28) //After 25 sec timeout
                                                        {
                                                            using (Form newForm4 = new Form())
                                                            {
                                                                newForm4.TopMost = true;
                                                                //newForm4.BackColor = Color.Gray;
                                                              //  newForm4.WindowState = FormWindowState.Maximized;
                                                                //newForm4.Visible = true;
                                                                newForm4.Activate();
                                                                MessageBoxEx msgBoxError100022AfterTimeout =
                                                                    MessageBoxExManager.CreateMessageBox(null);

                                                                msgBoxError100022AfterTimeout.Caption = "八達通錯誤號碼" + balance;
                                                                msgBoxError100022AfterTimeout.Text = GetErrorMessage(PollStatus2) +
                                                                              "\n 請重試(八達通號碼 :" + cardId + ")";

                                                                MessageBoxExButton btnRetry = new MessageBoxExButton();
                                                                btnRetry.Text = "Retry";
                                                                btnRetry.Value = "Retry";


                                                                MessageBoxExButton btnNo = new MessageBoxExButton();
                                                                btnNo.Text = "Cancel";
                                                                btnNo.Value = "Cancel";

                                                                msgBoxError100022AfterTimeout.AddButton(btnRetry);
                                                                msgBoxError100022AfterTimeout.AddButton(btnNo);
                                                                //msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                                                                msgBoxError100022AfterTimeout.Icon = MessageBoxExIcon.Warning;

                                                                // msgBox.SaveResponseText = "Don't ask me again";
                                                                // msgBox.AllowSaveResponse = true;

                                                                msgBoxError100022AfterTimeout.Font = new Font("Tahoma", 11);
                                                                //msgBox.Show(newForm2);
                                                                string result2 = msgBoxError100022AfterTimeout.Show(newForm4);
                                                                //newForm, GetErrorMessage(PollStatus4), "Error Code " + PollStatus4, MessageBoxButtons.RetryCancel);
                                                                switch (result2)
                                                                {
                                                                    case MessageBoxExResult.Retry:
                                                                        {
                                                                            log.Info("***100022 with Retry  by Operator");
                                                                            continue;
                                                                        }

                                                                    case MessageBoxExResult.Cancel:

                                                                        log.Info("***100022 with Cancel  by Operator");
                                                                        OctopousIdleDisplay();
                                                                        var TxnAmtStatus3 = OctDisplayPayAmount(OctValue);
                                                                        OctopousInitDiplay();
                                                                        OctopousIdleDisplay();
                                                                        newForm4.Close();
                                                                        FormCancelAndMinimize();
                                                                        if (timer.Enabled==false)
                                                                        {
                                                                            timer.Start();
                                                                        }
                                                                        goto EndOf100022;

                                                                }

                                                                break;
                                                                //newForm, GetErrorMessage(PollStatus4), "Error Code " + PollStatus4, MessageBoxButtons.RetryCancel);
                                                            }
                                                            
                                                        }

                                                    }//End of For loop

                                                    break;
                                                }


                                        }//End of switch
                                    }
                                EndOf100022:
                                    ;
                                }

                                #endregion

                                #region 100048 Insufficient Funds

                                if (balance == 100048)
                                {
                                    timer.Stop();
                                    log.Warn("XXXX Card do not have sufficient Fund " + GetErrorMessage(balance));

                                    using (Form newForm = new Form()) //Normal MessgeBox
                                    {
                                        //MessageBeep(0 /*MB_OK*/);
                                        newForm.TopMost = true;
                                        newForm.Activate();

                                        MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);
                                        msgBox.AddButtons(MessageBoxButtons.RetryCancel);

                                        msgBox.Caption = GetErrorMessage(balance);
                                        msgBox.Text = GetErrorMessage(balance) + "\n 會否用其他八達通?;";

                                        //msgBox.Timeout = 10000;
                                        //msgBox.TimeoutResult = TimeoutResult.Timeout;

                                        msgBox.Icon = MessageBoxExIcon.Warning;

                                        msgBox.Font = new Font("Microsoft YaHei", 10);

                                        var result = msgBox.Show(newForm);

                                        switch (result)
                                        {
                                            case MessageBoxExResult.Retry:
                                                {
                                                    log.Info("XXXX 100048 , Trying an other card");
                                                    goto FirstPoll;

                                                }

                                            case MessageBoxExResult.Cancel:
                                                {
                                                    var sqlclient = new SqlClient();
                                                    SqlClient.RemoveCurrentRecord(GetDeviceId(), cardId, 0, 999999);
                                                    log.Info("*****100048 Occur Cancel by operator******");
                                                    FormCancelAndMinimize();
                                                    timer.Start();
                                                    OctopousIdleDisplay();
                                                    break;


                                                }

                                        }

                                    }

                                    //string deviceID3 = string.Format("{0:x}", DevVerRec.DevID).ToUpper();

                                    //using (Form newForm = new Form())
                                    //{
                                    //    newForm.TopMost = true;
                                    //    newForm.Activate();
                                    //    var result = MessageBox.Show(newForm,
                                    //        GetErrorMessage(balance) + "\n 會否用其他八達通? ",
                                    //        "Error Code" + balance, MessageBoxButtons.RetryCancel);
                                    //    switch (result)
                                    //    {
                                    //        case DialogResult.Retry:
                                    //            {
                                    //                log.Info("XXXX 100048 , Trying an other card");
                                    //                goto FirstPoll;

                                    //            }

                                    //        case DialogResult.Cancel:
                                    //            {
                                    //                UpdateSQLTable(deviceID3, cardId, 0, 999999);
                                    //                log.Info("*****100048 SQL Updated, cancel by operator******");
                                    //                timer.Start();                                              

                                    //                break;

                                    //            }

                                    //    }
                                    //}


                                }
                                #endregion

                                #region Normal Operation

                                if(balance<100000)  //Normal Operation
                                {

                                    string deviceID2 = string.Format("{0:x}", DevVerRec.DevID).ToUpper();
                                    //var deviceID = device_struct.GetType().GetField("DevID");

                                    log.Info("----Normal SQL Updated----");
                                    var sql = new SqlClient();
                                    
                                    sql.SucessfulTransaactionUpdate(deviceID2, cardId, balance, PollStatus,TransDataTime);
                                    log.Info("----Normal SQL Finishe----");

                                    DisplayTxtbox.BackColor = Color.LightBlue;
                                    sqlResultTextBox.Clear();
                                    sqlResultTextBox.BackColor = Color.White;
                                    sqlResultTextBox.Text += "八達通交易成功" + Environment.NewLine;
                                    sqlResultTextBox.Text += "車牌:" + sqltransaction + Environment.NewLine;
                                    sqlResultTextBox.Text += "八達通號碼 :" + cardId + Environment.NewLine;
                                    sqlResultTextBox.Text += "扣除金額: $" +
                                                             (Convert.ToDecimal(OctValue).ToString("#,##0.00")) +
                                                              Environment.NewLine;
                                    sqlResultTextBox.Text +="餘額: $" + ((Convert.ToDecimal(balance)) / 10).ToString("#,##0.00")
                                    +Environment.NewLine;
                                                                

                                    //(Convert.ToDecimal(PollStatus) / 10).ToString("#,##.0")

                                   sqlResultTextBox.Refresh();
                                   CardEnquirybtn.Enabled = true;
                                   // reader.Close();
                                    if (timer.Enabled == false)
                                    {
                                        timer.Start();
                                    }

                                    

                                }

                                #endregion

                            }
                            else //if there is a error on first poll
                            {
                                log.Warn(string.Format("First Poll Error Code {0} , Message {1}", PollStatus,
                                    GetErrorMessage(PollStatus)));

                                timer.Stop();

                                #region FirstRollWithRetry
                                //100001, 100005,100016,100017, 100021
                                // 100003, 23, 25, 50, 55, 56 No retry
                                if (PollStatus == 100001 || PollStatus == 100005 || PollStatus == 100016 || PollStatus == 100017 || PollStatus == 100021
                                   || PollStatus == 100024 || PollStatus == 100032 || PollStatus == 100034 || PollStatus == 100035 || PollStatus == 100066)
                                {
                                    log.Warn(string.Format("First Poll Error Code {0}", GetErrorMessage(PollStatus)));
                                    using (Form newForm = new Form())
                                    {
                                        newForm.TopMost = true;
                                        newForm.Activate();
                                        MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                                        msgBox.Caption = "Error Code " + PollStatus;
                                        msgBox.Text = GetErrorMessage(PollStatus);

                                        msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                                        msgBox.Icon = MessageBoxExIcon.Question;


                                        // msgBox.SaveResponseText = "Don't ask me again";
                                        // msgBox.AllowSaveResponse = true;

                                        msgBox.Font = new Font("Tahoma", 10);
                                        var result = msgBox.Show(newForm);
                                        //newForm, GetErrorMessage(PollStatus4), "Error Code " + PollStatus4, MessageBoxButtons.RetryCancel);
                                        switch (result)
                                        {
                                            case MessageBoxExResult.Retry:
                                                {
                                                    log.Info("First Poll Error with Retry  by Operator");
                                                    goto FirstPoll;
                                                }


                                            case MessageBoxExResult.Cancel:
                                                log.Info("First Poll Error with Cancel  by Operator");
                                                string deviceID2 = string.Format("{0:x}", DevVerRec.DevID).ToUpper();
                                                var sql = new SqlClient();
                                                sql.RemoveCurrentRecord();
                                                timer.Start();
                                                OctopusLibrary.TxnAmt(0, -30000, 0, 0);
                                                break;

                                        }
                                    }

                                }
                                #endregion

                                #region Frist Poll Block Card

                                else if (PollStatus == 100019)
                                {
                                    log.Info(string.Format("**Block Card {0}  Card ID {1}", PollStatus, "Block Card"));
                                    //using (Form newForm = new Form()) // Display Error
                                    //{
                                    //    newForm.TopMost = true;
                                    //    newForm.Activate();
                                    //    MessageBox.Show(newForm, GetErrorMessage(PollStatus));
                                    //

                                    using (Form newForm9 = new Form()) //MessageBoxTemplate
                                    {
                                        //MessageBeep(0 /*MB_OK*/);
                                        newForm9.TopMost = true;
                                        newForm9.Activate();

                                        MessageBoxEx msgBox9 = MessageBoxExManager.CreateMessageBox(null);

                                        msgBox9.Caption = "八達通發生錯誤";
                                        msgBox9.Text = GetErrorMessage(PollStatus) + "\n錯誤號碼 :" + PollStatus + "\n會否用其他八達通????";

                                        // msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                                        MessageBoxExButton btnRetry = new MessageBoxExButton();
                                        btnRetry.Text = "是";
                                        btnRetry.Value = "Retry";


                                        MessageBoxExButton btnNo = new MessageBoxExButton();
                                        btnNo.Text = "放棄";
                                        btnNo.Value = "Cancel";

                                        msgBox9.AddButton(btnRetry);
                                        msgBox9.AddButton(btnNo);
                                        msgBox9.Icon = MessageBoxExIcon.Warning;

                                        msgBox9.Font = new Font("Microsoft YaHei", 10);

                                        var result = msgBox9.Show(newForm9);
                                        switch (result)
                                        {
                                            case MessageBoxExResult.Retry:
                                                {

                                                    goto FirstPoll;
                                                }

                                            case MessageBoxExResult.Cancel:
                                                var sql = new SqlClient();
                                                sql.RemoveCurrentRecord();
                                                FormCancelAndMinimize();
                                                OctopousInitDiplay();
                                                OctopousIdleDisplay();
                                                timer.Start();
                                                break;

                                        }


                                    }


               




                                }
                                #endregion
                                #region First Poll Error with No Retry
                                //100003, 23, 25, 50, 55, 56
                                else if (PollStatus == 100003 || PollStatus == 100023 || PollStatus == 100025 ||
                                         PollStatus == 100050 || PollStatus == 100055 || PollStatus == 100056 ||
                                         PollStatus == 100051)
                                {
                                    log.Warn(string.Format("First Poll Error Code {0}",
                                        GetErrorMessage(PollStatus)));
                                    using (Form newForm = new Form())
                                    {
                                        newForm.TopMost = true;

                                        newForm.Activate();

                                        MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                                        msgBox.Caption = "Error Code " + PollStatus;
                                        msgBox.Text = GetErrorMessage(PollStatus);

                                        msgBox.AddButtons(MessageBoxButtons.OK);
                                        msgBox.Icon = MessageBoxExIcon.Question;


                                        // msgBox.SaveResponseText = "Don't ask me again";
                                        // msgBox.AllowSaveResponse = true;

                                        msgBox.Font = new Font("Tahoma", 10);
                                        msgBox.Show(newForm);
                                        //newForm, GetErrorMessage(PollStatus4), "Error Code " + PollStatus4, MessageBoxButtons.RetryCancel);
                                        log.Info("First Poll Error with Cancel ny Default");
                                        string deviceID2 =
                                            string.Format("{0:x}", DevVerRec.DevID).ToUpper();
                                        var sql = new SqlClient();
                                        SqlClient.RemoveCurrentRecord();
                                        timer.Start();

                                        OctopousIdleDisplay();
                                    }

                                }

                                #endregion


                            }


                        }

                    }
                }
            }
        }


        private void FormCancelAndMinimize()
        {
            var sql = new SqlClient();
            sql.RemoveCurrentRecord();
            notifyIcon.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon.BalloonTipText = "You have successfully minimized your form.";
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(500);
            this.Hide();
        }

        private static int OctDisplayPayAmount(int OctValue)
        {
            int TxnAmtStatus = OctopusLibrary.TxnAmt(OctValue * 10, -30000, 0, 0);
            return TxnAmtStatus;
        }

        public void OctGUIGoBack_Click(object sender, EventArgs e)
        {
            isBusy = false;
           
            sqlResultTextBox.Clear();
            OctopousIdleDisplay();
            var DeviceID = GetDeviceId();
            OctopousIdleDisplay();
            DisplayTxtbox.Clear();
            var sql = new SqlClient();
            sql.RemoveCurrentRecord();
            notifyIcon.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon.BalloonTipText = "You have successfully minimized your form.";

            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(500);

            if (timer.Enabled != true)
            {
                timer.Start();
            }
            this.Hide();
            if (this.Visible==false)
            {
                log.Info("Form is minized");
            }
            
        }

        private void RestartApplicationbtn_Click(object sender, EventArgs e)
        {
            var rootAppender = ((Hierarchy)LogManager.GetRepository()).Root.Appenders.OfType<FileAppender>().FirstOrDefault();
            string filename = rootAppender != null ? rootAppender.File : string.Empty;

            MessageBox.Show(filename);
        }

        private void button11_Click(object sender, EventArgs e)
        {
           
        }

        private void CardEnquirybtn_Click_1(object sender, EventArgs e)
        {
            log.Info("----- Standard Card Enquiry-----");
            timer.Stop();
        CardEnquiry:
            OctopousInitDiplay();
            sqlResultTextBox.Clear();
            int PollStatus4 = 0;
            StringBuilder PollData = new StringBuilder(512);
            PollStatus4 = OctopusLibrary.Poll(2, 100, PollData);
            if (PollStatus4 < 100000)
            {
                log.Info("----- Standard Card Enquiry-----");
                string cardId = PollData.ToString().Substring(0, 8);
                log.Info("Data  :" + PollData);

                string[] dateStrings = PollData.ToString().Split(',');
                string firstRecordDateTime = dateStrings[5];
                //DateTime convertedTime = new DateTime(DateTime.Parse(firstRecordDateTime).Ticks), DateTimeKind.UTC);
                var startDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var firstRecordDateFormat = startDate.AddSeconds(firstRecordDateTime.ToInt32());

                string secondRecordDateTime = dateStrings[10];
                //DateTime convertedTime = new DateTime(DateTime.Parse(firstRecordDateTime).Ticks), DateTimeKind.UTC);
                var startDate2 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var secondRecordDateFormat = startDate.AddSeconds(secondRecordDateTime.ToInt32());

                string thirdRecordDateTime = dateStrings[15];
                //DateTime convertedTime = new DateTime(DateTime.Parse(firstRecordDateTime).Ticks), DateTimeKind.UTC);
                var startDate3 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var thirdRecordDateFormat = startDate.AddSeconds(thirdRecordDateTime.ToInt32());

                string forthRecordDateTime = dateStrings[20];
                //DateTime convertedTime = new DateTime(DateTime.Parse(firstRecordDateTime).Ticks), DateTimeKind.UTC);
                var startDate4 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var forthRecordDateFormat = startDate.AddSeconds(forthRecordDateTime.ToInt32());
                string something = null;

                //================================record 1=========================================================


                log.Info("Data  :" + PollData);
                sqlResultTextBox.Text += "Octopus no. : " + cardId + Environment.NewLine;
                sqlResultTextBox.Text += @"Octopus card Remaining Value: " +
                                         (Convert.ToDecimal(PollStatus4) / 10).ToString("#,##.0") + Environment.NewLine;
                sqlResultTextBox.Text += " No.        Transaction  Date Time     Amount    Device ID" +
                                         Environment.NewLine;
                sqlResultTextBox.Text += "  1" + "             " + firstRecordDateFormat.ToLocalTime().ToString() +
                                         "   ";
                sqlResultTextBox.Text += "     $" + ((dateStrings[4].ToDecimal()) / 10).ToString("#,##.0") + "       ";
                sqlResultTextBox.Text += 
                   
                    something = CheckString(string.Format("{0:X}", Convert.ToInt32(dateStrings[6])))+


                   string.Format("{0:X}", Convert.ToInt32(dateStrings[6])) + Environment.NewLine;//working
                //string.Format("{0:x}", DevVerRec.DevID).ToUpper()
                //================================record 2=========================================================

                sqlResultTextBox.Text += "  2" + "             " + secondRecordDateFormat.ToLocalTime().ToString() +
                                         "   ";
                sqlResultTextBox.Text += "     $" + ((dateStrings[9].ToDecimal()) / 10).ToString("#,##.0") + "       ";
                sqlResultTextBox.Text += 
                    
                     something = CheckString(string.Format("{0:X}", Convert.ToInt32(dateStrings[11]))) +


                     string.Format("{0:X}", Convert.ToInt32(dateStrings[11]))+ Environment.NewLine;
                //================================record 3=========================================================

                sqlResultTextBox.Text += "  3" + "             " + thirdRecordDateFormat.ToLocalTime().ToString() +
                                         "   ";
                sqlResultTextBox.Text += "     $" + ((dateStrings[14].ToDecimal())/10).ToString("#,##.0") + "       ";
                sqlResultTextBox.Text +=
                      something = CheckString(string.Format("{0:X}", Convert.ToInt32(dateStrings[16]))) +

                          string.Format("{0:X}", Convert.ToInt32(dateStrings[16])) + Environment.NewLine;

                //sqlResultTextBox.Text += string.Format("{0:X}", Convert.ToInt32(dateStrings[16])) + Environment.NewLine;
                //================================record 4=========================================================
                sqlResultTextBox.Text += "  4" + "             " + forthRecordDateFormat.ToLocalTime().ToString() +
                                         "   ";
                sqlResultTextBox.Text += "     $" + ((dateStrings[19].ToDecimal()) / 10).ToString("#,##.0") + "       ";
                sqlResultTextBox.Text +=
                    something = CheckString(string.Format("{0:X}", Convert.ToInt32(dateStrings[21]))) +
                                string.Format("{0:X}", Convert.ToInt32(dateStrings[21])) + Environment.NewLine;

                 //string.Format("{0:X}", Convert.ToInt32(dateStrings[21])) + Environment.NewLine;


                sqlResultTextBox.Refresh();
                OctopousDisplayBalanace(PollStatus4);
            }

            else
            {
                using (Form newForm = new Form()) //MessageBoxTemplate
                {
                    //MessageBeep(0 /*MB_OK*/);
                    newForm.TopMost = true;
                    newForm.Activate();

                    MessageBoxEx msgBox = MessageBoxExManager.CreateMessageBox(null);

                    msgBox.Caption = "八達通發生錯誤";
                    msgBox.Text = GetErrorMessage(PollStatus4) + "\n錯誤號碼 :" + PollStatus4;

                    // msgBox.AddButtons(MessageBoxButtons.RetryCancel);
                    MessageBoxExButton btnRetry = new MessageBoxExButton();
                    btnRetry.Text = "重試";
                    btnRetry.Value = "Retry";


                    MessageBoxExButton btnNo = new MessageBoxExButton();
                    btnNo.Text = "放棄";
                    btnNo.Value = "Cancel";

                    msgBox.AddButton(btnRetry);
                    msgBox.AddButton(btnNo);
                    msgBox.Icon = MessageBoxExIcon.Warning;

                    msgBox.Font = new Font("Microsoft YaHei", 10);

                    var result = msgBox.Show(newForm);
                    switch (result)
                    {
                        case MessageBoxExResult.Retry:
                            {

                                goto CardEnquiry;
                            }

                        case MessageBoxExResult.Cancel:
                            OctopousInitDiplay();
                            OctopousIdleDisplay();
                            break;

                    }


                }

            }

            timer.Start();
        }

        private string CheckString(string format)
        {
            if (format == GetDeviceId().Substring(2,4))
            {
                return "##";
            }
            else
            {
                return "";
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            string curFile = "error.txt";
            MessageBox.Show(File.Exists(curFile) ? "File exists." : "File does not exist.");

        }
    }
}
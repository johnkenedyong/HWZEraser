using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using ScrapperMinDLL;

namespace HWZEraser
{
    public partial class Form1 : Form
    {
        public Thread RunningThread = null;
        public string Scripts = @"ERRORRESUME('1');
REM('TITLE : HWZ Auto Bump');
REM('WIDTH : 400');
REM('PARAM 0 : HWZ Username : 20');
REM('PARAM 1 : HWZ Password : 20');
SET('User', '{PARAM0}');
SET('Pass', '{PARAM1}');
SET('PG', WC_GetPage('https://secureforums.hardwarezone.com.sg/logon.php', ''));
SET('PS', WC_GetPostStringRaw('https://secureforums.hardwarezone.com.sg/logon.php', GET('PG'), '1', FORMAT('vb_login_username={0}&vb_login_md5password_utf={1}&vb_login_md5password={2}', GET('User'), SO_GetMd5Hash(GET('Pass')),SO_GetMd5Hash(GET('Pass'))), ''));
SET('PG2', WC_PostPageComplete(WC_GetLastActionUrl(), GET('PS'), 'http://forums.hardwarezone.com.sg'));
IF(GET('PG2'), 'CONTAINS', 'Thank you for logging', 
    LOG('Successfully Login');
    SET('PG2', WC_MethodPage('http://forums.hardwarezone.com.sg/eat-drink-man-woman-16/', 'GET', '', ''));
    SET('UID', SO_TagMatch(GET('PG2'), ',""user"":{""id"":""', '""'));
    SET('UR', JOIN('', 'http://forums.hardwarezone.com.sg/search.php?do=finduser&u=', GET('UID')));
    SET('PG0', WC_MethodPageStop(GET('UR'), 'GET', '', JOIN('', 'Upgrade-Insecure-Requests=1&referer=', GET('UR'))));
    SET('SID', SO_TagMatch(GET('PG0'), '/search.php?searchid=', '&'));
    SET('URL', JOIN('', 'http://forums.hardwarezone.com.sg/search.php?searchid=', GET('SID')));
    SET('URLX', JOIN('', GET('URL')));
    SET('PG3', WC_MethodPage(GET('URLX'), 'GET', '', ''));
    SET('SID', SO_TagMatch(GET('PG3'), 'searchid=', '&'));
    SET('URL', JOIN('', 'http://forums.hardwarezone.com.sg/search.php?searchid=', GET('SID')));
    SET('I', '1');
    WHILE('1', '=', '1',
       SET('URLX', JOIN('', GET('URL'), '&pp=25&page=', GET('I')));
       SET('PG3', WC_MethodPage(GET('URLX'), 'GET', '', ''));
       IF(GET('PG3'), 'CONTAINS', 'requires that you wait', LOG('WAIT'); SLEEP('15'); CONTINUE(), PASS());       
       SET('STRS', SO_TagMatch(GET('PG3'), '#post', '""'));
       SET('HAS', '0');
       IF(COUNT(GET('STRS')), '<=', '0', EXIT('Finish'),
          FOR('STR', GET('STRS'),
             SET('ISDONE', SO_TagMatch(GET('PG3'), JOIN('', '#post', GET('STR'), '"">'), '<'));
             IF(GET('ISDONE'), 'CONTAINS', 'edited', LOG('Skip');CONTINUE(), PASS());
             SET('URL2', JOIN('', 'http://forums.hardwarezone.com.sg/editpost.php?do=editpost&p=', GET('STR')));
             SET('PG4', WC_MethodPage(GET('URL2'), 'GET', '', ''));
             IF(GET('PG4'), 'CONTAINS', 'This thread is closed', LOG('Closed'); CONTINUE(), PASS());

             SET('TOKEN', SO_TagMatchSingle(GET('PG4'), 'name=""securitytoken""', 'value=""', '""'));
             SET('URL3', JOIN('', 'http://forums.hardwarezone.com.sg/editpost.php?do=deletepost&p=', GET('STR')));
             SET('PS2', JOIN('', 's=&securitytoken=', GET('TOKEN'), '&p=', GET('STR'), '&url=index.php&do=deletepost&deletepost=delete&reason=it+is+best+not+to+say+too+much'));
             SET('PG5', WC_MethodPage(GET('URL3'), 'POST', GET('PS2'), ''));
             IF(GET('PG5'), 'CONTAINS', 'you do not have permission to access', 
                SET('URL2', JOIN('', 'http://forums.hardwarezone.com.sg/editpost.php?do=editpost&p=', GET('STR')));
                SET('PG4', WC_MethodPage(GET('URL2'), 'GET', '', ''));
                SET('TOKEN', SO_TagMatchSingle(GET('PG4'), 'name=""securitytoken""', 'value=""', '""'));
                SET('URL3', JOIN('', 'http://forums.hardwarezone.com.sg/editpost.php?do=updatepost&p=', GET('STR')));
                SET('PS2', JOIN('', 's=&securitytoken=', GET('TOKEN'), '&p=', GET('STR'), 'reason=&title=edited&message=edited+for+good&wysiwyg=0&iconid=0&do=updatepost&posthash=&poststarttime=&sbutton=Save+Changes&parseurl=1&emailupdate=9999'));
                SET('PG5', WC_MethodPage(GET('URL3'), 'POST', GET('PS2'), ''));
                IF(GET('STR'), '=', '105511711', EXIT(GET('PG5')), PASS());
                IF(GET('PG5'), 'CONTAINS', 'you do not have permission to access', 
                   LOG('Fail')
                , 
                   IF(GET('PG5'), 'CONTAINS', 'This thread is closed', 
                      LOG('Unable to Delete Post due to Thread Closed');
                   , SET('HAS', '1');LOG('1 Post Deleted'));
                );
             , 
                IF(GET('PG5'), 'CONTAINS', 'This thread is closed', 
                   LOG('Unable to Delete Post due to Thread Closed');
                , SET('HAS', '1');LOG('1 Post Deleted'));
             );
          );
       );
       IF(GET('HAS'), '=', '0', SET('I', MI_Add(GET('I'), '1')), PASS());
    );
, EXIT('Invalid Login'));";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Stop")
            {
                button1.Text = "Erase && Wait";
                Stop();
            }
            else
            {
                if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Please input username and password");
                    return;
                }
                button1.Text = "Stop";
                Start();
            }
        }

        private List<string> DataList = new List<string>();
        private void Start()
        {
            DataList.Clear();
            DataList.Add(txtUsername.Text);
            DataList.Add(txtPassword.Text);

            ThreadStart ts = new ThreadStart(RunThread);
            RunningThread = new Thread(ts);
            RunningThread.Start();
        }

        private void RunThread()
        {
            ScrapperMin sm = ScrapperMin.InitWC();
            sm.Log = new Action<string>(AddLog);
            string[] result = sm.Multiple(Scripts, DataList);
        }

        private void AddLog(string str)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), str);
                return;
            }
            rtbErase.AppendText(str + "\r\n");
        }

        private void Stop()
        {
            if (RunningThread != null)
            {
                try
                {
                    RunningThread.Abort();
                    RunningThread = null;
                }
                catch { }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
            Environment.Exit(0);
        }
    }
}

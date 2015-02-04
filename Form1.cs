using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AutoShort
{
    public partial class Form1 : Form
    {
        IntPtr nextClipboardViewer;

        [DllImport("User32.dll")]
        protected static extern int
            SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool
               ChangeClipboardChain(IntPtr hWndRemove,
                                    IntPtr hWndNewNext);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg,
                                             IntPtr wParam,
                                             IntPtr lParam);

        protected override void
          WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (Clipboard.ContainsText() && Regex.IsMatch(Clipboard.GetText(), @"((https?:)?//)?(([\d\w]|%[a-fA-f\d]{2,2})+(:([\d\w]|%[a-fA-f\d]{2,2})+)?@)?([\d\w][-\d\w]{0,253}[\d\w]\.)+[\w]{2,63}(:[\d]+)?(/([-+_~.\d\w]|%[a-fA-f\d]{2,2})*)*(\?(&?([-+_~.\d\w]|%[a-fA-f\d]{2,2})=?)*)?(#([-+_~.\d\w]|%[a-fA-f\d]{2,2})*)?"))
                    {
                        if (Clipboard.GetText().StartsWith("http://short.tjhorner.com") || Clipboard.GetText().StartsWith("http://puu.sh"))
                        {
                            break;
                        }
                        using (var wb = new WebClient())
                        {
                            var data = new NameValueCollection();
                            data["url"] = Clipboard.GetText();

                            label2.Text = "last url shortened: " + Clipboard.GetText();

                            byte[] response = wb.UploadValues("http://short.tjhorner.com/create.json", "POST", data);

                            string strRes = Encoding.UTF8.GetString(response);

                            JObject res = JObject.Parse(strRes);

                            if ((bool)res.GetValue("success"))
                            {
                                Clipboard.SetText("http://short.tjhorner.com/" + res.GetValue("shortId"));
                            }
                        }
                    }
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam,
                                m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam,
                                    m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public Form1()
        {
            InitializeComponent();
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int) this.Handle);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}

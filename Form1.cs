using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Win32;
using System.Diagnostics;

namespace VLC_HTTP_Launcher
{
    public partial class Form1 : Form
    {
        double Version = 2.0;
        public int defSelection, changedCombobox;
        public string urlHistory, urlFirst, userPass, userName, path, regLoc, urlS, urlHidden, tmpPL, networkCValue;
        public bool userWarned;
        string[] saveLines, linkLines, playLines = new string [1];
        string[] serverList = new string[1], userNames = new string[1], userPasswords = new string[1];
        public HttpWebRequest httpRequest;

        public Form1()
        {
            InitializeComponent();
            this.listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.myListBox_DrawItem);
            Array.Resize<string>(ref playLines, 0);
            Array.Resize<string>(ref serverList, 0);
            Array.Resize<string>(ref userNames, 0);
            Array.Resize<string>(ref userPasswords, 0);
            comboBox1.Items.Add("<Add Server>");
        }

        public void AutoUpdate()
        {
            //
        }

        private void myListBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            e.DrawBackground();
            Font myFont;
            Brush myBrush;
            int i = e.Index;
            myFont = e.Font;
            
            try
            {
                if (StringContainsMedia(linkLines[e.Index]))
                {
                    myBrush = Brushes.Green;
                }
                else
                {
                    myBrush = Brushes.Blue;
                }
                e.Graphics.DrawString(listBox1.Items[i].ToString(), myFont, myBrush, e.Bounds, StringFormat.GenericDefault);
            }
            catch
            {

            }
        }

        public string GetDirectoryFilesRegexForUrl(string url)
        {
            return "<a href=\".*\">(?<name>.*)</a>";
        }

        public string GetDirectoryLinksRegexForUrl(string url)
        {
            return "<a href=\"(?<name>.*).*\">";
        }

        public void GetDirectories(string urls)
        {
            listBox1.Items.Clear();
            urlHidden = urlHidden.TrimEnd('/');
            urlHidden = urlHidden + "/";
            //MessageBox.Show(urlHidden);
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(urlHidden);
                httpRequest.Credentials = new NetworkCredential(userNames[comboBox1.SelectedIndex], userPasswords[comboBox1.SelectedIndex]);
            }
            catch
            {
                MessageBox.Show("Error connecting to server");
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryFilesRegexForUrl(urlHidden));
                        Regex regexLinks = new Regex(GetDirectoryLinksRegexForUrl(urlHidden));
                        MatchCollection matches = regex.Matches(html);
                        MatchCollection linkMatches = regexLinks.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    listBox1.Items.Add(match.Groups["name"].ToString().TrimEnd('/'));
                                }
                            }
                            listBox1.Items[0] = "../";
                        }
                        if (linkMatches.Count > 0)
                        {
                            List<string> ls = new List<string>();

                            foreach (Match match in linkMatches)
                            {
                                if (match.Success)
                                {
                                    ls.Add(match.Groups["name"].ToString().TrimEnd('/'));
                                }
                            }
                            linkLines = ls.ToArray();
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error: Unsupported File Type");
                MoveUpDirectory("both", false);
                GetDirectories(urlHidden);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            urlHistory = comboBox1.SelectedItem.ToString();
            urlFirst = comboBox1.SelectedItem.ToString();
            urlHidden = comboBox1.SelectedItem.ToString();
            GetDirectories(urlHidden);
        }

        public void MoveUpDirectory(string type, bool skipRefresh)
        {
            if (type == "link" || type == "both")
            {
                urlHistory = urlHidden;
                urlHistory = urlHistory.TrimEnd('/');
                int index2 = urlHistory.LastIndexOf("/");
                if (urlHistory.Length > 0)
                {
                    urlHistory = urlHistory.Substring(0, index2);
                }
                urlHidden = urlHistory + "/";
            }
            if (!skipRefresh)
            {
                GetDirectories(urlHidden);
            }

        }

        public bool StringContainsMedia(string s)
        {
            string[] fileTypes = { ".mkv", ".mp4", ".avi", "m4v", ".vob", ".mp3", ".wmv", ".mpeg" };

            for (int x = 0; x < fileTypes.Length - 1; x++)
            {
                if (s.Contains(fileTypes[x]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool BoxSelectedItemContainsMedia(ListBox box, string[] linkBox)
        {
            string[] fileTypes = { ".mkv", ".mp4", ".avi", "m4v", ".vob", ".mp3", ".wmv", ".mpeg" };

            for (int x = 0; x < fileTypes.Length - 1; x++)
            {
                if (linkLines[box.SelectedIndex].Contains(fileTypes[x]))
                {
                    return true;
                }
            }
            return false;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (BoxSelectedItemContainsMedia(listBox1, linkLines))
            {
                if (!urlHidden.Contains(linkLines[listBox1.SelectedIndex]))
                {
                    urlHidden = urlHidden + linkLines[listBox1.SelectedIndex];
                }
                if (checkBox2.Checked)
                {
                    Array.Resize<string>(ref playLines, playLines.Length + 1);
                    if (StringContainsMedia(linkLines[listBox1.SelectedIndex]))
                    {
                        playLines[playLines.Length - 1] = urlHidden;
                        listBox2.Items.Add(listBox1.SelectedItem);
                    }
                }
                else
                {
                    CheckNetworkCacheValue();
                    LaunchFile();
                }
                MoveUpDirectory("link", true);
            }
            else
            {
                if (listBox1.SelectedIndex == 0)
                {
                    MoveUpDirectory("both", false);
                }
                else
                {
                    urlHidden = urlHidden + linkLines[listBox1.SelectedIndex];
                    GetDirectories(urlHidden);
                }
            }
        }

        public void CheckNetworkCacheValue()
        {
            path = string.Format(@"C:\Users\{0}\AppData\Roaming\vlc\vlcrc", Environment.UserName);
            if (!(textBox4.Text == networkCValue) && (System.IO.File.Exists(path)))
            {
                System.IO.StreamReader vlcrcfile = new System.IO.StreamReader(path);
                string tmpS = vlcrcfile.ReadToEnd();
                vlcrcfile.Close();
                textBox4.Text = Regex.Replace(textBox4.Text, "[^0-9]", "");
                if (tmpS.Contains("#network-c"))
                {
                    tmpS = tmpS.Replace("#network-c", "network-c");
                }

                tmpS = tmpS.Replace("k-caching=" + networkCValue.ToString(), "k-caching=" + textBox4.Text + Environment.NewLine);
                
                //Clipboard.SetText(tmpO);
                File.Delete(path);
                System.IO.File.WriteAllText(path, tmpS);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            path = string.Format(@"C:\Users\{0}\AppData\Roaming\vlc\vlcrc", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader vlcrcfile = new System.IO.StreamReader(path);
                string tmpS = vlcrcfile.ReadToEnd();
                int index = tmpS.LastIndexOf("network-caching=");
                int index2 = tmpS.IndexOf("# Clock reference average counter");
                string txt1 = tmpS.Substring(index + 16, (index2 - (index + 16)));
                networkCValue = txt1;
                //MessageBox.Show(txt1);
                vlcrcfile.Close();
            }

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\V.txt", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);

                string line;
                int counter = 0;

                while ((line = file.ReadLine()) != null)
                {
                    switch (counter)
                    {
                        case 0:
                            userWarned = Convert.ToBoolean(line);
                            break;
                        case 1:
                            checkBox1.Checked = Convert.ToBoolean(line);
                            break;
                        case 2:
                            checkBox2.Checked = Convert.ToBoolean(line);
                            break;
                        case 3:
                            textBox4.Text = line;
                            textBox4.Text = Regex.Replace(textBox4.Text, "[^0-9]", "");
                            if (!(textBox4.Text == networkCValue))
                            {
                                CheckNetworkCacheValue();
                            }
                            if (textBox4.Text == "")
                            {
                                textBox4.Text = "5000";
                            }
                            break;
                        case 4:
                            line = Regex.Replace(line, "[^0-9]", "");
                            if (line != null)
                            {
                                defSelection = Convert.ToInt32(line);
                            }
                            break;
                    }
                    counter++;
                }

                file.Close();
            }
            else
            {
                string subPath = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\", Environment.UserName);
                System.IO.Directory.CreateDirectory(subPath);
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\V.txt", Environment.UserName);
                System.IO.File.WriteAllText(path, "");
            }

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\S.txt", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);

                string line;
                int counter = 0;

                while ((line = file.ReadLine()) != null)
                {
                    Array.Resize<string>(ref serverList, counter + 1);
                    serverList[counter] = line;
                    counter++;
                }

                file.Close();

                comboBox1.Items.Clear();
                for (int i = 0; i < serverList.Length; i++)
                {
                    comboBox1.Items.Add(serverList[i]);
                }
                comboBox1.Items.Add("<Add Server>");

                if (checkBox1.Checked && (serverList.Length > 0))
                {
                    comboBox1.SelectedIndex = comboBox1.FindStringExact(serverList[defSelection]);
                }
            }
            else
            {
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\S.txt", Environment.UserName);
                System.IO.File.WriteAllText(path, "");
            }

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\U.txt", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);

                string line;
                int counter = 0;

                while ((line = file.ReadLine()) != null)
                {
                    Array.Resize<string>(ref userNames, counter + 1);
                    userNames[counter] = line;
                    counter++;
                }

                file.Close();
            }
            else
            {
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\U.txt", Environment.UserName);
                System.IO.File.WriteAllText(path, "");
            }

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\P.txt", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);

                string line;
                int counter = 0;

                while ((line = file.ReadLine()) != null)
                {
                    Array.Resize<string>(ref userPasswords, counter + 1);
                    userPasswords[counter] = line;
                    counter++;
                }

                file.Close();
            }
            else
            {
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\P.txt", Environment.UserName);
                System.IO.File.WriteAllText(path, "");
            }

            AutoUpdate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\V.txt", Environment.UserName);
            if (!System.IO.File.Exists(path))
            {
                string subPath = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\", Environment.UserName);
                System.IO.Directory.CreateDirectory(subPath);
            }

            saveLines = new string[5];

            if (checkBox1.Checked)
            {
                defSelection = comboBox1.SelectedIndex;

                saveLines[0] = userWarned.ToString();
                saveLines[1] = checkBox1.Checked.ToString();
                saveLines[2] = checkBox2.Checked.ToString();
                saveLines[3] = textBox4.Text;
                saveLines[4] = defSelection.ToString();

                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\V.txt", Environment.UserName);
                System.IO.File.WriteAllLines(path, saveLines);

                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\S.txt", Environment.UserName);
                System.IO.File.WriteAllLines(path, serverList);

                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\U.txt", Environment.UserName);
                System.IO.File.WriteAllLines(path, userNames);

                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\P.txt", Environment.UserName);
                System.IO.File.WriteAllLines(path, userPasswords);
            }
            else
            {
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\V.txt", Environment.UserName);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\S.txt", Environment.UserName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\U.txt", Environment.UserName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\P.txt", Environment.UserName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\Playlist.xspf", Environment.UserName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public void LaunchFile()
        {
            string urlLink = urlHidden;
            urlLink = urlLink.Replace(" ", "%20");
            urlLink = urlLink.Replace("(", "%28");
            urlLink = urlLink.Replace(")", "%29");
            urlLink = urlLink.Replace("[", "%5b");
            urlLink = urlLink.Replace("]", "%5d");
            urlLink = urlLink.Remove(0, 7);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C vlc.exe " + "http://" + userNames[comboBox1.SelectedIndex] + ":" + userPasswords[comboBox1.SelectedIndex] + "@" + urlLink;
            process.StartInfo = startInfo;
            process.Start();
            //Clipboard.SetText("/C vlc.exe " + "http://" + textBox2.Text + ":" + textBox3.Text + "@" + urlLink);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LaunchFile();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!userWarned)
            {
                MessageBox.Show("Warning this option will save your password in plain text in a text file!");
                userWarned = true;
            }
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) 
            {
                int index = listBox2.IndexFromPoint(e.X, e.Y);
                if (index != ListBox.NoMatches)
                {
                    listBox2.SelectedIndex = index;
                    listBox2.Items.RemoveAt(index);
                    List<string> tmpList;
                    tmpList = playLines.ToList();
                    tmpList.RemoveAt(index);
                    Array.Resize<string>(ref playLines, playLines.Length - 1);
                    playLines = tmpList.ToArray();
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                listBox2.Visible = true;
                button6.Visible = true;
                listBox1.Width = 232;
                listBox1.Update();
            }
            else
            {
                listBox2.Visible = false;
                button6.Visible = false;
                listBox1.Width = 446;
                listBox1.Update();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CheckNetworkCacheValue();
            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\Variables.txt", Environment.UserName);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            tmpPL = @"<?xml version=""1.0"" encoding=""UTF-8""?><playlist xmlns=""http://xspf.org/ns/0/"" xmlns:vlc=""http://www.videolan.org/vlc/playlist/ns/0/"" version=""1""><title>Playlist</title><trackList>";
            for (int x = 0; x < playLines.Length; x++)
            {
                tmpPL += "<track><location>http://" + userNames[comboBox1.SelectedIndex] + ":" + userPasswords[comboBox1.SelectedIndex] + "@" + playLines[x].Remove(0, 7) + "</location>";
                tmpPL += string.Format(@"<extension application=""http://www.videolan.org/vlc/playlist/0""><vlc:id>{0}</vlc:id></extension></track>", x);
            }
            tmpPL += @"</trackList><extension application=""http://www.videolan.org/vlc/playlist/0"">";
            for (int x = 0; x < playLines.Length; x++)
            {
                tmpPL += string.Format(@"<vlc:item tid=""{0}""/>", x);
            }
            tmpPL += "</extension></playlist>";

            path = string.Format(@"C:\Users\{0}\AppData\Roaming\VLC-HTTP-Launcher\Playlist.xspf", Environment.UserName);
            string[] s = { tmpPL };
            System.IO.File.WriteAllLines(path, s);
            startInfo.Arguments = "/C vlc.exe " + path;
            process.StartInfo = startInfo;
            process.Start();
            //Clipboard.SetText(tmpPL);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "<Add Server>")
            {
                Form2 addForm = new Form2(this);
                addForm.Show();
            }
            else if (changedCombobox > 0)
            {
                urlHistory = comboBox1.SelectedItem.ToString();
                urlFirst = comboBox1.SelectedItem.ToString();
                urlHidden = comboBox1.SelectedItem.ToString();
                GetDirectories(urlHidden);
            }
            changedCombobox++;
        }

        public void AddServer(string server, string user, string pass)
        {
            int l = serverList.Length;
            Array.Resize<string>(ref serverList, l + 1);
            serverList[l] = server;

            comboBox1.Items.Clear();
            for (int i = 0; i < serverList.Length; i++)
            {
                comboBox1.Items.Add(serverList[i]);
            }
            comboBox1.Items.Add("<Add Server>");

            l = userNames.Length;
            Array.Resize<string>(ref userNames, l + 1);
            userNames[l] = user;

            l = userPasswords.Length;
            Array.Resize<string>(ref userPasswords, l + 1);
            userPasswords[l] = pass;

            comboBox1.SelectedIndex = comboBox1.FindStringExact(serverList[serverList.Length - 1]);
        }
    }
}
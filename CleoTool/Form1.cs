using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static CleoTool.CleoToolDBDataSet;

namespace CleoTool
{
    public partial class Form1 : Form
    {
        static string DK_COLOR = "#C41E3A";
        static string DRUID_COLOR = "#FF7C0A";
        static string HUNTER_COLOR = "#AAD372";
        static string MAGE_COLOR = "#69CCF0";
        static string PALY_COLOR = "#F48CBA";
        static string PRIEST_COLOR = "#FFFFFF";
        static string ROGUE_COLOR = "#FFF468";
        static string SHAMAN_COLOR = "#2459FF";
        static string WARLOCK_COLOR = "#9482C9";
        static string WARRIOR_COLOR = "#C69B6D";


        CElement cParent;
        CleoLootListConfig lootListconfig = new CleoLootListConfig();
        CleoLists cl = new CleoLists();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initConfig();
            
            foreach(SettingsProperty prop in Properties.Settings.Default.Properties)
            {
                comboBox3.Items.Add(prop.Name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initConfig();
        }

        private void initConfig()
        {
            cParent = new CElement("Root");
            cl = new CleoLists();
            lootListconfig = new CleoLootListConfig();
            List<String> players = new List<string>();

            //parse lua data
            try
            {
                String rawTxt = System.IO.File.ReadAllText(Properties.Settings.Default.CleoConfigLoc);
                rawTxt = rawTxt.Replace("\n", "");
                rawTxt = rawTxt.Replace("\r", "");
                rawTxt = rawTxt.Replace("\t", "");
                //parse lua data
                if (!rawTxt.StartsWith("Cleo_DB"))
                    throw new Exception("Invalid Cleo Config File - " + Properties.Settings.Default.CleoConfigLoc);
                cParse4(rawTxt, cParent);
                //build treeview
                TreeNode root = new TreeNode(cParent.ToString());
                buildTreeNodes(root, cParent);
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(root);
                toolStripStatusLabel1.Text = "-";
            }
            catch (Exception ex)
            {
                TreeNode root = new TreeNode("Error : " + ex.Message);
                buildTreeNodes(root, cParent);
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(root);
                toolStripStatusLabel1.Text = ex.Message;
            }

            //build list data
            try
            {
                cl.buildPlayerList(CConfig.findElement(Properties.Settings.Default.ToonCache, cParent), CConfig.findElement(Properties.Settings.Default.AltMapping, cParent), CConfig.findElement(Properties.Settings.Default.ActivePlayerList, cParent));

                lootListconfig.LootListsConfig = CConfig.findElement(Properties.Settings.Default.LootConfigs, cParent).children.ToArray();
                lootListconfig.LootLists = CConfig.findElement(Properties.Settings.Default.LootLists, cParent).children.ToArray();
                comboBox1.Items.Clear();
                foreach (CElement llConfig in lootListconfig.LootListsConfig)
                {
                    comboBox1.Items.Add(llConfig.findAtt("name"));
                }
            }
            catch (Exception ex)
            {
                comboBox1.Items.Add(ex.Message);
            }

            
        }

        private void cParse(String doc, CElement parent)
        {
            int cIndex = 0;
            while (cIndex < doc.Length)
            {
                char cChar = doc[cIndex];
                if (doc[cIndex] == '[')
                {
                    int endBracket = doc.IndexOf(']');
                    String name = doc.Substring(cIndex + 2, endBracket - cIndex - 3);
                    CElement nEle = new CElement(name);
                    char nextParam = doc[endBracket + 4];

                    if (nextParam == '{')
                    {
                        cParse(doc.Substring(endBracket + 1), nEle);
                        String tDoc = doc.Substring(doc.IndexOf('}') + 1);
                        //cIndex = doc.IndexOf('}') + 1;
                        doc = doc.Substring(doc.IndexOf('}') + 1);
                        cIndex = -1;
                    }
                    else
                    {
                        String val = doc.Split('"')[0];
                        nEle.Value = val;
                    }
                    parent.addChild(nEle);
                }
                else if (doc[cIndex] == '}')
                    //break;
                    return;
                cIndex++;
            }
        }

        private string cParse2(String doc, CElement parent)
        {
            int cIndex = 0;
            while (cIndex < doc.Length)
            {
                char cChar = doc[cIndex];
                if (doc[cIndex] == '-' && doc[cIndex + 1] == '-')
                {
                    doc = doc.Substring(doc.IndexOf(']') + 1);
                    cIndex = -1;
                }
                else if (doc[cIndex] == '[')
                {
                    int endBracket = doc.IndexOf(']');
                    String name = doc.Substring(cIndex + 2, endBracket - cIndex - 3);
                    CElement nEle = new CElement(name);
                    char nextParam = doc[endBracket + 4];

                    if (nextParam == '{')
                    {
                        doc = cParse2(doc.Substring(endBracket + 1), nEle);
                        String tDoc = doc.Substring(doc.IndexOf('}') + 1);
                        //cIndex = doc.IndexOf('}') + 1;
                        //doc = doc.Substring(doc.IndexOf('}') + 1);
                        cIndex = -1;
                    }
                    else
                    {
                        String key = doc.Split('"')[1];
                        String val = doc.Split('"')[3];
                        doc = doc.Trim(',');
                        doc = doc.Substring(doc.IndexOf(','));
                        cIndex = -1;
                        nEle.Key = key;
                        nEle.Value = val;
                    }
                    parent.addChild(nEle);
                }
                else if(doc[cIndex] == ',')
                {
                    String[] next = doc.Substring(cIndex).Split(',');
                    String nextStr = next[1];
                    if (nextStr.Contains("--"))
                        nextStr = nextStr.Substring(nextStr.IndexOf("]") + 1);
                    //if (!nextStr.Contains("}") && !nextStr.Contains("["))
                    //{
                    //    int t = 0;
                    //}
                }
                else if (doc[cIndex] == '}')
                    //break;
                    return doc.Substring(cIndex + 1);
                cIndex++;
            }
            return "";
        }

        private string cParse3(String doc, CElement parent)
        {
            int cIndex = 0;
            while (cIndex < doc.Length)
            {
                char cChar = doc[cIndex];

                //parse child objects
                if (doc[cIndex] == '{')
                {
                    
                    String name = "NONE";

                    if (doc.IndexOf('=') < cIndex)
                    {
                        String nameStr = doc.Remove(cIndex);
                        String[] text = nameStr.Split('=');
                        char[] trimChars = new char[] { ' ', '}', '{', ',', '[', ']', '"' };
                        name = text[text.Length - 2].Trim(trimChars);                        
                    }

                    CElement nEle = new CElement(name);
                    //recurse child then advance parser
                    doc = cParse3(doc.Substring(cIndex + 1), nEle);
                    cIndex = -1;

                    parent.addChild(nEle);
                }
                //exit child object
                else if (doc[cIndex] == '}')
                {                    
                    return doc.Substring(cIndex + 1);
                    
                }
                //parse child attributes
                else if (doc[cIndex] == ',')
                {
                    int nextEq = doc.IndexOf('=');
                    int nextEnd = doc.IndexOf('}');
                    String inner = doc.Remove(nextEnd);
                    //if attribute is key / value pair
                    if (doc[nextEq + 2] != '{')
                    {
                        String aInner = doc.Remove(cIndex);
                        cParseObj(aInner, parent);
                        //advance parser
                        doc = doc.Substring(doc.IndexOf(',') + 1);
                        cIndex = -1;
                    }
                    //if attribute is list item
                    else if (!(inner.Contains("=") || inner == "," || inner == ""))
                    {
                        //if (inner.Contains("\""))
                        //{
                        //    int t4 = 0;
                        //}
                        cParseObj(inner.Split(',')[0], parent);
                        //advance parser
                        doc = doc.Substring(doc.IndexOf(',') + 1);                        
                        cIndex = -1;
                    }
                }
                cIndex++;
            }
            return "";
        }

        private string cParse4(String doc, CElement parent)
        {
            int cIndex = 0;
            while (cIndex < doc.Length)
            {
                char cChar = doc[cIndex];

                //parse child objects
                if (doc[cIndex] == '{')
                {

                    String name = "NONE";

                    if (doc.IndexOf('=') < cIndex)
                    {
                        String nameStr = doc.Remove(cIndex);
                        String[] text = nameStr.Split('=');
                        char[] trimChars = new char[] { ' ', '}', '{', ',', '[', ']', '"' };
                        name = text[text.Length - 2].Trim(trimChars);
                        
                    }

                    CElement nEle = new CElement(name);
                    //recurse child then advance parser
                    doc = cParse4(doc.Substring(cIndex + 1), nEle);
                    cIndex = -1;

                    parent.addChild(nEle);
                }
                //exit child object
                else if (doc[cIndex] == '}')
                {
                    return doc.Substring(cIndex + 1);

                }
                //parse child attributes
                else if (doc[cIndex] == ',')
                {
                    int nextEq = doc.IndexOf('=');
                    int nextEnd = doc.IndexOf('}');
                    String inner = doc.Remove(nextEnd);
                    //if attribute is key / value pair
                    if (doc[nextEq + 2] != '{')
                    {
                        if (doc[nextEq + 2] == '"' && doc.IndexOf('=') < doc.IndexOf("}"))
                        {
                            
                            int startQu = nextEq + 2;
                            String innerQu = doc.Substring(startQu + 1);
                            int endQu = startQu + 1 + innerQu.IndexOf('"');
                            //innerQu = innerQu.Remove(innerQu.IndexOf('"'));
                            
                            String atParse = doc.Remove(endQu);
                            
                            cParseObj4(atParse, parent);
                            //advance parser
                            doc = doc.Substring(endQu + 1);
                            cIndex = -1;
                        }
                        else
                        {
                            String aInner = doc.Remove(cIndex);
                            cParseObj4(aInner, parent);
                            //advance parser
                            doc = doc.Substring(doc.IndexOf(',') + 1);
                            cIndex = -1;
                        }
                    }
                    //if attribute is list item
                    else if (!(inner.Contains("=") || inner == "," || inner == ""))
                    {
                        cParseObj4(inner.Split(',')[0], parent);
                        //advance parser
                        doc = doc.Substring(doc.IndexOf(',') + 1);
                        cIndex = -1;
                    }
                }
                cIndex++;
            }
            return "";
        }

        private void cParseObj(String doc, CElement obj)
        {            
            String[] attArray = doc.Split(',');
            foreach(String at in attArray)
            {
                
                char[] trimChars = new char[] { ' ', '}', '{', ',', '[', ']', '"' };
                String atVal = at.Trim(' ');
                //remove commenting
                if (atVal.StartsWith("--"))
                {
                    atVal = atVal.Substring(6);
                }
                //add attribute
                if (atVal != "")
                {
                    String[] sep = atVal.Split('=');
                    obj.addAttribute(sep);
                }                
            }
        }

        private void cParseObj4(String doc, CElement obj)
        {            
            String at = doc;
            
            char[] trimChars = new char[] { ' ', '}', '{', ',', '[', ']', '"' };
            String atVal = at.Trim(' ');
            //remove commenting
            if (atVal.StartsWith("--"))
            {
                atVal = atVal.Substring(6);
            }
            //add attribute
            if (atVal != "")
            {
                String[] sep = atVal.Split('=');
                obj.addAttribute(sep);
            }            
        }

        private void buildTreeNodes(TreeNode tParent, CElement parent)
        {
            //add objects
            foreach (CElement ce in parent.children)
            {
                TreeNode nNode = new TreeNode(ce.ToString());
                tParent.Nodes.Add(nNode);
                buildTreeNodes(nNode, ce);
            }
            //add attributes
            foreach (String[] ca in parent.att)
            {
                String nodeText = ca[0] + " : " + ca[1];
                TreeNode aNode = new TreeNode(nodeText);
                tParent.Nodes.Add(aNode);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            listView1.Items.Clear();
            String RHdata = "";
            bool wcError = false;
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
               //stupid characters
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(wc.DownloadString("https://raid-helper.dev/api/event/" + textBox1.Text));
                byte[] win1252Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("Windows-1252"), utf8Bytes);
                RHdata = Encoding.UTF8.GetString(win1252Bytes).Trim(new char[] {'{','}' });
                String[] pData = RHdata.Split(',');
                if (pData[0].Contains("unknown event"))
                    throw new Exception(pData[0]);
            }
            catch (Exception ex)
            {
                String[] lviAttributes = new string[] { "Error", RHdata, ex.Message };
                ListViewItem nItem = new ListViewItem(lviAttributes);
                listView1.Items.Add(nItem);
                wcError = true;
            }



            if (RHdata != "" && !wcError)
            {
                
                //cl.buildPlayerList(CConfig.findElement("Cleo_DB>global>cache>player>", cParent), CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>configurations>634442A9-6586-D664-5DB7-DEA8147C6E33>alts", cParent), CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>lists>63446602-2A74-DAD4-6934-BE542E7DBA8E>players", cParent));
                cl.buildRHData(RHdata);

                String unixUpdate = RHdata.Substring(RHdata.IndexOf("last_updated") + 14);
                unixUpdate = unixUpdate.Remove(unixUpdate.IndexOf(","));
                Double lastUpdate = Double.Parse(unixUpdate);
                System.DateTime updateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                updateTime = updateTime.AddMilliseconds(lastUpdate).ToLocalTime();

                String unixClosing = RHdata.Substring(RHdata.IndexOf("closingtime") + 13);
                unixClosing = unixClosing.Remove(unixClosing.IndexOf(","));
                Double dUnixClosing = Double.Parse(unixClosing);
                System.DateTime closingTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                closingTime = closingTime.AddSeconds(dUnixClosing).ToLocalTime();



                foreach (CPlayer cPlayer in cl.player)
                {                    
                    String altStr = "";
                    for (int i = 1; i < cPlayer.toons.Count; i++)                    
                    {
                        altStr += cPlayer.toons[i][1] + ",";
                    }
                    if (altStr == "")
                        altStr= "NONE";

                    String[] lviAttributes = new string[] { cPlayer.toons[0][1].Split('-')[0], cPlayer.RLrole, cPlayer.RLstatus1, altStr.Trim(',') };
                    ListViewItem nItem = new ListViewItem(lviAttributes);
                    if (cPlayer.RLstatus1 == "NONE")
                    {
                        nItem.BackColor = Color.Red;
                    }                    
                    nItem.Tag = cPlayer;
                    listView1.Items.Add(nItem);
                }
                foreach (CPlayer dp in cl.dPlayer)
                {
                    String[] lviAttributes = new string[] { dp.RLtoon1, "No Cleo Match", ""};
                    ListViewItem nItem = new ListViewItem(lviAttributes);
                    nItem.BackColor = Color.Orange;
                    listView1.Items.Add(nItem);
                }
                textBox2.Text = cl.RHmsg;
                textBox2.Text += "Last Updated : " + updateTime + " - Closing Time : " + closingTime + " - Date Pulled : " + DateTime.Now.ToLocalTime() + "ST";
                button3.Enabled = true;
            }           
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText(listView1.SelectedItems[0].Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            //String htmlStart = "<!--StartFragment--><google-sheets-html-origin><style type=\"text/css\"><!--td {border: 1px solid #cccccc;}br {mso-data-placement:same-cell;}--></style><table xmlns=\"http://www.w3.org/1999/xhtml\" cellspacing=\"0\" cellpadding=\"0\" dir=\"ltr\" border=\"1\" style=\"table-layout:fixed;font-size:10pt;font-family:Arial;width:0px;border-collapse:collapse;border:none\"><colgroup><col width=\"100\"/></colgroup><tbody>";
            //String rowStart = "<tr style=\"height:21px;\"><td style=\"overflow:hidden;padding:2px 3px 2px 3px;vertical-align:bottom;\" data-sheets-value=\"{&quot;1&quot;:2,&quot;2&quot;:&quot;Test1&quot;}\">";
            //String rowEnd = "</td></tr>";
            //String tableEnd = "</tbody></table><!--EndFragment-->\r\n</body>\r\n</html>";

            String htmlStart = "<html>\r\n<body><!--StartFragment--><table>";
            String tHead = "<thead><tr><th>Cleo Player Name</th><th>Signup Status</th><th>Actions</th></tr></thead>";
            String cellStart = "<td>";
            String cellStartRed = "<td style=\"color:#ff0000;\">";
            String cellEnd = "</td>";
            String htmlEnd = "</tbody></table><!--EndFragment-->\r\n</body>\r\n</html>";

            String rosterHtml = htmlStart + tHead + "<tbody>";
            CPlayer[] RHplayerSorted = new CPlayer[cl.player.Count];
            foreach (CPlayer cPlayer in cl.player)
            {
                int pos = 100;
                if (cPlayer.RLposition1 != "NONE")
                {
                    pos = Int16.Parse(cPlayer.RLposition1);
                    RHplayerSorted[pos - 1] = cPlayer;
                }
                else
                {                    
                    int fPos = RHplayerSorted.Length - 1;
                    while(true)
                    {
                        if (RHplayerSorted[fPos] == null)
                        {
                            RHplayerSorted[fPos] = cPlayer;
                            break;
                        }
                        fPos--;
                    }
                }
            }


            foreach (CPlayer cPlayer in RHplayerSorted)
            {
                rosterHtml += "<tr>";
                rosterHtml += cellStart + cPlayer.ToString().Split('-')[0] + cellEnd;
                String status = "RH - No Signup";
                if (cPlayer.RLstatus1 == "Tentative")
                    status = "RH - Tentative";
                else if (cPlayer.RLstatus1 == "Absence")
                    status = "RH - Absence";
                else if (cPlayer.RLstatus1 == "Bench")
                    status = "RH - Bench";
                else if (cPlayer.RLstatus1 == "NONE")
                    status = "RH - No Signup";
                else
                    status = "RH - Attending";
                if (cPlayer.RLrole.StartsWith("*"))
                    status = "*" + status;
                rosterHtml += cellStart + status + cellEnd;
                rosterHtml += "</tr>";
            }
            
            rosterHtml += "<tr><td>" + textBox2.Text + "</td></tr>";
            rosterHtml += htmlEnd;
            String RHdate = textBox2.Lines[1];
            String[] RHdateFix = RHdate.Split('-');
            RHdate = RHdateFix[1] + "-" + RHdateFix[0] + "-" + RHdateFix[2];
            String oFileName = "RHroster-" + RHdate + ".html";

            System.IO.File.WriteAllText(Properties.Settings.Default.OutputLoc + oFileName, rosterHtml);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                String path = e.Node.FullPath.Replace("\\", ">");
                path = path.Substring(path.IndexOf('>'));
                textBox3.Text = path;
                //System.Windows.Forms.Clipboard.SetText(listView1.SelectedItems[0].Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            String llconfigId = lootListconfig.getllconfigID(comboBox1.SelectedItem.ToString());
            CElement[] lootLists = lootListconfig.getlootLists(llconfigId);
            comboBox2.Items.Clear();

            List<string> strLists = new List<string>();
            foreach (CElement loot in lootLists)
            {
                strLists.Add(lootListconfig.getllName(loot.ToString()));
                //comboBox2.Items.Add(lootListconfig.getllName(loot.ToString()));
            }
            comboBox2.Items.AddRange(strLists.OrderBy(x => x).ToArray());
            button4.Enabled = true;
            textBox8.Enabled = true;
            textBox8.Text = comboBox1.Text + " - " + DateTime.Now;
            comboBox4.Enabled= true;
            comboBox4.Items.Clear();
            button8.Enabled = true;

            //populate loot DB selection from config
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {

                    //conn.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\offsp\\source\\repos\\Offsprey\\CleoTool\\CleoTool\\CleoToolDB.mdf;Integrated Security=True;Connect Timeout=30";
                    conn.ConnectionString = Properties.Settings.Default.CleoToolDBConnectionString;
                    conn.Open();

                    //Find previous loot instance
                    SqlCommand command = new SqlCommand("SELECT * FROM Loot WHERE CONVERT(VARCHAR,Lootconfig) = '" + comboBox1.SelectedItem + "' ORDER BY LootDateTime DESC", conn);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox4.Items.Add(reader[1]);
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                comboBox4.Items.Add(ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), comboBox2.SelectedItem.ToString());

            CElement ll = lootListconfig.getlootList(llconfigId);
            CElement llPlayers = CConfig.findElement("players", ll);
            listView2.Items.Clear();
            foreach(String[] player in llPlayers.att)
            {
                String toonName = cl.findToonName(player[1]);
                String toonClass = cl.findToonClass(player[1]);
                ListViewItem lItem = new ListViewItem(new string[] { player[0], toonName });
                

                switch (toonClass)
                {
                    case "DEATHKNIGHT":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(DK_COLOR);                        
                        break;
                    case "DRUID":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(DRUID_COLOR);
                        break;
                    case "HUNTER":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(HUNTER_COLOR);
                        break;
                    case "MAGE":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(MAGE_COLOR);
                        break;
                    case "PALADIN":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(PALY_COLOR);
                        break;
                    case "PRIEST":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(PRIEST_COLOR);
                        break;
                    case "ROGUE":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(ROGUE_COLOR);
                        break;
                    case "SHAMAN":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(SHAMAN_COLOR);
                        break;
                    case "WARLOCK":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(WARLOCK_COLOR);
                        break;
                    case "WARRIOR":
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml(WARRIOR_COLOR);
                        break;
                    default:
                        lItem.BackColor = System.Drawing.ColorTranslator.FromHtml("#ff33cc");
                        break;


                }

                
                listView2.Items.Add(lItem);
            }

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //build list table
            int ilist = 0;            
            String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), (String)comboBox2.Items[0]);
            CElement ll = lootListconfig.getlootList(llconfigId);
            CElement llPlayers = CConfig.findElement("players", ll);
            String[,] lists = new string[comboBox2.Items.Count, llPlayers.att.Count];
            int playerCount = llPlayers.att.Count;
            int listCount = comboBox2.Items.Count;

            foreach (String list in comboBox2.Items)
            {
                llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), list);
                ll = lootListconfig.getlootList(llconfigId);
                llPlayers = CConfig.findElement("players", ll);

                int iplayer = 0;
                foreach (String[] player in llPlayers.att)
                {
                    String toonName = cl.findToonName(player[1]);
                    String toonClass = cl.findToonClass(player[1]);
                    String row = "<td style=color:";
                    switch(toonClass)
                    {
                        case "DEATHKNIGHT":
                            row += DK_COLOR;
                            break;
                        case "DRUID":
                            row += DRUID_COLOR;
                            break;
                        case "HUNTER":
                            row += HUNTER_COLOR;
                            break;
                        case "MAGE":
                            row += MAGE_COLOR;
                            break;
                        case "PALADIN":
                            row += PALY_COLOR;
                            break;
                        case "PRIEST":
                            row += PRIEST_COLOR;
                            break;
                        case "ROGUE":
                            row += ROGUE_COLOR;
                            break;
                        case "SHAMAN":
                            row += SHAMAN_COLOR;
                            break;
                        case "WARLOCK":
                            row += WARLOCK_COLOR;
                            break;
                        case "WARRIOR":
                            row += WARRIOR_COLOR;
                            break;
                        default:
                            row += "#ff33cc";
                            break;


                    }
                    row += ";>";

                    ListViewItem lItem = new ListViewItem(new string[] { player[0], toonName });
                    lists[ilist, iplayer] = row + toonName.Split('-')[0] + "</td>";
                    iplayer++;
                }
                ilist++;
            }

            String htmlStyle = "<style>td:nth-child(even), th:nth-child(even) {background-color: #666666;}body{font-family: Arial, Helvetica, sans-serif;background-color:#4d4d4d; color:#d9d9d9;}tr {border-bottom: 1px solid #d9d9d9;}th, td {padding-left: 40px;padding-right: 40px;max-width: 100;}table {border-collapse: collapse;color:#d9d9d9;font-weight:bold;}</style>";
            String htmlStart = "<html><head>" + htmlStyle + "</head><body><!--StartFragment--><table>";
            String tHead = "<thead><tr><th>Priority</th>";
            foreach (String list in comboBox2.Items)
            {
                tHead += "<th>" + list + "</th>";
            }
            tHead += "</tr></thead>";
            String cellStart = "<td>";
            String cellEnd = "</td>";
            String htmlEnd = "<!--EndFragment-->\r\n</body>\r\n</html>";
            
            String rosterHtml = htmlStart + tHead + "<tbody>"; 


            for (int i = 0; i < playerCount; i++)
            {
                String row = "<tr>" + cellStart + (i + 1).ToString() + cellEnd;
                for (int o = 0; o < listCount; o++)
                {
                    //row += "<td>" + lists[o, i] + "</td>";
                    if (lists[o, i] == null)
                        row += "<td>-</td>";
                    else
                        row += lists[o, i];
                }
                row += "</tr>";
                rosterHtml += row;

            }
            rosterHtml += "</tbody></table>";
            rosterHtml += "<div>" + comboBox1.Text + " - " + DateTime.Now + "</div>";
            rosterHtml += htmlEnd;

            String cleoDate = DateTime.Now.ToString("yyyy-dd-M--HH-mm");
            String oFileName = "Cleo-" + comboBox1.Text + "-" + cleoDate + ".html";

            System.IO.File.WriteAllText(Properties.Settings.Default.OutputLoc + oFileName, rosterHtml);
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && cParent != null)
                button2.Enabled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox4.Text = Properties.Settings.Default.PropertyValues[comboBox3.SelectedItem.ToString()].PropertyValue.ToString();
            //textBox4.Text = Properties.Settings.Default.Properties[comboBox3.SelectedItem.ToString()].DefaultValue.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine(DataCompression.zlipDecomp("test"));
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                textBox6.Text = DataCompression.zlipDecomp(textBox5.Text);
            }
            catch (Exception ex)
            {
                textBox5.Text = "";
            }

        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            textBox7.Text = Properties.Settings.Default.CleoConfigLoc.ToString();

            openFileDialog1.FileName = Properties.Settings.Default.CleoConfigLoc;
            openFileDialog1.InitialDirectory = Properties.Settings.Default.CleoConfigLoc.Remove(Properties.Settings.Default.CleoConfigLoc.LastIndexOf("\\"));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox7.Text = openFileDialog1.FileName;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default.CleoConfigLoc = textBox7.Text;
            Properties.Settings.Default["CleoConfigLoc"] = textBox7.Text;
            Properties.Settings.Default.Save();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Ensure Loot Instance Name is Correct.", "Save to Database", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                saveToBD();
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }


        }

        public void saveToBD()
        {
            using (SqlConnection conn = new SqlConnection())
            {

                //build list table
                int ilist = 0;
                String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), (String)comboBox2.Items[0]);
                CElement ll = lootListconfig.getlootList(llconfigId);
                CElement llPlayers = CConfig.findElement("players", ll);
                String[,] lists = new string[comboBox2.Items.Count, llPlayers.att.Count];
                int playerCount = llPlayers.att.Count;
                int listCount = comboBox2.Items.Count;
                conn.ConnectionString = Properties.Settings.Default.CleoToolDBConnectionString;
                conn.Open();

                // use the connection here
                SqlCommand insertCommand = new SqlCommand("INSERT INTO Loot (LootInstance, LootDateTime, LootConfig, LootServer) VALUES (@0, @1, @2, @3)", conn);
                insertCommand.Parameters.Add(new SqlParameter("0", textBox8.Text));
                insertCommand.Parameters.Add(new SqlParameter("1", DateTime.Now));
                insertCommand.Parameters.Add(new SqlParameter("2", comboBox1.SelectedItem));
                insertCommand.Parameters.Add(new SqlParameter("3", Properties.Settings.Default.LootLists));
                insertCommand.ExecuteNonQuery();


                SqlCommand command = new SqlCommand("SELECT LootId FROM Loot WHERE LootId=(SELECT max(LootId) FROM Loot)", conn);
                int id = 0;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    var rid = reader[0];
                    id = Convert.ToInt32(rid);
                }

                foreach (String list in comboBox2.Items)
                {
                    llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), list);
                    ll = lootListconfig.getlootList(llconfigId);
                    llPlayers = CConfig.findElement("players", ll);

                    foreach (String[] player in llPlayers.att)
                    {
                        String toonName = cl.findToonName(player[1]);
                        String toonClass = cl.findToonClass(player[1]);

                        SqlCommand insertCommand2 = new SqlCommand("INSERT INTO LootListEntry (LootList, LootRank, PlayerName, LootId, PlayerClass) VALUES (@0, @1, @2, @3, @4)", conn);
                        insertCommand2.Parameters.Add(new SqlParameter("0", list));
                        insertCommand2.Parameters.Add(new SqlParameter("1", player[0]));
                        insertCommand2.Parameters.Add(new SqlParameter("2", toonName));
                        insertCommand2.Parameters.Add(new SqlParameter("3", id));
                        insertCommand2.Parameters.Add(new SqlParameter("4", toonClass));
                        insertCommand2.ExecuteNonQuery();


                    }
                    ilist++;
                }


                conn.Close();
            }
        }


        public void importRanking(string filePath)
        {
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    
                    conn.ConnectionString = Properties.Settings.Default.CleoToolDBConnectionString;
                    conn.Open();

                    // use the connection here
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO Loot (LootInstance, LootDateTime, LootConfig, LootServer) VALUES (@0, @1, @2, @3)", conn);
                    insertCommand.Parameters.Add(new SqlParameter("0", ""));
                    insertCommand.Parameters.Add(new SqlParameter("1", DateTime.Now));
                    insertCommand.Parameters.Add(new SqlParameter("2", ""));
                    insertCommand.Parameters.Add(new SqlParameter("3", ""));
                    insertCommand.ExecuteNonQuery();


                    SqlCommand command = new SqlCommand("SELECT LootId FROM Loot WHERE LootId=(SELECT max(LootId) FROM Loot)", conn);
                    int id = 0;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        var rid = reader[0];
                        id = Convert.ToInt32(rid);
                    }

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters("\t");
                    string[] listFields = new string[0];
                    int row = 0;
                    while (!parser.EndOfData)
                    {
                        //Process row
                        
                        string[] fields = parser.ReadFields();

                        int column = 0;
                        if (row == 0)
                        {
                            listFields = fields;
                        }
                        else
                        {
                            foreach (string field in fields)
                            {
                                if (column != 0)
                                {
                                    SqlCommand insertCommand2 = new SqlCommand("INSERT INTO LootListEntry (LootList, LootRank, PlayerName, LootId, PlayerClass) VALUES (@0, @1, @2, @3, @4)", conn);
                                    insertCommand2.Parameters.Add(new SqlParameter("0", listFields[column]));
                                    insertCommand2.Parameters.Add(new SqlParameter("1", row));
                                    insertCommand2.Parameters.Add(new SqlParameter("2", field + "-Atiesh"));
                                    insertCommand2.Parameters.Add(new SqlParameter("3", id));
                                    insertCommand2.Parameters.Add(new SqlParameter("4", ""));
                                    insertCommand2.ExecuteNonQuery();
                                }
                                column++;
                            }
                        }
                        row++;

                    }

                    conn.Close();
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //tab seperated values
            importRanking("C:\\Sheet1.tsv");
        }

        private void button10_Click(object sender, EventArgs e)
        {

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = Properties.Settings.Default.CleoToolDBConnectionString;
                conn.Open();

                String pLootInstance = "";
                int pLootId = 0;

                //Find previous loot instance
                //SqlCommand command = new SqlCommand("SELECT * FROM Loot WHERE LootDateTime=(SELECT max(LootDateTime) FROM Loot)", conn);
                SqlCommand command = new SqlCommand("SELECT * FROM Loot", conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String rInst = (string)reader[1];
                        if (rInst == comboBox4.SelectedItem.ToString())
                        {
                            pLootId = Convert.ToInt32(reader[0]);
                            pLootInstance = (string)reader[1];
                        }
                    }
                }

                //build list table
                int ilist = 0;
                String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), (String)comboBox2.Items[0]);
                CElement ll = lootListconfig.getlootList(llconfigId);
                CElement llPlayers = CConfig.findElement("players", ll);
                String[,] lists = new string[comboBox2.Items.Count, llPlayers.att.Count];
                int playerCount = llPlayers.att.Count;
                int listCount = comboBox2.Items.Count;

                foreach (String list in comboBox2.Items)
                {
                    llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), list);
                    ll = lootListconfig.getlootList(llconfigId);
                    llPlayers = CConfig.findElement("players", ll);

                    int iplayer = 0;
                    foreach (String[] player in llPlayers.att)
                    {
                        String toonName = cl.findToonName(player[1]);
                        String toonClass = cl.findToonClass(player[1]);
                        String row = "<td><span style=color:";
                        switch (toonClass)
                        {
                            case "DEATHKNIGHT":
                                row += DK_COLOR;
                                break;
                            case "DRUID":
                                row += DRUID_COLOR;
                                break;
                            case "HUNTER":
                                row += HUNTER_COLOR;
                                break;
                            case "MAGE":
                                row += MAGE_COLOR;
                                break;
                            case "PALADIN":
                                row += PALY_COLOR;
                                break;
                            case "PRIEST":
                                row += PRIEST_COLOR;
                                break;
                            case "ROGUE":
                                row += ROGUE_COLOR;
                                break;
                            case "SHAMAN":
                                row += SHAMAN_COLOR;
                                break;
                            case "WARLOCK":
                                row += WARLOCK_COLOR;
                                break;
                            case "WARRIOR":
                                row += WARRIOR_COLOR;
                                break;
                            default:
                                row += "#ff33cc";
                                break;


                        }
                        row += ";>";
                        //build differential data from DB
                        String difStr = "*";
                        String difColor = "#fefefe";
                        try
                        {
                            int pRank = 0;
                            command = new SqlCommand("SELECT LootRank FROM LootListEntry WHERE (" +
                                "LootId=" + pLootId.ToString() +
                                " AND CONVERT(VARCHAR,PlayerName) = '" + toonName + "'" +
                                " AND CONVERT(VARCHAR,LootList) = '" + list + "'" +
                                ")", conn);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                reader.Read();
                                var rid = reader[0];
                                pRank = Convert.ToInt32(rid);
                            }
                            int rDif = pRank - (iplayer + 1);
                            if (rDif < 0)
                            {
                                //down list
                                difStr = Math.Abs(rDif).ToString();                                
                                difColor = "#fa0202";
                            }
                            else if (rDif > 0)
                            {
                                //up list
                                difStr = rDif.ToString();
                                difColor = "#10f202";
                            }
                            else
                            {
                                //no change
                                difStr = "-";
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        ListViewItem lItem = new ListViewItem(new string[] { player[0], toonName });
                         
                        lists[ilist, iplayer] = row + toonName.Split('-')[0] + "</span><span style=font-weight:normal;color:" + difColor +"> (" + difStr + ")</span></td>";
                        iplayer++;
                    }
                    ilist++;
                }

                //build HTML table
                //dark theme
                String htmlStyle = "<style>td:nth-child(even), th:nth-child(even) {background-color: #666666;}body{font-family: Arial, Helvetica, sans-serif;background-color:#4d4d4d; color:#d9d9d9;}tr {border-bottom: 1px solid #d9d9d9;}th, td {padding-left: 30px;padding-right: 30px;max-width: 140;}table {border-collapse: collapse;color:#d9d9d9;font-weight:bold;}</style>";
                String htmlStart = "<html><head>" + htmlStyle + "</head><body><!--StartFragment--><table>";
                String tHead = "<thead><tr><th>Priority</th>";
                //build list names
                foreach (String list in comboBox2.Items)
                {
                    tHead += "<th>" + list + "</th>";
                }
                tHead += "</tr></thead>";
                String cellStart = "<td>";
                String cellEnd = "</td>";
                String htmlEnd = "<!--EndFragment-->\r\n</body>\r\n</html>";

                String rosterHtml = htmlStart + tHead + "<tbody>";


                for (int i = 0; i < playerCount; i++)
                {
                    String row = "<tr>" + cellStart + (i + 1).ToString() + cellEnd;
                    for (int o = 0; o < listCount; o++)
                    {
                        //row += "<td>" + lists[o, i] + "</td>";
                        if (lists[o, i] == null)
                            row += "<td>-</td>";
                        else
                            row += lists[o, i];
                    }
                    row += "</tr>";
                    rosterHtml += row;

                }
                rosterHtml += "</tbody></table>";
                rosterHtml += "<div>" + comboBox1.Text + " - " + DateTime.Now + "</div>";
                rosterHtml += htmlEnd;

                String cleoDate = DateTime.Now.ToString("yyyy-dd-M--HH-mm");
                String oFileName = "Cleo-" + comboBox1.Text + "-" + cleoDate + ".html";


                System.IO.File.WriteAllText(Properties.Settings.Default.OutputLoc + oFileName, rosterHtml);
                conn.Close();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            button10.Enabled = true;
            button11.Enabled = true;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            //delete loot instance
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = Properties.Settings.Default.CleoToolDBConnectionString;
                conn.Open();

                String pLootInstance = "";
                int pLootId = 0;

                SqlCommand command = new SqlCommand("SELECT * FROM Loot", conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String rInst = (string)reader[1];
                        if (rInst == comboBox4.SelectedItem.ToString())
                        {
                            pLootId = Convert.ToInt32(reader[0]);
                            pLootInstance = (string)reader[1];
                        }
                    }
                }
                
                SqlCommand deleteCommand = new SqlCommand("DELETE FROM LootListEntry WHERE LootId = " + pLootId , conn);

                deleteCommand.ExecuteNonQuery();

                deleteCommand = new SqlCommand("DELETE FROM Loot WHERE LootId = " + pLootId, conn);

                deleteCommand.ExecuteNonQuery();

                comboBox4.Items.Clear();
                button10.Enabled = false;
                button11.Enabled = false;

                //Find previous loot instance
                command = new SqlCommand("SELECT * FROM Loot WHERE CONVERT(VARCHAR,Lootconfig) = '" + comboBox1.SelectedItem + "' ORDER BY LootDateTime DESC", conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox4.Items.Add(reader[1]);
                    }
                }

                conn.Close();
            }
        }
    }
}

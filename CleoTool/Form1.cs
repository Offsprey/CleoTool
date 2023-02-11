﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CleoTool
{
    public partial class Form1 : Form
    {
        CElement cParent;
        CleoLootListConfig lootListconfig = new CleoLootListConfig();
        CleoLists cl = new CleoLists();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            cParent = new CElement("Root");
            cl = new CleoLists();
            lootListconfig = new CleoLootListConfig();
            List<String> players = new List<string>();

            //prep lua data
            try 
            {
                
                    String rawTxt = System.IO.File.ReadAllText("C:\\Program Files (x86)\\World of Warcraft\\_classic_\\WTF\\Account\\OFFSPREY\\SavedVariables\\Cleo.lua");
                //String rawTxt = System.IO.File.ReadAllText("C:\\Users\\16187\\Documents\\Cleo.lua");
                rawTxt = rawTxt.Replace("\n", "");
                rawTxt = rawTxt.Replace("\r", "");
                rawTxt = rawTxt.Replace("\t", "");
                //parse lua data
                cParse4(rawTxt, cParent);
                //build treeview
                TreeNode root = new TreeNode(cParent.ToString());
                buildTreeNodes(root, cParent);
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(root);

            }
            catch (Exception ex)
            {
                TreeNode root = new TreeNode("Error : " + ex.Message);
                buildTreeNodes(root, cParent);
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(root);
            }

            try
            {
                cl.buildPlayerList(CConfig.findElement("Cleo_DB>global>cache>player>", cParent), CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>configurations>634442A9-6586-D664-5DB7-DEA8147C6E33>alts", cParent), CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>lists>63446602-2A74-DAD4-6934-BE542E7DBA8E>players", cParent));

                lootListconfig.LootListsConfig = CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>configurations>", cParent).children.ToArray();
                lootListconfig.LootLists = CConfig.findElement("Cleo_Lists>factionrealm>Alliance - Atiesh>lists>", cParent).children.ToArray();
                //Root>Cleo_Lists>factionrealm>Alliance - Atiesh>lists
                //CElement[] lootListconfig = listconfig.children.ToArray();
                comboBox1.Items.Clear();
                foreach (CElement llConfig in lootListconfig.LootListsConfig)
                {
                    comboBox1.Items.Add(llConfig.findAtt("name"));

                }
            }
            catch
            {

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
                    if (!nextStr.Contains("}") && !nextStr.Contains("["))
                    {
                        int t = 0;
                    }
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
                        if (name == "6344681C-FBDF-9994-D19D-EF54F23CAA03")
                        {
                            int t5 = 0;
                        }
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
                            if (atParse.Contains("}"))
                            {
                                int t2 = 0;
                            }
                            cParseObj4(atParse, parent);
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
                    int t = 0;
                }                
            }
        }

        private void cParseObj4(String doc, CElement obj)
        {
            
            String at = doc;
            if (at.Contains("}"))
            {
                int t2 = 0;
            }
            

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
                int t = 0;
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
            String RHdata = "";
            bool wcError = false;
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
               //stupid characters
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(wc.DownloadString("https://raid-helper.dev/api/event/" + textBox1.Text));
                byte[] win1252Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("Windows-1252"), utf8Bytes);
                RHdata = Encoding.UTF8.GetString(win1252Bytes);
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

                foreach (CPlayer cPlayer in cl.player)
                {                    
                    String altStr = "";
                    for (int i = 1; i < cPlayer.toons.Count; i++)                    
                    {
                        altStr += cPlayer.toons[i][1] + ",";
                    }

                    String[] lviAttributes = new string[] { cPlayer.toons[0][1].Split('-')[0], cPlayer.RLrole, cPlayer.RLstatus1, altStr.Trim(',') };
                    ListViewItem nItem = new ListViewItem(lviAttributes);
                    if (cPlayer.RLstatus1 == "NONE")
                    {
                        nItem.BackColor = Color.Red;
                    }                    
                    nItem.Tag = cPlayer;
                    listView1.Items.Add(nItem);
                }
                foreach (String dp in cl.dPlayer)
                {
                    String[] lviAttributes = new string[] { dp, "Not in Cleo", "not in Cleo" };
                    ListViewItem nItem = new ListViewItem(lviAttributes);
                    nItem.BackColor = Color.Orange;
                    listView1.Items.Add(nItem);
                }
                textBox2.Text = cl.RHmsg;
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
            foreach (CPlayer cPlayer in cl.player)
            {
                rosterHtml += "<tr>";
                rosterHtml += cellStart + cPlayer.ToString().Split('-')[0] + cellEnd;
                String status = "RH - No Signup";
                if (cPlayer.RLstatus1 == "Tentative")
                    status = "RH - Tentative";
                else if (cPlayer.RLstatus1 == "Absence")
                    status = "RH - Absence";
                else if (cPlayer.RLstatus1 == "NONE")
                    status = "RH - No Signup";
                else
                    status = "RH - Attending";
                rosterHtml += cellStart + status + cellEnd;
                rosterHtml += "</tr>";
            }
            foreach(String dp in cl.dPlayer)
            {
                rosterHtml += "<tr>";
                rosterHtml += cellStartRed + "*" + dp + cellEnd;
                rosterHtml += cellStartRed + "RH - Not in Cleo" + cellEnd;
                rosterHtml += "</tr>";
            }
            rosterHtml += htmlEnd;
            String RHdate = textBox2.Lines[1];
            String[] RHdateFix = RHdate.Split('-');
            RHdate = RHdateFix[1] + "-" + RHdateFix[0] + "-" + RHdateFix[2];
            String oFileName = "RHroster-" + RHdate + ".html";

            System.IO.File.WriteAllText("C:\\Users\\16187\\Documents\\" + oFileName, rosterHtml);
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

            foreach (CElement loot in lootLists)
            {
                comboBox2.Items.Add(lootListconfig.getllName(loot.ToString()));
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), comboBox2.SelectedItem.ToString());
            //String llconfigId = lootListconfig.getllID(comboBox1.SelectedItem.ToString(), comboBox2.SelectedItem.ToString().Split('-')[0].Trim());

            CElement ll = lootListconfig.getlootList(llconfigId);
            CElement llPlayers = CConfig.findElement("players", ll);
            listView2.Items.Clear();
            foreach(String[] player in llPlayers.att)
            {
                String toonName = cl.findToonName(player[1]);
                ListViewItem lItem = new ListViewItem(new string[] { player[0], toonName });
                listView2.Items.Add(lItem);
            }

        }
    }
}
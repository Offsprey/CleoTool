using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleoTool
{
    internal class CleoLists
    {
        public List<CPlayer> player = new List<CPlayer>();
        public List<String> dPlayer = new List<String>();
        List<String[]> toons = new List<String[]>();
        List<String> dsMapping = new List<String>();
        public string RHmsg = "";
        

        public CleoLists()
        {

        }

        public void buildPlayerList(CElement players, CElement  alts, CElement lList)
        {
            //populate toon list from gloabl cache
            toons.Clear();
            player.Clear();
            foreach (CElement cePlayer in players.children)
            {
                String id = cePlayer.ToString();
                //String pName = cePlayer.att[0][1];
                //String pName = "NO NAME ATT";
                //foreach (String[] att in cePlayer.att)
                //{
                //    if (att[0] == "name")
                //        pName = att[1];
                //}
                String pName = cePlayer.findAtt("name");
                if (pName == "Officer")
                {
                    int t = 0;
                }
                toons.Add(new String[] { id.Substring(id.IndexOf("-") + 1), pName, cePlayer.findAtt("class") });
            }
            //populate players from loot list
            foreach (String[] lst in lList.att)
            {
                CPlayer nPlayer = new CPlayer();
                nPlayer.toons.Add(new String[] { lst[1], findToonName(lst[1]) });
                player.Add(nPlayer);
            }
            //populate player alts
            foreach (CElement pAlt in alts.children)
            {
                foreach (CPlayer tPlayer in player)
                {
                    if (tPlayer.toons[0][0] == pAlt.ToString())
                    {   
                        foreach(String[] aAlt in pAlt.att)
                        {
                            tPlayer.toons.Add(new String[] { aAlt[1], findToonName(aAlt[1]) });
                        }
                    }
                }                
            }
        }        

        public String findToonName(String id)
        {
            foreach (String[] toon in toons)
            {
                if (toon[0] == id)
                    return toon[1];
            }
            return "";
        }

        public String findToonClass(String id)
        {
            foreach (String[] toon in toons)
            {
                if (toon[0] == id)
                    return toon[2];
            }
            return "";
        }

        public void buildRHData(String RHjson)
        {
            dPlayer.Clear();
            dsMapping.Clear();
            //dPlayer.Add("GhostPlayer");

            //populate player sign up info
            String[] sections = RHjson.Split('[');
            String[] RHsignups = sections[1].Split('{');
            //map toon-discord name
            dsMapping.AddRange(Properties.Settings.Default.DiscordUserMapping.Split('{'));
            foreach (String s in RHsignups)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    String[] attributes = s.Split(',');
                    String RHname = attributes[1].Split(':')[1].Trim().Trim('"');
                    //check for mapped toon
                    foreach (String mapping in dsMapping)
                    {
                        String[] map = mapping.Trim('}').Split(':');
                        if (map[0] == RHname)
                            RHname = map[1];
                    }
                    //cleanup discord name
                    RHname = RHname.Split('/')[0];
                    RHname = RHname.Split('(')[0];
                    RHname = RHname.Replace("\\", "");
                    RHname = RHname.Replace("\"", "");

                    if (RHname.StartsWith("Z"))
                    {
                        int t2 = 0;
                    }
                    
                    String RHrole = attributes[0].Split(':')[1].Trim().Trim('"');
                    String RHclass = attributes[8].Split(':')[1].Trim().Trim('"');
                    //find player from sign up
                    CPlayer tPlayer = findPlayer(RHname);

                    //update player sign up indo
                    if (tPlayer != null)
                    {
                        tPlayer.RLstatus1 = RHclass;
                        tPlayer.RLtoon1 = RHname;
                        tPlayer.RLrole = RHrole;
                    }
                    else if (attributes[0].Split(':')[0].Trim().Trim('"') != "show_header")
                        dPlayer.Add(attributes[1].Split(':')[1].Trim().Trim('"'));
                     
                }
            }

            String[] signupInfo = sections[18].Split('{');
            String[] RHdetails = signupInfo[4].Split('"');
            String RHMtitle = RHdetails[27]; //27
            String RHMdescription = RHdetails[19]; //19
            String RHMchannel = RHdetails[51]; //51
            String RHMdatetime = sections[0].Split('"')[3];
            RHmsg += RHMtitle + "\r\n";
            RHmsg += RHMdatetime + "\r\n";
            RHmsg += RHMdescription + "\r\n";
            RHmsg += RHMchannel + "\r\n";            

        }

        public CPlayer findPlayer(String toonName)
        {
            //find player by toon name
            foreach (CPlayer tPlayer in player)
            {
                foreach (String[] toon in tPlayer.toons)
                {
                    String toonShortName = toon[1].Split('-')[0].Trim('"');
                    if (toonShortName.ToLower() == toonName.ToLower())
                        return tPlayer;
                }
            }
            return null;
        }
    }
}

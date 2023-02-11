using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleoTool
{
    internal class CleoLootListConfig
    {
        CElement[] lootLists;
        CElement[] lootListsConfig;

        internal CElement[] LootLists { get => lootLists; set => lootLists = value; }
        internal CElement[] LootListsConfig { get => lootListsConfig; set => lootListsConfig = value; }

        public String getllconfigID(String name)
        {
            foreach(CElement ele in LootListsConfig)
            {
                if (ele.findAtt("name") == name)
                    return ele.ToString();
            }
            return "NO CONFIG";
        }
        public String getllID(String cname, String lname)
        {
            foreach (CElement ele in LootLists)
            {
                if (ele.findAtt("name") == lname && ele.findAtt("configId") == getllconfigID(cname))
                    return ele.ToString();
            }
            return "NO CONFIG";
        }
        public CElement[] getlootLists(String cid)
        {
            List<CElement> list = new List<CElement>();
            foreach (CElement ele in LootLists)
            {
                if (ele.findAtt("configId") == cid)
                    list.Add(ele);
            }
            return list.ToArray();
        }

        public CElement getlootList(String id)
        {
           
            foreach (CElement ele in LootLists)
            {
                if (ele.ToString() == id)
                    return ele;
            }
            return null;
        }

        public String getllName(String id)
        {
            String llName = "NO NAME";
            foreach (CElement ele in LootLists)
            {
                if (ele.ToString() == id)
                {
                    bool nameStart = false;
                    foreach (String[] at in ele.att)
                    {
                        if (at[0] == "name")
                        {
                            llName = at[1];
                            nameStart = true;
                        }
                        else if (at[0] != "configId" && nameStart)
                            llName += " - " + at[1];
                    }
                }
            }

            return llName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleoTool
{
    //Cleo Lua Element
    internal class CElement
    {
        String cname;
        public List<CElement> children = new List<CElement>();
        public List<String[]> att = new List<String[]>();
        String value;
        String key;

        public string Value { get => value; set => this.value = value; }
        public string Key { get => key; set => key = value; }

        public CElement(List<CElement> children)
        {
            new CElement();
            this.children = children;
        }
        public CElement()
        {
            children = new List<CElement>();
        }

        public CElement(string cname)
        {
            new CElement();
            this.cname = cname;
        }

        public void addChild(CElement cEl)
        {
            children.Add(cEl);
        }
        public void addAttribute(String[] at)
        {
            char[] trimChars = new char[] { ' ', '}', '{', ',', '[', ']', '"' };
            String key = "NONE";
            String val = "";
            //if list item
            if (at.Length == 1)
            {
                key = (att.Count + 1).ToString();
                val = at[0].Trim(trimChars);
            }
            //if key / value pair
            else if (at.Length == 2)
            {
                key = at[0].Trim(trimChars);
                val = at[1].Trim(trimChars);
            }

            String[] nAt = new String[] { key, val };
            att.Add(nAt);    
        }

        public String findAtt(String attName)
        {
            foreach (String[] at in att)
            {
                if (at[0] == attName)
                    return at[1];
            }
            return "NO ATT";
        }

        public override String ToString()
        {
            return cname;
        }
    }
}

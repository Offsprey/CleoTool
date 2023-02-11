using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleoTool
{
    static internal class CConfig
    {
        public static CElement findElement(String path, CElement root)
        {
            //recurse path to return Cleo Element
            CElement item = root;
            String[] pPath = path.Split('>');
            
            foreach(CElement childCe in root.children)
            {
                if (childCe.ToString() == pPath[0])
                    return findElement(path.Substring(path.IndexOf('>') + 1), childCe);
            }

            return root;
        }

        
    }
}

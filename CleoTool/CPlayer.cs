using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleoTool
{
    internal class CPlayer
    {
        public List<String[]> toons = new List<String[]>();
        private String rLrole = "NONE";
        private String RLstatus = "NONE";
        private String RLtoon = "NONE";
        private String RLposition= "NONE";

        public CPlayer()
        {

        }

        public string RLrole { get => rLrole; set => rLrole = value; }
        public string RLstatus1 { get => RLstatus; set => RLstatus = value; }
        public string RLtoon1 { get => RLtoon; set => RLtoon = value; }
        public string RLposition1 { get => RLposition; set => RLposition = value; }

        public override String ToString()
        {
            return toons[0][1];
        }
    }
}

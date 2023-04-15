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

        public CPlayer(string rLrole, string rLstatus, string rLtoon, string rLposition)
        {
            this.rLrole = rLrole;
            RLstatus = rLstatus;
            RLtoon = rLtoon;
            RLposition = rLposition;
        }

        public CPlayer(string rLrole, string rLstatus, string rLtoon, string rLposition, string mToon)
        {
            this.rLrole = rLrole;
            RLstatus = rLstatus;
            RLtoon = rLtoon;
            RLposition = rLposition;
            toons.Add(new String[] { "Not In Cleo", mToon });
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

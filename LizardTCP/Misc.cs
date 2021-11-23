using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardTCP
{
    public class Misc
    {
        public static void AppendRule(RulesClass rule)
        {
            Program.Rules = new List<RulesClass>(Program.Rules) { rule }.ToArray();
        }
    }
}
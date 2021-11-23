using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardTCP
{
    public class RulesClass
    {
        public string ruleName { get; set; }
        public string ruleType { get; set; }
        public string ruleIP { get; set; }
        public string rulePort { get; set; }
        public string bindIP { get; set; }
        public string bindPort { get; set; }
    }

    public class SettingsClass
    {
        public string proxy_mode { get; set; }
        public bool debug { get; set; }
        public bool useCFHeaders { get; set; }
        public string bindingIP { get; set; }
        public string bindingPort { get; set; }
    }
}
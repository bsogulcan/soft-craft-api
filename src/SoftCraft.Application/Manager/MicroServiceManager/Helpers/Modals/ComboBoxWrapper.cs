using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftCraft.Manager.MicroServiceManager.Helpers.Modals
{
    public class ComboBoxWrapper
    {
        public ComboBoxWrapper()
        {
            this.OnChangeEventTasks = new List<string>();
        }
        public string EntityName { get; set; }
        public string PropertyName { get; set; }
        public bool IsInputProperty { get; set; }
        public string AccessString { get; set; }
        public string ComboBoxName { get { return $"{EntityName}{PropertyName}"; } }
        public string NGModel { get; set; }
        public string DataSource { get; set; }
        public string DataSourceGetFunction { get; set; }
        public string OnChangeEvent { get; set; }
        public List<string> OnChangeEventTasks { get; set; }
    }
}

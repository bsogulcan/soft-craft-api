using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftCraft.Manager.MicroServiceManager.Helpers.Modals
{
    public class PropertyWrapper
    {
        public PropertyWrapper()
        {
            this.Parents = new List<PropertyWrapper>();
        }
        public string PropertyName { get; set; }
        public string EntityName { get; set; }
        public PropertyWrapper Child { get; set; }
        public List<PropertyWrapper> Parents { get; set; }
    }
}

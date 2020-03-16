using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsRoot { get; set; } = false;
        public List<Indicator> Indicators { get; set; }
        public Component Parent { get; set; }
        public List<Component> Details { get; set; }
        

        public List<Component> ToListRecursive()
        {
            var list = new List<Component>();

            ToListRecursive(list);

            return list;
        }

        private void ToListRecursive(List<Component> components)
        {
            components.Add(this);

            if (Details != null)
                foreach (var detail in Details)
                    detail.ToListRecursive(components);
        }

        public void OrderRecursive()
        {
            if (Details == null)
                return;

            Details = Details.OrderBy(x => x.Order).ToList();

            foreach (var detail in Details)
                detail.OrderRecursive();
        }

    }
}

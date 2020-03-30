namespace CHI.Models.ServiceAccounting
{
    public class CaseFilter
    {
        public int Id { get; set; }
        public CaseFilterKind Kind { get; set; }
        public double? Code { get; set; }

        public Component Component { get; set; }

        public CaseFilter()
        {
        }

        public CaseFilter(Component component)
        {
            Component = component;
        }
    }
}

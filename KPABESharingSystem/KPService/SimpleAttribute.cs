namespace KPServices
{
    public class SimpleAttribute : UniverseAttribute
    {
        public SimpleAttribute(string name) : base(name)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
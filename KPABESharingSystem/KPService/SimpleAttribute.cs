namespace KPServices
{
    public class SimpleAttribute : UniverseAttribute
    {
        public SimpleAttribute(string name) : base(name){}

        override public string ToString()
        {
            return Name;
        }
    }
}

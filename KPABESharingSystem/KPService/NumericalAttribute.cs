using System;

namespace KPServices
{

    

    public class NumericalAttribute : UniverseAttribute
    {
        private UInt64 number;
        public UInt64 Number
        {
            get
            {
                return number;
            }
            set
            {
                if (value >= Math.Pow(2, NumberResolution))
                    throw new ArgumentOutOfRangeException("value", String.Format("The value {0} can't be stored in {1} bits", value, NumberResolution));
                number = value;
            }
        }
        public byte NumberResolution
        {
            get;
            set;
        } = 64;

        public NumericalAttribute(string name) : base(name) {}

        public NumericalAttribute(string name, UInt64 number) : base(name)
        {
            Number = number;
        }

        public NumericalAttribute(string name, UInt64 number, byte numberResolution) : base(name)
        {
            //the assignments must be in this order to properly check if number can stay in numberResolution bits
            NumberResolution = numberResolution;
            Number = number;
        }

        public override string ToString()
        {
            return $"{Name} = # {NumberResolution}";
        }
    }
}

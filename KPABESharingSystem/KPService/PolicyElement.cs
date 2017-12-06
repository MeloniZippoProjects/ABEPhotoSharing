using System;
using System.Linq;

namespace KPServices
{
    public enum PolicyType
    {
        Equal,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual
    }

    public class PolicyElement
    {
        private readonly UniverseAttribute _attribute;

        private readonly PolicyType _policyType;


        public PolicyElement(UniverseAttribute numericalAttribute, PolicyType policyType = PolicyType.Equal)
        {
            _attribute = numericalAttribute;

            PolicyType maxValue = Enum.GetValues(typeof(PolicyType)).Cast<PolicyType>().Max();
            if (!(0 <= policyType && policyType <= maxValue))
                throw new ArgumentOutOfRangeException(nameof(policyType), "The value is not a valid PolicyType");
            _policyType = policyType;
        }

        public override string ToString()
        {
            if (_attribute is NumericalAttribute numericalAttribute)
            {
                String policyComparator;
                switch (_policyType)
                {
                    case PolicyType.Equal:
                        policyComparator = "=";
                        break;
                    case PolicyType.LessThan:
                        policyComparator = "<";
                        break;
                    case PolicyType.GreaterThan:
                        policyComparator = ">";
                        break;
                    case PolicyType.LessThanOrEqual:
                        policyComparator = "<=";
                        break;
                    case PolicyType.GreaterThanOrEqual:
                        policyComparator = ">=";
                        break;
                    default:
                        policyComparator = "";
                        break;
                }

                //return numericalAttribute.Name + policyComparator + numericalAttribute.Number + "#" + numericalAttribute.NumberResolution;
                return numericalAttribute.ToString().Replace("=", policyComparator);
            }
            return _attribute.Name;
        }
    }
}
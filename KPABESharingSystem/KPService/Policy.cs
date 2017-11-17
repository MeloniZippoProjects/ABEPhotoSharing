using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class Policy
    {
        private UniverseAttribute Attribute;

        private PolicyType PolicyType;


        public Policy(UniverseAttribute numericalAttribute, PolicyType policyType = PolicyType.Equal)
        {
            Attribute = numericalAttribute;

            PolicyType maxValue = Enum.GetValues(typeof(PolicyType)).Cast<PolicyType>().Max();
            if (!(0 <= policyType && policyType <= maxValue))
                throw new ArgumentOutOfRangeException("policyType", "The value is not a valid PolicyType");
            PolicyType = policyType;
        }

        public override string ToString()
        {
            String policyComparator;

            if (Attribute is NumericalAttribute numericalAttribute)
            {
                switch (PolicyType)
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

                return numericalAttribute.Name + policyComparator + numericalAttribute.Number + "#" + numericalAttribute.NumberResolution;
            }
            else
            {
                return Attribute.Name;
            }
        }
    }
}

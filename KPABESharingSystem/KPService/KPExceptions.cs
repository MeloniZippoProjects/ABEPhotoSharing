using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    public class SuiteException : Exception
    {
        public SuiteException() {}
        public SuiteException(String message) : base(message){}
    }

    public class ToolNotFound : SuiteException
    {
        public ToolNotFound(){}
        public ToolNotFound(String message) : base(message){}
    }

    public class SetupException : SuiteException
    {
        public SetupException() { }
        public SetupException(String message) : base(message){ }
    }

    public class UniverseNotDefined : SuiteException
    {
        public UniverseNotDefined() { }
        public UniverseNotDefined(String message) : base(message){ }
    }

    public class KeygenException : SuiteException
    {
        public KeygenException() { }
        public KeygenException(String message) : base(message){ }
    }

    public class TrivialPolicy : KeygenException
    {
        public TrivialPolicy(){}
        public TrivialPolicy(string message) : base(message){}
    }

    public class UnsatisfiablePolicy : KeygenException
    {
        public UnsatisfiablePolicy(){}
        public UnsatisfiablePolicy(string message) : base(message){}
    }

    public class AttributeNotFound : KeygenException
    {
        public AttributeNotFound() { }
        public AttributeNotFound(string message) : base(message) { }
    }

    public class EncryptException : SuiteException
    {
        public EncryptException() { }
        public EncryptException(String message) : base(message){ }
    }

    public class DecryptException : SuiteException
    {
        public DecryptException() { }
        public DecryptException(string message) : base(message) { }
    }

    public class PolicyUnsatisfied : DecryptException
    {
        public PolicyUnsatisfied() { }
        public PolicyUnsatisfied(string message) : base(message) { }
    }
}
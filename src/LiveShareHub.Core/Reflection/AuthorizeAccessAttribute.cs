using System;
using System.Collections.Generic;
using System.Text;

namespace LiveShareHub.Core.Reflection
{
    public class AuthorizeAccessAttribute : Attribute
    {
        public AuthorizeAccessAttribute(AuthorizationType types)
        {
            this.AuthorizationType = types;
        }

        public AuthorizationType AuthorizationType { get; }
    }

    [Flags]
    public enum AuthorizationType
    {
        Basic = 1
    }
}

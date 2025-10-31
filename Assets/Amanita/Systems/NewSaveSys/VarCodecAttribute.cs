using System;
using System.Collections.Generic;

namespace Amanita.SaveSys
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VarCodecAttribute : Attribute
    {
        public bool Active { get; protected set; }
        public IList<Type> SupportedContentTypes { get; protected set; }

        /// <summary>
        /// Registers the codec for the given content types. If active, the codec will be used for those types.
        /// </summary>
        public VarCodecAttribute(bool active, params Type[] supportedContentTypes)
        {
            Active = active;
            SupportedContentTypes = supportedContentTypes;
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeValidate.Test.TestData
{
    public class ARequest : HttpWebRequest
    {
        public ARequest(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        public ClaimsPrincipal User { get {
                GenericIdentity MyIdentity = new GenericIdentity("MyIdentity");
                // Create generic principal.  
                String[] MyStringArray = { "Manager", "Teller" };
                GenericPrincipal MyPrincipal =
                    new GenericPrincipal(MyIdentity, MyStringArray);

                Thread.CurrentPrincipal = MyPrincipal;
                return MyPrincipal;
            }
            set => throw new NotImplementedException(); }

    }
}

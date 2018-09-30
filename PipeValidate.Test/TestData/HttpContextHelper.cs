using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace PipeValidate.Test.TestData
{
	public static class HttpContextHelper
	{
        public static void SetObjectAsJson(this HttpContextAccessor accessor, string key, object value)
        {
            accessor.HttpContext.Session.Set(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        public static T GetObjectFromJson<T>(this HttpContextAccessor accessor, string key)
        {
            if (accessor.HttpContext.Session.TryGetValue(key, out var bytes)) {
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
            }
            return default(T);
        }

        public static ClaimsPrincipal GetUser()
        {
            var user = new ClaimsPrincipal();

            GenericIdentity MyIdentity = new GenericIdentity("MyIdentity");

            // Create generic principal.  
            String[] MyStringArray = { "Manager", "Teller" };
            GenericPrincipal MyPrincipal = new GenericPrincipal(MyIdentity, MyStringArray);
            Thread.CurrentPrincipal = MyPrincipal;
            user.AddIdentity(new ClaimsIdentity(MyPrincipal.Claims));

            return user;

        }
        public static HttpContextAccessor SetUpHttpContextAccessor()
        {
            var cntAccessor = new HttpContextAccessor();
            cntAccessor.HttpContext = new DefaultHttpContext();
            cntAccessor.HttpContext.Session = new SessionHandler();
            cntAccessor.HttpContext.Session.Set("tenant", Encoding.UTF8.GetBytes("Ikke"));

            var handler = new AuthHandler();
            var user = new ClaimsPrincipal();

            GenericIdentity MyIdentity = new GenericIdentity("MyIdentity");
            String[] MyStringArray = { "Manager", "Teller" };
            GenericPrincipal MyPrincipal = new GenericPrincipal(MyIdentity, MyStringArray);
            
            Thread.CurrentPrincipal = MyPrincipal;
            user.AddIdentity(new ClaimsIdentity(MyPrincipal.Claims, "aAuthenticationType"));
            cntAccessor.HttpContext.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature() { Handler = handler, User = user });

            return cntAccessor;
        }

        private class SessionHandler : ISession
        {
            private Dictionary<string, Byte[]> SessionKeys = new Dictionary<string, byte[]>();
            public bool IsAvailable => throw new NotImplementedException();

            public string Id => Guid.NewGuid().ToString();

            public IEnumerable<string> Keys => SessionKeys.Keys;

            public void Clear()
            {
                SessionKeys = new Dictionary<string, byte[]>();
            }

            public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public void Remove(string key)
            {
                SessionKeys.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                SessionKeys.Add(key, value);
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return SessionKeys.TryGetValue(key, out value);
            }
        }

        private class AuthHandler : IAuthenticationHandler
        {

            public bool SignedIn { get; set; }

            public Task AuthenticateAsync(AuthenticateContext context)
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(ChallengeContext context)
            {
                throw new NotImplementedException();
            }

            public void GetDescriptions(DescribeSchemesContext context)
            {
                throw new NotImplementedException();
            }

            public Task SignInAsync(SignInContext context)
            {
                SignedIn = true;
                context.Accept();
                return Task.FromResult(0);
            }

            public Task SignOutAsync(SignOutContext context)
            {
                SignedIn = false;
                context.Accept();
                return Task.FromResult(0);
            }
        }
    }
}

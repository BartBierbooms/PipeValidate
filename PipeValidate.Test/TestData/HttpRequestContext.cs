using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PipeValidate.Test.TestData
{
	public class HttpRequestContext
	{
		public const string notAuthenticated = "The request is invalid vbecause it is not authenticated";
        public const string inValidRole = "The User down't have the right role";

        public readonly HttpContextAccessor ContextAccessor;
        private readonly ClaimsPrincipal claimsPrincipal;

        public HttpRequestContext() { }
        public HttpRequestContext(HttpContextAccessor context, ClaimsPrincipal claimsPrincipal) {
            ContextAccessor = context;
            this.claimsPrincipal = claimsPrincipal;
        }


		public void ValidateIsAuthenticated(Result ret)
		{

            ContextAccessor.HttpContext.Authentication.SignInAsync("ignore", claimsPrincipal);
            var authenticated = ContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated;
            if (!authenticated.GetValueOrDefault(false)) {
                ret.AddValidationMessage(notAuthenticated);
            }


		}

        public void ValidateIsManager_WhichSucceeds(Result ret)
        {
            var isManager = this.ContextAccessor?.HttpContext?.User?.IsInRole("Manager");
            if (!isManager.GetValueOrDefault(false))
            {
                ret.AddValidationMessage(inValidRole);
            }
        }

        public void ValidateIsEmployee_WhichFails(Result ret)
        {
            var isEmployee = this.ContextAccessor?.HttpContext?.User?.IsInRole("Employee");
            if (!isEmployee.GetValueOrDefault(false))
            {
                ret.AddValidationMessage(inValidRole);
            }
        }
    }
}
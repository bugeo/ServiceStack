﻿using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Host;
using ServiceStack.Testing;
using ServiceStack.Utils;
using ServiceStack.Web;

namespace ServiceStack.Common.Tests.OAuth
{
	[TestFixture]
	public class CredentialsServiceTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			AuthenticateService.Init(() => new AuthUserSession(),
				new CredentialsAuthProvider());           
		}

		public AuthenticateService GetAuthService()
		{
		    var authService = new AuthenticateService {
                RequestContext = new MockRequestContext(),
                //ServiceExceptionHandler = (req, ex) =>
                //    ValidationFeature.HandleException(new BasicResolver(), req, ex)
            };
		    return authService;
		}

        class ValidateServiceRunner<T> : ServiceRunner<T>
        {
            public ValidateServiceRunner(IAppHost appHost, ActionContext actionContext)
                : base(appHost, actionContext) {}

            public override object HandleException(IRequestContext requestContext, T request, System.Exception ex)
            {
                return DtoUtils.CreateErrorResponse(request, ex);
            }
        }

        public object GetAuthService(AuthenticateService authService, Authenticate request)
        {
            var serviceRunner = new ValidateServiceRunner<Authenticate>(null, new ActionContext {
                Id = "GET Auth",
                ServiceAction = (service, req) => ((AuthenticateService)service).Get((Authenticate)req)
            });

            return serviceRunner.Process(authService.RequestContext, authService, request);
        }

	    [Test]
		public void Empty_request_invalidates_all_fields()
		{
            using (new BasicAppHost().Init())
            {
                var authService = GetAuthService();

                var response = (HttpError)GetAuthService(authService, new Authenticate());
                var errors = response.GetFieldErrors();

                Assert.That(errors.Count, Is.EqualTo(2));
                Assert.That(errors[0].ErrorCode, Is.EqualTo("NotEmpty"));
                Assert.That(errors[0].FieldName, Is.EqualTo("UserName"));
                Assert.That(errors[1].ErrorCode, Is.EqualTo("NotEmpty"));
                Assert.That(errors[1].FieldName, Is.EqualTo("Password"));
            }
		}

		[Test]
		public void Requires_UserName_and_Password()
		{
            using (new BasicAppHost().Init())
            {
                var authService = GetAuthService();

                var response = (HttpError)GetAuthService(authService,
                    new Authenticate { provider = AuthenticateService.CredentialsProvider });

                var errors = response.GetFieldErrors();

                Assert.That(errors.Count, Is.EqualTo(2));
                Assert.That(errors[0].ErrorCode, Is.EqualTo("NotEmpty"));
                Assert.That(errors[0].FieldName, Is.EqualTo("UserName"));
                Assert.That(errors[1].FieldName, Is.EqualTo("Password"));
                Assert.That(errors[1].ErrorCode, Is.EqualTo("NotEmpty"));
            }
		}
	}
}
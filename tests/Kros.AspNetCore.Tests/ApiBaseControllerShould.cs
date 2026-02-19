using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace Kros.AspNetCore.Tests
{
    public class ApiBaseControllerShould
    {
        #region Test classes

        public class TestApiBaseController : ApiBaseController
        { }

        #endregion

        [Fact]
        public void BeAuthorized()
        {
            Assert.NotNull(GetControllerAttribute<AuthorizeAttribute>(new TestApiBaseController()));
        }

        [Fact]
        public void BeApiController()
        {
            Assert.NotNull(GetControllerAttribute<ApiControllerAttribute>(new TestApiBaseController()));
        }

        [Fact]
        public void HasRouteAttribute()
        {
            Assert.NotNull(GetControllerAttribute<RouteAttribute>(new TestApiBaseController()));
        }

        private static T GetControllerAttribute<T>(ApiBaseController controller) where T : Attribute
        {
            Type type = controller.GetType();
            object[] attributes = type.BaseType.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Length == 0 ? null : (T)attributes[0];
            return attribute;
        }
    }
}

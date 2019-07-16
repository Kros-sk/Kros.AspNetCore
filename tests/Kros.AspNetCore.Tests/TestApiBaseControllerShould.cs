using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Xunit;

namespace Kros.AspNetCore.Tests
{
    public class TestApiBaseControllerShould
    {
        #region Test classes

        public class TestApiBaseController : ApiBaseController
        { }

        #endregion

        [Fact]
        public void BeAuthorized()
        {
            GetControllerAttribute<AuthorizeAttribute>(new TestApiBaseController()).Should().NotBeNull();
        }

        [Fact]
        public void BeApiController()
        {
            GetControllerAttribute<ApiControllerAttribute>(new TestApiBaseController()).Should().NotBeNull();
        }

        [Fact]
        public void HasRouteAttribute()
        {
            GetControllerAttribute<RouteAttribute>(new TestApiBaseController()).Should().NotBeNull();
        }

        private static T GetControllerAttribute<T>(ApiBaseController controller) where T : Attribute
        {
            Type type = controller.GetType();
            object[] attributes = type.BaseType.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Count() == 0 ? null : (T)attributes[0];
            return attribute;
        }
    }
}

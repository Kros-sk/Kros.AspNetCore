using FluentAssertions;
using Kros.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.JsonPatch
{
    public class JsonPatchDocumentExtensionsShould
    {
        [Fact]
        public void GetColumnsNamesForFlatModelWithoutConfiguration()
        {
            var jsonPatch = new JsonPatchDocument<Foo>();
            jsonPatch.Replace(p => p.Property1, "Value");

            var columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("Property1");
        }

        [Fact]
        public void GetMoreColumnsNamesForFlatModelWithoutConfiguration()
        {
            var jsonPatch = new JsonPatchDocument<Foo>();
            jsonPatch.Replace(p => p.Property1, "Value");
            jsonPatch.Replace(p => p.Property2, "Value");

            var columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("Property1", "Property2");
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithFlattening()
        {
            var jsonPatch = new JsonPatchDocument<Document>();

            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            var columns = jsonPatch.GetColumnsNames(new JsonPatchMapperConfig<Document>());
            columns.Should()
                .BeEquivalentTo("SupplierName");
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithDefaultMapping()
        {
            JsonPatchMapperConfig<Document>
                .NewConfig()
                .Map(src =>
                {
                    const string address = ".Address.";

                    var index = src.IndexOf(address);
                    if (index > -1)
                    {
                        return src.Remove(index, address.Length);
                    }

                    return src;
                });

            var jsonPatch = new JsonPatchDocument<Document>();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            var columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("SupplierCountry", "SupplierName");
        }

        [Fact]
        public void NoMapProperties()
        {
            var config = new JsonPatchMapperConfig<Document>()
                .Map(src =>
                {
                    if (src.Contains(".Address."))
                    {
                        return null;
                    }

                    return src;
                });

            var jsonPatch = new JsonPatchDocument<Document>();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            var columns = jsonPatch.GetColumnsNames(config);
            columns.Should()
                .BeEquivalentTo("SupplierName");
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithCustomMapping()
        {
            var config = new JsonPatchMapperConfig<Document>()
                .Map(src =>
                {
                    const string address = ".Address.";

                    var index = src.IndexOf(address);
                    if (index > -1)
                    {
                        return src.Remove(index, address.Length);
                    }

                    return src;
                });

            var jsonPatch = new JsonPatchDocument<Document>();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");

            var columns = jsonPatch.GetColumnsNames(config);
            columns.Should()
                .BeEquivalentTo("SupplierCountry");
        }

        #region Nested classes

        public class Document
        {
            public Partner Supplier { get; set; } = new Partner();

            public IEnumerable<string> Items { get; set; }
        }

        public class Partner
        {
            public string Name { get; set; }

            public Address Address { get; set; } = new Address();
        }

        public class Address
        {
            public string Country { get; set; }
        }

        public class Foo
        {
            public string Property1 { get; set; }

            public string Property2 { get; set; }
        }

        #endregion
    }
}

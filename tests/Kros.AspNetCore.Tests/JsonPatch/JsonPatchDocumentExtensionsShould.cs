using FluentAssertions;
using Kros.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.JsonPatch
{
    public class JsonPatchDocumentExtensionsShould
    {
        [Fact]
        public void GetColumnsNamesForFlatModelWithoutConfiguration()
        {
            JsonPatchDocument<Foo> jsonPatch = new();
            jsonPatch.Replace(p => p.Property1, "Value");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("Property1");
        }

        [Fact]
        public void GetMoreColumnsNamesForFlatModelWithoutConfiguration()
        {
            JsonPatchDocument<Foo> jsonPatch = new();
            jsonPatch.Replace(p => p.Property1, "Value");
            jsonPatch.Replace(p => p.Property2, "Value");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("Property1", "Property2");
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithFlattening()
        {
            JsonPatchDocument<Document> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(new JsonPatchMapperConfig<Document>());
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
                    const string address = "/Address/";

                    int index = src.IndexOf(address);
                    if (index > -1)
                    {
                        return src.Remove(index, address.Length);
                    }

                    return src;
                });

            JsonPatchDocument<Document> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames();
            columns.Should()
                .BeEquivalentTo("SupplierCountry", "SupplierName");
        }

        [Fact]
        public void NoMapProperties()
        {
            JsonPatchMapperConfig<Document> config = new JsonPatchMapperConfig<Document>()
                .Map(src =>
                {
                    if (src.Contains("/Address/"))
                    {
                        return null;
                    }

                    return src;
                });

            JsonPatchDocument<Document> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(config);
            columns.Should()
                .BeEquivalentTo("SupplierName");
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithCustomMapping()
        {
            JsonPatchMapperConfig<Document> config = new JsonPatchMapperConfig<Document>()
                .Map(src =>
                {
                    const string address = "/Address/";

                    int index = src.IndexOf(address);
                    if (index > -1)
                    {
                        return src.Remove(index, address.Length);
                    }

                    return src;
                });

            JsonPatchDocument<Document> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(config);
            columns.Should()
                .BeEquivalentTo("SupplierCountry");
        }

        [Fact]
        public void MapPathToColumnsNamesAsPascalCase()
        {
            JsonPatchDocument jsonPatch = new();
            jsonPatch.Replace("/supplier/address/country", "Slovakia");

            string serialized = JsonConvert.SerializeObject(jsonPatch);
            JsonPatchDocument<Document> deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<Document>>(serialized);

            IEnumerable<string> columns = deserialized.GetColumnsNames(new JsonPatchMapperConfig<Document>());
            columns.Should()
                .BeEquivalentTo("SupplierAddressCountry");
        }

        [Fact]
        public void UseCaseSensitiveCaching()
        {
            JsonPatchDocument jsonPatch = new();
            jsonPatch.Replace("pRoperty1", "Value");
            string serialized = JsonConvert.SerializeObject(jsonPatch);
            JsonPatchDocument<Document> deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<Document>>(serialized);

            JsonPatchDocument jsonPatch2 = new();
            jsonPatch2.Replace("property1", "Value");
            string serialized2 = JsonConvert.SerializeObject(jsonPatch2);
            JsonPatchDocument<Document> deserialized2 = JsonConvert.DeserializeObject<JsonPatchDocument<Document>>(serialized2);

            IEnumerable<string> columns = deserialized.GetColumnsNames();
            IEnumerable<string> columns2 = deserialized2.GetColumnsNames();

            columns.Should()
                .BeEquivalentTo("PRoperty1");
            columns2.Should()
                .BeEquivalentTo("Property1");
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

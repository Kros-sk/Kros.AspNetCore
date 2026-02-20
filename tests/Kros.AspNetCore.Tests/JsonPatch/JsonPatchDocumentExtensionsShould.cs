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
            Assert.Equivalent(new[] { "Property1" }, columns);
        }

        [Fact]
        public void GetMoreColumnsNamesForFlatModelWithoutConfiguration()
        {
            JsonPatchDocument<Foo> jsonPatch = new();
            jsonPatch.Replace(p => p.Property1, "Value");
            jsonPatch.Replace(p => p.Property2, "Value");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames();
            Assert.Equivalent(new[] { "Property1", "Property2" }, columns);
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithFlattening()
        {
            JsonPatchDocument<Document1> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(JsonPatchMapperConfig<Document1>.NewConfig());
            Assert.Equivalent(new[] { "SupplierName" }, columns);
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithDefaultMapping()
        {
            JsonPatchMapperConfig<Document5>
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

            JsonPatchDocument<Document5> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames();
            Assert.Equivalent(new[] { "SupplierCountry", "SupplierName" }, columns);
        }

        [Fact]
        public void NoMapProperties()
        {
            JsonPatchMapperConfig<Document2> config = JsonPatchMapperConfig<Document2>.NewConfig()
                .Map(src =>
                {
                    if (src.Contains("/Address/"))
                    {
                        return null;
                    }

                    return src;
                });

            JsonPatchDocument<Document2> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
            jsonPatch.Replace(p => p.Supplier.Name, "Bob");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(config);
            Assert.Equivalent(new[] { "SupplierName" }, columns);
        }

        [Fact]
        public void GetColumnsNamesForComplexTypeWithCustomMapping()
        {
            JsonPatchMapperConfig<Document3> config = JsonPatchMapperConfig<Document3>.NewConfig()
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

            JsonPatchDocument<Document3> jsonPatch = new();
            jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");

            IEnumerable<string> columns = jsonPatch.GetColumnsNames(config);
            Assert.Equivalent(new[] { "SupplierCountry" }, columns);
        }

        [Fact]
        public void MapPathToColumnsNamesAsPascalCase()
        {
            JsonPatchDocument jsonPatch = new();
            jsonPatch.Replace("/supplier/address/country", "Slovakia");

            string serialized = JsonConvert.SerializeObject(jsonPatch);
            JsonPatchDocument<Document4> deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<Document4>>(serialized);

            IEnumerable<string> columns = deserialized.GetColumnsNames(JsonPatchMapperConfig<Document4>.NewConfig());
            Assert.Equivalent(new[] { "SupplierAddressCountry" }, columns);
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

            Assert.Equivalent(new[] { "PRoperty1" }, columns);
            Assert.Equivalent(new[] { "Property1" }, columns2);
        }

        #region Nested classes

        public class Document
        {
            public Partner Supplier { get; set; } = new Partner();
            public IEnumerable<string> Items { get; set; }
        }

        public class Document1
        {
            public Partner Supplier { get; set; } = new Partner();
            public IEnumerable<string> Items { get; set; }
        }

        public class Document2
        {
            public Partner Supplier { get; set; } = new Partner();
            public IEnumerable<string> Items { get; set; }
        }

        public class Document3
        {
            public Partner Supplier { get; set; } = new Partner();
            public IEnumerable<string> Items { get; set; }
        }

        public class Document4
        {
            public Partner Supplier { get; set; } = new Partner();
            public IEnumerable<string> Items { get; set; }
        }

        public class Document5
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

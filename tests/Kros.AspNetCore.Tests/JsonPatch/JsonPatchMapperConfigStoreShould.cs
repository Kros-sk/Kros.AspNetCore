using Kros.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;

namespace Kros.AspNetCore.Tests.JsonPatch
{
    public class JsonPatchMapperConfigStoreShould
    {
        private const int ConcurrentCallers = 8;

        [Fact]
        public void ServeTheSameConfigToConcurrentFirstCallers()
        {
            ConcurrentQueue<Exception> errors = new();

            // The store is a static singleton, so every model type gets exactly one first call per test run.
            // Several types are used because a single one leaves only one instant in which the race can happen.
            RequestColumnsConcurrently<ConcurrentModel<byte>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<short>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<int>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<long>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<float>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<double>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<decimal>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<string>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<Guid>>(errors);
            RequestColumnsConcurrently<ConcurrentModel<DateTime>>(errors);

            Assert.True(errors.IsEmpty, string.Join(Environment.NewLine, errors));
        }

        [Fact]
        public void StillRejectANewConfigForATypeItAlreadyServed()
        {
            JsonPatchDocument<ServedModel> jsonPatch = new();
            jsonPatch.Replace(p => p.Property1, "Value");
            jsonPatch.GetColumnsNames();

            Assert.Throws<InvalidOperationException>(() => JsonPatchMapperConfig<ServedModel>.NewConfig());
        }

        private static void RequestColumnsConcurrently<TModel>(ConcurrentQueue<Exception> errors)
            where TModel : ConcurrentModel, new()
        {
            JsonPatchDocument<TModel> jsonPatch = new();
            jsonPatch.Replace(p => p.Property1, "Value");

            using Barrier barrier = new(ConcurrentCallers);
            Thread[] threads = new Thread[ConcurrentCallers];

            for (int i = 0; i < ConcurrentCallers; i++)
            {
                threads[i] = new Thread(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        jsonPatch.GetColumnsNames();
                    }
                    catch (Exception ex)
                    {
                        errors.Enqueue(ex);
                    }
                });
                threads[i].Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        public class ServedModel
        {
            public string Property1 { get; set; }
        }

        public abstract class ConcurrentModel
        {
            public string Property1 { get; set; }
        }

        public class ConcurrentModel<T> : ConcurrentModel
        {
            public T Property2 { get; set; }
        }
    }
}

using System;

namespace Kros.MassTransit.AzureServiceBus.Extensions
{
    /// <summary>
    /// Extensions working with azure service bus endpoint.
    /// </summary>
    public static class EndpointExtensions
    {
        private const string MachineNamePlaceholder = "{MACHINE_NAME}";

        /// <summary>
        /// Returns the name of the endpoint if isDevelopment == false.
        /// If isDevelopment == true and name of the endpoint contains machine name
        /// placeholder replaces machine name placeholder with machine name or guid.
        /// See <see cref="ReplaceMachineNamePlaceholder"/>.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="isDevelopment"></param>
        /// <param name="machineNamePlaceholder"></param>
        /// <returns></returns>
        public static string GetEndpointName(
            this AzureServiceBusOptions.AzureServiceBusEndpoint endpoint,
            bool isDevelopment,
            string machineNamePlaceholder = MachineNamePlaceholder)
        {
            string endpointName = endpoint.Name;

            if (!isDevelopment)
            {
                return endpointName;
            }

            if (endpointName.Contains(machineNamePlaceholder, StringComparison.OrdinalIgnoreCase))
            {
                endpointName = endpointName.ReplaceMachineNamePlaceholder(machineNamePlaceholder);
            }

            return endpointName;
        }

        /// <summary>
        /// Replaces machine name placeholder in string with machine name
        /// or GUID (if it is not possible to retrieve machine name).
        /// </summary>
        /// <param name="text"></param>
        /// <param name="machineNamePlaceholder"></param>
        /// <returns></returns>
        public static string ReplaceMachineNamePlaceholder(this string text, string machineNamePlaceholder)
        {
            string machineName = GetMachineNameOrGuid();
            return text.Replace(machineNamePlaceholder, machineName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetMachineNameOrGuid()
        {
            string machineName = GetMachineName();

            if (string.IsNullOrEmpty(machineName))
            {
                machineName = Guid.NewGuid().ToString("N");
            }

            return machineName.ToLower();
        }

        private static string GetMachineName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}

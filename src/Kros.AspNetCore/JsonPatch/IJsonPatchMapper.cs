namespace Kros.AspNetCore.JsonPatch
{
    /// <summary>
    /// Mapping JSON patch operation path to database column name.
    /// </summary>
    internal interface IJsonPatchMapper
    {
        /// <summary>
        /// Get database column name.
        /// </summary>
        /// <param name="path">JSON PATCH path.</param>
        /// <returns>Database column.</returns>
        string GetColumnName(string path);
    }
}

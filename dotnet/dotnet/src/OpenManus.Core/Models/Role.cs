namespace OpenManus.Core.Models
{
    /// <summary>
    /// Message role options for conversation
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// System message role
        /// </summary>
        System,

        /// <summary>
        /// User message role
        /// </summary>
        User,

        /// <summary>
        /// Assistant message role
        /// </summary>
        Assistant,

        /// <summary>
        /// Tool message role
        /// </summary>
        Tool
    }
}

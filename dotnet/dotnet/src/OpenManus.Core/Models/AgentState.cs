namespace OpenManus.Core.Models
{
    /// <summary>
    /// Agent execution states
    /// </summary>
    public enum AgentState
    {
        /// <summary>
        /// Agent is idle and ready to receive tasks
        /// </summary>
        Idle,

        /// <summary>
        /// Agent is currently running a task
        /// </summary>
        Running,

        /// <summary>
        /// Agent has finished its task successfully
        /// </summary>
        Finished,

        /// <summary>
        /// Agent encountered an error during execution
        /// </summary>
        Error
    }
}

using System;

namespace OpenManus.Core.Models
{
    public class Schema
    {
        // Define properties related to the application's data model here
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Add any additional properties or methods as needed
    }
}

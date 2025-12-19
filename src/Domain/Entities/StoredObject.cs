
using System;
using System.Collections.Generic;

namespace Domain.Entities
 {
            public class StoredObject
            {
                public Guid Id { get; set; }
                public string Key { get; set; } = default!;
                public string OriginalName { get; set; } = default!;
                public long SizeBytes { get; set; }
                public string ContentType { get; set; } = default!;
                public string Checksum { get; set; } = default!;
                public string? Tags { get; set; }
                public DateTime CreatedAtUtc { get; set; }
                public DateTime? DeletedAtUtc { get; set; }
                public int? Version { get; set; }
                public string CreatedByUserId { get; set; } = default!;
         
           public string Path { get; set; } = default!;


    }
}

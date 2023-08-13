using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerApi.Models
{
    /// <summary>
    /// Records are distinct from classes in that record types use value-based equality.
	/// Two variables of a record type are equal if the record type definitions are identical,
	/// and if for every field, the values in both records are equal.
    /// </summary>
    public record RequestMessage
    {
		public string Key { get; set; }
		[Required]
		public string Value { get; set; }
		[Required]
		public string Topic { get; set; }
	}
}


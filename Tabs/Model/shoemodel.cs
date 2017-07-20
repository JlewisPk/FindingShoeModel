using System;
using Newtonsoft.Json;

namespace Tabs
{
    public class shoemodel
    {
		[JsonProperty(PropertyName = "Id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

    }
}

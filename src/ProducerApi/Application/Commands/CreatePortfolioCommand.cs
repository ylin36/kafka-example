using System;
using System.Text.Json.Serialization;
using MediatR;

namespace ProducerApi.Application.Commands
{
	public class CreatePortfolioCommand : IRequest<int>
	{
        [JsonPropertyName("description")]
		public string Description { get; set; }

        public CreatePortfolioCommand()
		{
		}
	}
}


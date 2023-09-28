using System;
using MediatR;

namespace ProducerApi.Application.Commands
{
	public class CreateProductCommand : IRequest<int>
	{
		public CreateProductCommand()
		{
		}
	}
}


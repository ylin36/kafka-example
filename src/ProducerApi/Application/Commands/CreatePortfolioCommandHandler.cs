using Confluent.Kafka;
using MediatR;
using ProducerApi.Application.Abstractions;

namespace ProducerApi.Application.Commands
{
	public class CreatePortfolioCommandHandler : IRequestHandler<CreatePortfolioCommand, int>
	{
        private readonly IPortfolioRepository _portfolioRepository;
		public CreatePortfolioCommandHandler(IProducer<string, string> kafkaProducer, IPortfolioRepository portfolioRepository)
		{
            _portfolioRepository = portfolioRepository;
		}

        public Task<int> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
        {
            await _portfolioRepository.SavePortfoliosAsync

            throw new NotImplementedException();
        }
    }
}


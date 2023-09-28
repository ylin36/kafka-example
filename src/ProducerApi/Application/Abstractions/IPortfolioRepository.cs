using System;
using ProducerApi.Domain.Entities;

namespace ProducerApi.Application.Abstractions
{
	public interface IPortfolioRepository
	{
		Task<int> SavePortfoliosAsync(List<Portfolio> portfolios);
	}
}


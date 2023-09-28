using System;
using ProducerApi.Domain.Entities;

namespace ProducerApi.Application.Abstractions
{
	public interface IProductRepository
	{
        Task<int> SaveProductAsync(List<Portfolio> portfolios);
    }
}


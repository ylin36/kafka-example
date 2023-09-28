using System;
using System.Collections.Generic;

namespace ProducerApi.Domain.Entities;

public partial class Portfolio
{
    public int PortfolioId { get; set; }

    public Guid? PortfolioUuid { get; set; }

    public string Description { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

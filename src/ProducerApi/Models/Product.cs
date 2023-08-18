using System;
using System.Collections.Generic;

namespace ProducerApi.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int PortfolioId { get; set; }

    public Guid? ProductUuid { get; set; }

    public string Payload { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public virtual Portfolio Portfolio { get; set; } = null!;
}

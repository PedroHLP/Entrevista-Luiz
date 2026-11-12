using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestcom.Infrastructure.Persistencia.Configuracoes;

public class ItemPedidoConfig : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ITEMPED");
        builder.HasKey(i => new { i.NumeroPedido, i.Item });
        builder.Property(i => i.NumeroPedido).HasColumnName("NUMPED");
        builder.Property(i => i.Item).HasColumnName("ITEM");
        builder.Property(i => i.CodigoProduto).HasColumnName("CODPROD").HasMaxLength(10);
        builder.Property(i => i.Quantidade).HasColumnName("QTD").HasPrecision(10, 2);
        builder.Property(i => i.Preco).HasColumnName("PRECO").HasPrecision(12, 2);
        builder.Property(i => i.PercentualDesconto).HasColumnName("PERCDESC").HasPrecision(5, 2);
        builder.Property(i => i.ValorDesconto).HasColumnName("VLRDESC").HasPrecision(12, 2);
        builder.Property(i => i.ValorItem).HasColumnName("VLRITEM").HasPrecision(12, 2);
        builder.Property(i => i.AliquotaIcms).HasColumnName("ALIQICMS").HasPrecision(5, 2);
        builder.Property(i => i.ValorIcms).HasColumnName("VLRICMS").HasPrecision(12, 2);
    }
}

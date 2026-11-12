using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestcom.Infrastructure.Persistencia.Configuracoes;

public class PedidoConfig : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("PEDIDOS");
        builder.HasKey(p => p.Numero);
        builder.Property(p => p.Numero).HasColumnName("NUMPED");
        builder.Property(p => p.CodigoCliente).HasColumnName("CODCLI").HasMaxLength(8);
        builder.Property(p => p.Data).HasColumnName("DATA");
        builder.Property(p => p.Status).HasColumnName("STATUS").HasMaxLength(1);
        builder.Property(p => p.ValorBruto).HasColumnName("VLRBRUTO").HasPrecision(12, 2);
        builder.Property(p => p.ValorDesconto).HasColumnName("VLRDESC").HasPrecision(12, 2);
        builder.Property(p => p.ValorIcms).HasColumnName("VLRICMS").HasPrecision(12, 2);
        builder.Property(p => p.ValorTotal).HasColumnName("VLRTOTAL").HasPrecision(12, 2);
        builder.Property(p => p.Cfop).HasColumnName("CFOP").HasMaxLength(4);
        builder.Ignore(p => p.ValorDescontoFuncionario);
        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.NumeroPedido)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

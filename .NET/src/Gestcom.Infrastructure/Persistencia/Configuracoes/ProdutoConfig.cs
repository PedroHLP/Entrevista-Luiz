using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestcom.Infrastructure.Persistencia.Configuracoes;

public class ProdutoConfig : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("PRODUTOS");
        builder.HasKey(p => p.Codigo);
        builder.Property(p => p.Codigo).HasColumnName("CODPROD").HasMaxLength(10);
        builder.Property(p => p.Descricao).HasColumnName("DESCRICAO").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Ncm).HasColumnName("NCM").HasMaxLength(8).IsRequired();
        builder.Property(p => p.Preco).HasColumnName("PRECO").HasPrecision(12, 2);
        builder.Property(p => p.Estoque).HasColumnName("ESTOQUE").HasPrecision(10, 2);
        builder.Property(p => p.EstoqueMinimo).HasColumnName("ESTMIN").HasPrecision(10, 2);
        builder.Property(p => p.Ativo).HasColumnName("ATIVO");
    }
}

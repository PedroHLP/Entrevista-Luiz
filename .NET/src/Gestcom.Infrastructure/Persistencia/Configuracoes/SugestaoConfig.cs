using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestcom.Infrastructure.Persistencia.Configuracoes;

public class SugestaoConfig : IEntityTypeConfiguration<SugestaoReposicao>
{
    public void Configure(EntityTypeBuilder<SugestaoReposicao> builder)
    {
        builder.ToTable("SUGREPOS");
        builder.HasKey(s => s.CodigoProduto);
        builder.Property(s => s.CodigoProduto).HasColumnName("CODPROD").HasMaxLength(10);
        builder.Property(s => s.QuantidadeAtual).HasColumnName("QTDATUAL").HasPrecision(10, 2);
        builder.Property(s => s.QuantidadeMinima).HasColumnName("QTDMIN").HasPrecision(10, 2);
        builder.Property(s => s.QuantidadeSugerida).HasColumnName("QTDSUG").HasPrecision(10, 2);
        builder.Property(s => s.Data).HasColumnName("DATA");
    }
}

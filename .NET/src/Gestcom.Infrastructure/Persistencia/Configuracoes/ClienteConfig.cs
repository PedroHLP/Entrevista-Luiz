using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestcom.Infrastructure.Persistencia.Configuracoes;

public class ClienteConfig : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("CLIENTES");
        builder.HasKey(c => c.Codigo);
        builder.Property(c => c.Codigo).HasColumnName("CODCLI").HasMaxLength(8);
        builder.Property(c => c.Nome).HasColumnName("NOME").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Uf).HasColumnName("UF").HasMaxLength(2).IsRequired();
        builder.Property(c => c.Tipo)
            .HasColumnName("TIPO")
            .HasMaxLength(1)
            .HasConversion(
                v => v.ParaCodigo(),
                v => TipoClienteExtensoes.DeCodigo(v));
    }
}

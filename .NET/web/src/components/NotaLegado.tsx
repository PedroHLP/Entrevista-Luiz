type Props = {
  programas: string
  texto: string
}

export function NotaLegado({ programas, texto }: Props) {
  return (
    <aside className="nota-legado">
      <strong>Legado vs novo</strong>
      Substitui {programas.split(',').map((p, i) => (
        <span key={p}>
          {i > 0 ? ', ' : ''}
          <code>{p.trim()}</code>
        </span>
      ))}
      . {texto}
    </aside>
  )
}

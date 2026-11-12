# Gestcom — front-end da demonstração

Aplicação React + Vite + TypeScript que consome a API em `http://localhost:5088`. As telas espelham o menu do Visual FoxPro (`menu.prg`, `cad_produtos.prg`, `ped_inclui.prg`, `rel_reposicao.prg`).

## Como rodar

Em um terminal, suba a API (ela já carrega os dados de homologação na primeira execução):

```bash
cd ".NET"
dotnet run --project src/Gestcom.Api
```

Em outro:

```bash
cd ".NET/web"
npm install
npm run dev
```

Abra [http://localhost:5173](http://localhost:5173). O CORS da API libera essa origem.

`npm run build` gera a pasta `dist/` (não versionada).

A URL da API pode ser alterada com `VITE_API_URL` (padrão: `http://localhost:5088`).

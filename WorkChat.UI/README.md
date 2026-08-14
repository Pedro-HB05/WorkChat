# WorkChat.UI

Frontend do WorkChat em React e TypeScript, empacotado com Webpack (sem Vite).

## Executar localmente

1. Inicie a API pelo perfil HTTP, em `http://localhost:5019`.
2. Instale as dependências com `npm install`.
3. Inicie o frontend com `npm start`.
4. Acesse `http://localhost:3000`.

Por padrão, as rotas `/api` e `/hubs` usam `https://workchat-rvup.onrender.com`. Para usar outra origem, defina `WORKCHAT_API_URL` antes do build.

## Scripts

- `npm start`: servidor local com atualização automática.
- `npm run typecheck`: valida os tipos TypeScript.
- `npm run build`: gera a versão de produção em `dist/`.

## Organização

- `src/app`: rotas principais.
- `src/auth`: sessão e proteção de rotas.
- `src/components`: componentes de layout e interface.
- `src/pages`: páginas da aplicação.
- `src/services`: acesso à API e armazenamento local.
- `src/styles`: estilos globais e responsivos.
- `src/types`: contratos TypeScript alinhados aos DTOs da API.

## Rotas principais

- `/atendimento/:empresaId`: entrada pública do cliente, sem login.
- `/atendimento/:empresaId/chat/:conversaId`: chat público do cliente.
- `/conversas`: console de atendimento da equipe.
- `/clientes`, `/setores`, `/usuarios`: gestão administrativa.
- `/configuracoes`: dados da empresa e link público de atendimento.

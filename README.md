# Bling MCP

**Model Context Protocol (MCP) Integration para o ERP Bling**

Este projeto implementa um servidor MCP (Model Context Protocol) escrito em `.NET`, que expõe uma série de ferramentas (tools) conectadas à API v3 do Bling. Ele permite que assistentes virtuais de IA (como Claude Desktop, Antigravity, Cursor, etc.) obtenham, consultem e analisem dados de pedidos de venda, produtos, contatos e finanças de forma direta, realizando requisições reais para sua conta do Bling.

---

## 1. Configuração do Aplicativo no site do Bling

Para conectar este servidor MCP à sua conta Bling, será necessário criar um Aplicativo no portal de desenvolvedores (Bling Developers) para a obtenção das credenciais de API (`Client ID` e `Client Secret`).

### Passo a Passo:
1. Acesse o sistema do Bling e vá até **Preferências > Integrações > Configurações de Integrações** (ou acesse diretamente pelo [Portal de Desenvolvedores do Bling](https://developer.bling.com.br/)).
2. Clique em **Aplicativos** e logo após em "Criar Aplicativo".
3. Preencha as informações do aplicativo. O mais importante é o campo **URI de Redirecionamento (Redirect URI)**. Ele **DEVE OBRIGATORIAMENTE** ser preenchido com:
   `http://localhost:8087/callback/`
   *(Esta é a URL que o servidor local escutará para capturar o código de autenticação OAuth2).*
4. Selecione os **Escopos** necessários. Para que todas as ferramentas do MCP funcionem, adicione permissões de leitura para todos os módulos que você pretende explorar (ex: Produtos, Pedidos e Cadastros, Finanças, Estoques, etc). Pensando numa futura expansão do MCP, você pode permitir também os escopos de escrita.
5. Salve o aplicativo. O sistema lhe fornecerá um **Client ID** e um **Client Secret**. Guarde estas chaves de integração, pois elas alimentarão o ambiente.

---

## 2. Variáveis de Ambiente (Configuração do Sistema)

O servidor precisa saber quais credenciais utilizar. Existem duas formas de configurar o sistema: definindo variáveis do sistema operacional (ex: na configuração do MCP Client), ou criando um arquivo `.env` localmente.

### Variáveis Obrigatórias:
- `BLING_CLIENT_ID`: O Client ID gerado para o seu aplicativo no passo anterior.
- `BLING_CLIENT_SECRET`: O Client Secret gerado para o aplicativo no passo anterior.

### Variável Opcional (Uso de Banco de Dados PostgreSQL):
- `BLING_POSTGRES_CONNECTION`: Exemplo: `Host=localhost;Database=bling_mcp;Username=bling;Password=root`
  - **Se não for informada:** O sistema utilizará cache em memória RAM e salvará os tokens do OAuth localmente num arquivo JSON em sua máquina (`%APPDATA%\bling-mcp\bling_tokens.json`). Esse comportamento pode ser problemático, por causa da necessidade de consultar os mesmos pedidos caso eles precisem ser analisados novamente, o que irá consumir muito tempo e chamadas na API.
  - **Se for informada:** O sistema persistirá os tokens de autenticação, o resultado em cache e fará o gerenciamento das Background Tasks (tarefas de longa duração e elaboração de relatórios, utilizando campos `JSONB`) através dos repositórios nativos no PostgreSQL, garantindo maior desempenho para altos volumes de dados. RECOMENDADO!
  - **Requisito:** Você precisará estabelecer um banco de dados PostgreSQL em sua máquina se quiser utilizar o sistema de cache.

Exemplo de um arquivo `.env` para uso local (você pode criá-lo na mesma pasta onde o executável residir):
```env
BLING_CLIENT_ID=5c...
BLING_CLIENT_SECRET=de47...
BLING_POSTGRES_CONNECTION=Host=localhost;Database=bling_mcp;Username=postgres;Password=123
```

---

## 3. Como Rodar e Estabelecer o Sistema

Para inicializar a integração, sendo uma aplicação Console em `.NET` utilizando pacotes MCP com protocolo StdIO (standard input/output), aconselhamos compilar/publicar o projeto primeiro para que ele seja executado isoladamente por um cliente MCP local.

### 3.1 Publicando o Executável (.NET 8+)
Abra o seu terminal na pasta do repositório (`src`) e publique o projeto `Bling.Mcp`:

```bash
dotnet publish src/Bling.Mcp/Bling.Mcp.csproj -c Release -o ./publish
```

### 3.2 Configurando no seu Cliente MCP (Claude Desktop / Antigravity / Cursor)
Você precisa informar o caminho absoluto do servidor publicado dentro das configurações do cliente de IA. 
Adicione ao arquivo de configurações (por exemplo `claude_desktop_config.json` ou `mcp_config.json` do ecossistema local):

```json
{
  "mcpServers": {
    "bling-mcp": {
      "command": "C:\\Caminho\\Absoluto\\Para\\O\\Repo\\src\\publish\\Bling.Mcp.exe",
      "env": {
        "BLING_CLIENT_ID": "seu_client_id_aqui",
        "BLING_CLIENT_SECRET": "seu_client_secret_aqui",
        "BLING_POSTGRES_CONNECTION": "Host=localhost;Database=bling_mcp;Username=postgres;Password=123"
      },
      "disabled": false
    }
  }
}
```
*Dica: Informar as variáveis de ambiente diretamente na configuração do JSON (`env`) é uma maneira nativa e muito fácil para clientes MCP.*

---

## 4. Primeira Autenticação e Login no Bling

Ao iniciar o seu cliente local de IA, o servidor `Bling.Mcp.exe` começará a rodar em *Background* (segundo plano) e fará a validação se já existe algum token configurado e salvo.

Caso seja a 1ª execução e a autenticação seja exigida:
1. O servidor MCP irá tentar invocar automaticamente a janela do seu **Navegador Padrão**.
2. Uma nova aba exigirá a autorização do OAuth2 do Bling (`https://www.bling.com.br/Api/v3/oauth/authorize...`).
3. Entre com a sua conta caso não logado e autorize o seu Aplicativo.
4. O Bling fará o redirecionamento com o código com sucesso para `localhost:8087`, e sua tela vai exibir que a autorização foi concluída com sucesso.
5. Feche a aba do navegador.
6. Pronto! A partir de agora os próximos fluxos de token e *Refresh Token* ocorrem de modo invisível com segurança. O seu assistente de IA agora tem todas as ferramentas à sua disposição para conversar em tempo real com seu ERP.

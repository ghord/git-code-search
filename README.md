# git-code-search

Windows tool for searching git repositories


## Function - MCP server

Git Code Search can expose the configured repositories to LLM clients through a local [MCP](https://modelcontextprotocol.io) server.
Enable it in **Settings > MCP server** (choose a port, default 3333). 
It listens on localhost only and runs while the application is running.

Tools: 
- `list_repositories`
- `get_current_branch`
- `search_file_content`
- `search_commit_messages`
- `get_file_content`

Client configuration:

Claude Code (`.mcp.json`):

```json
{ "mcpServers": { "git-code-search": { "type": "http", "url": "http://localhost:3333/mcp" } } }
```

Cursor (`~/.cursor/mcp.json` for all projects, or `.cursor/mcp.json` in a project):

```json
{ "mcpServers": { "git-code-search": { "url": "http://localhost:3333/mcp" } } }
```

### Making agents use it

The server sends usage instructions to the client, so agents know to try it when something is not found in the current project.
For more reliable behaviour, add a rule to your client (Cursor rules, `CLAUDE.md`, ...):

```
If a file, class or usage is not found in the current project, search the user's other repositories
and branches with the git-code-search MCP (search_file_content, then get_file_content).
```

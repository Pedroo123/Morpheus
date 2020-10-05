using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace botzaoDaMassa.comandos
{
    class CommandHandler
    {
        //Seta os campos para serem usados depois no construtor
        private IConfiguration _config;
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            //Executando a ação quando executado um comando
            _commands.CommandExecuted += CommandExecutedAsync;

            //Executar ação quando mensagem for recebida (verificar se é um comando valido)
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            //Registra os modulos que sao assincronos e herda a classe ModuleBase<T>
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            //Validando se a mensagem processada é mesmo de um usuário, e nao de um bot ou do sistema
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            //Ajusta a posição do argumento
            var argPos = 0;

            //Buscar o prefixo do arquivo de configuração
            char prefix = Char.Parse(_config["Prefix"]);

            //Determina se a mensagem possui um prefixo valido
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);

            //Executa o comando se as condições acima forem aceitas
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
           //Validação para caso o comando nao for encontrado
           if (!command.IsSpecified)
           {
                return;
           }

           if (result.IsSuccess)
           {
                return;
           }

            //Cenario de falha (Fetch data, ou erro no servidor do bot)
            await context.Channel.SendMessageAsync($"Sorry {context.User.Username}.... something went wrong -> [{result}]!");
        }
    }
}

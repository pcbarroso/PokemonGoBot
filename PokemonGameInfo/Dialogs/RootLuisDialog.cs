using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using PokeAPI;
using PokemonGameInfo.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PokemonGameInfo.Dialogs
{
    [LuisModel(Constants.LuisIdRoot, Constants.LuisSubscriptionKey)]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        [LuisIntent("QualTipo")]
        public async Task RootAsync(IDialogContext context, LuisResult result)
        {
           // PokemonSpecies p  = await DataFetcher.GetApiObject<PokemonSpecies>(001);
            
            if (result.Entities.Any())
            {
                string PokemonName = null;
                foreach (var e in result.Entities)
                {
                    if("PokemonName".Equals(e.Type,StringComparison.InvariantCultureIgnoreCase))
                    {
                        PokemonName = e.Entity;
                        await context.PostAsync($"Você quer saber o Tipo de: {PokemonName}");
                    }
                }

                if(PokemonName != null)
                {
                    try
                    {
                        PokemonSpecies p = await DataFetcher.GetNamedApiObject<PokemonSpecies>(PokemonName);
                        if(p != null)
                        {
                            await context.PostAsync($"O Tipo de {p.Name} é {p.Genera[0].Name}");
                        }
                    }
                    catch (Exception e)
                    {

                    }

                }
            }
            else
            {
                await context.PostAsync("Não entendi qual você procura");
            }
            
            context.Wait(MessageReceived);
        }

        private string userToBotText;
        protected async override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            userToBotText = (await item).Text;
            await base.MessageReceived(context, item);
        }
    }
}
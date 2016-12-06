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
                    }
                }

                if(PokemonName != null)
                {
                    Dictionary<int, double> Pokemons = Utils.FindPokemonSimilarities(PokemonName, 10);
                    if(Pokemons.First().Value > 0.85 && (Pokemons.First().Value - Pokemons.ElementAt(1).Value) > 0.2)
                    {
                        string PkmnName = "";
                        if(Constants.PokemonNames.TryGetValue(Pokemons.First().Key,out PkmnName))
                        {
                            await GetPkmnType(context, PkmnName);
                        }
                        context.Wait(MessageReceived);
                    }
                    else
                    {
                        List<string> Pkmns = new List<string>();
                        foreach (var i in Pokemons)
                        {
                            string CurName = "";
                            if(i.Value >= 0.6)
                            {
                                if(Constants.PokemonNames.TryGetValue(i.Key,out CurName))
                                {
                                    Pkmns.Add(CurName);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        PromptDialog.Choice(context, CallbackSelPkmn, Pkmns,"Qual desses Pokémons você quis dizer?");
                     }
                }
            }
            else
            {
                await context.PostAsync("Não entendi qual você procura");
                context.Wait(MessageReceived);
            }
        }

        private async Task CallbackSelPkmn(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string text = await result;
                await GetPkmnType(context, text);
            }
            catch (TooManyAttemptsException)
            {
                context.Wait(MessageReceived);
            }
        }

        private async Task GetPkmnType(IDialogContext context, string PkmnName)
        {
            try
            {
                List<string> Types = new List<string>();
                Pokemon p = await DataFetcher.GetNamedApiObject<Pokemon>(PkmnName);
                if (p != null)
                {
                    string Tipos = "";
                    foreach (var tp in p.Types)
                    {
                        Types.Add(tp.Type.Name);
                        Tipos += tp.Type.Name;
                        Tipos += " ";
                    }
                    //string Tipos = "Flying Dragon "; // Para testes no Proxy
                    await context.PostAsync($"Tipo(s) de {PkmnName}: {Tipos.Trim()}");
                }
            }
            catch (Exception e)
            {
                await context.PostAsync($"[GetPkmnType]Ocorreu um erro: {e.Message}");
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
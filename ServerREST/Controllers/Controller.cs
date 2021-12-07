using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerREST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        static int entity_pool = 0;
        static Dictionary<int, Player> entities = new Dictionary<int, Player>();

        //client id and pending updates
        static Dictionary<int, Stack<int>> clients = new Dictionary<int, Stack<int>>();

        //send changed entities to client
        [HttpGet("csync")]
        public Dictionary<int, Player> ClientSync(int client)
        {
            //items which particular client will receive
            Dictionary<int, Player> changes = new Dictionary<int, Player>();

            //check if the client is authorized
            if(clients.TryGetValue(client, out Stack<int> changed_items))
            {
                //add changed items and mark them as sent by removing
                while (changed_items.Count > 0)
                {
                    var item = changed_items.Pop();
                    if(!changes.ContainsKey(item))
                        changes.Add(item, entities[item]);
                }
                if(changes.Count > 0)
                    Console.WriteLine("Client " + client + " gets " + changes.Count + " changes");
            }
            else
                Console.WriteLine("Client " + client +  " not authorized!");

            return changes;
        }

        [HttpGet("auth")]
        public int AuthorizeClient()
        {
            //create new id
            int clientid = clients.Count + 1;
            //add client to the list
            clients.Add(clientid, new Stack<int>());
            //pass every object to by fetched in the next csync 
            clients[clientid] = new Stack<int>(entities.Keys);

            Console.WriteLine("Client " + clientid + " authorized");
            return clientid;
        }

        [HttpGet("new")]
        public int NewEntity()
        {
            //reserve new entity id
            entity_pool++;
            entities.Add(entity_pool, new Player());

            return entity_pool;
        }

        [HttpPost("ssync")]
        public void ServerSync(Upload items)
        {
            if (items != null)
            {      
                if(items.changes.Count > 0)
                    Console.WriteLine("Received " + items.changes.Count + " changes from client " + items.client_id);

                foreach (var item in items.changes)
                {
                    if (entities.ContainsKey(item.Key))
                    {
                        entities[item.Key] = item.Value;
                        foreach (var client in clients)
                        {
                            if(client.Key != items.client_id)
                                client.Value.Push(item.Key);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Entity '" + item.Key + "' does not exist!");
                    }
                }
            }
        }
    }
}

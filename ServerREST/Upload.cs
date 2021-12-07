using System.Collections.Generic;

namespace ServerREST
{
    public class Upload
    {
        public int client_id { get; set; }
        public Dictionary<int, Player> changes { get; set; }
    }
}
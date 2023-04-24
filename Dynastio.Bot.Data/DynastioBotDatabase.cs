using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    public class DynastioBotDatabase
    {

        public DynastioBotDatabase()
        {

        }
        public enum DatabasesInstances { Mongodb }
        public async Task<IDynastioBotDatabase> GetInstanseAsync(string connection, DatabasesInstances instances = DatabasesInstances.Mongodb)
        {
            if (instances is DatabasesInstances.Mongodb)
            {
                IDynastioBotDatabase dbContext = new MongoDbContext(connection);
                await dbContext.InitializeAsync();
                return dbContext;
            }
            return await Task.FromResult<IDynastioBotDatabase>(null);
        }
    }
}

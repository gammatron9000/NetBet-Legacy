using System;
using Raven.Client;
using Raven.Client.Document;

namespace NetBet
{
    public class RavenDocStore
    {
        private static Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateDocStore);

        public static IDocumentStore Store
        {
            get { return store.Value; }
        }

        private static IDocumentStore CreateDocStore()
        {
            return new DocumentStore
            {
                Url = Settings.RavenUrl,
                DefaultDatabase = Settings.RavenDatabase
            }.Initialize();
        }

    }
}
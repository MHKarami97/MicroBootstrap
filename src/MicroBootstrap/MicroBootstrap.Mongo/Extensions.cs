using MicroBootstrap.Types;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MicroBootstrap.Mongo
{
    public static class Extensions
    {
        private const string SectionName = "mongo";
        public static IServiceCollection AddMongo(this IServiceCollection serviceCollection, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            var mongoOptions = serviceCollection.GetOptions<MongoDbOptions>(sectionName);
            serviceCollection.AddSingleton(mongoOptions);

            serviceCollection.AddSingleton<IMongoClient>(sp =>
            {
                return new MongoClient(mongoOptions.ConnectionString);
            });

            serviceCollection.AddTransient(sp =>
            {
                var options = sp.GetService<MongoDbOptions>();
                var client = sp.GetService<IMongoClient>();
                return client.GetDatabase(options.Database);
            });
            serviceCollection.AddTransient<IMongoDbInitializer, MongoDbInitializer>();
            serviceCollection.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

            serviceCollection.AddTransient<IMongoDbSeeder, MongoDbSeeder>();
            RegisterConventions();

            return serviceCollection;
        }
        private static void RegisterConventions()
        {
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?),
                new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            ConventionRegistry.Register("conventions", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            }, _ => true);
        }
        public static IServiceCollection AddMongoRepository<TEntity, TIdentifiable>(this IServiceCollection serviceCollection,
         string collectionName) where TEntity : IIdentifiable<TIdentifiable>
        {
            serviceCollection.AddTransient<IMongoRepository<TEntity, TIdentifiable>>(sp =>
            {
                var database = sp.GetService<IMongoDatabase>();
                return new MongoRepository<TEntity, TIdentifiable>(database, collectionName);
            });
            return serviceCollection;
        }
    }
}
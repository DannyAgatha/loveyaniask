using System;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.DbServer.TimeSpaceService;
using WingsAPI.Communication.DbServer.WarehouseService;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Families.Warehouse;
using WingsAPI.Communication.Mail;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.Relation;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.Services;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Translations;

namespace WingsEmu.Communication.gRPC.Extensions
{
    public static class ServiceEnvironmentConsts
    {
        public const string MASTER_IP = "MASTER_IP";
        public const string MASTER_PORT = "MASTER_PORT";

        public const string BAZAAR_SERVER_IP = "BAZAAR_SERVER_IP";
        public const string BAZAAR_SERVER_PORT = "BAZAAR_SERVER_PORT";

        public const string RELATION_SERVER_IP = "RELATION_SERVER_IP";
        public const string RELATION_SERVER_PORT = "RELATION_SERVER_PORT";

        public const string FAMILY_SERVER_IP = "FAMILY_SERVER_IP";
        public const string FAMILY_SERVER_PORT = "FAMILY_SERVER_PORT";

        public const string MAIL_SERVER_IP = "MAIL_SERVER_IP";
        public const string MAIL_SERVER_PORT = "MAIL_SERVER_PORT";

        public const string DB_SERVER_IP = "DB_SERVER_IP";
        public const string DB_SERVER_PORT = "DB_SERVER_PORT";

        public const string TRANSLATION_SERVER_IP = "TRANSLATIONS_SERVER_IP";
        public const string TRANSLATION_SERVER_PORT = "TRANSLATIONS_SERVER_PORT";
    }

    public static class ServiceProviderExtensions
    {
        private const string DEFAULT_IP = "localhost";
        private const string DEFAULT_PORT = "20500";

        private static void AddGrpcService<T>(this IServiceCollection services, string ipEnvironmentVariable, string portEnvironmentVariable, string portDefault = null) where T : class
        {
            services.AddScoped(s =>
            {
                string ip = Environment.GetEnvironmentVariable(ipEnvironmentVariable) ?? DEFAULT_IP;
                int port = Convert.ToInt32(Environment.GetEnvironmentVariable(portEnvironmentVariable) ?? portDefault ?? DEFAULT_PORT);
                var options = new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = null,
                    MaxSendMessageSize = null
                };
                T generatedService = GrpcChannel.ForAddress($"http://{ip}:{port}", options).CreateGrpcService<T>();
                return generatedService;
            });
        }

        public static void AddServerApiServiceClient(this IServiceCollection services)
        {
            services.AddGrpcService<IServerApiService>(ServiceEnvironmentConsts.MASTER_IP, ServiceEnvironmentConsts.MASTER_PORT);
        }

        public static void AddGrpcSessionServiceClient(this IServiceCollection services)
        {
            services.AddGrpcService<ISessionService>(ServiceEnvironmentConsts.MASTER_IP, ServiceEnvironmentConsts.MASTER_PORT);
        }

        public static void AddGrpcClusterStatusServiceClient(this IServiceCollection services)
        {
            services.AddGrpcService<IClusterStatusService>(ServiceEnvironmentConsts.MASTER_IP, ServiceEnvironmentConsts.MASTER_PORT);
        }

        public static void AddClusterCharacterServiceClient(this IServiceCollection services)
        {
            services.AddGrpcService<IClusterCharacterService>(ServiceEnvironmentConsts.MASTER_IP, ServiceEnvironmentConsts.MASTER_PORT);
        }

        public static void AddTranslationsGrpcClient(this IServiceCollection services)
        {
            const string defaultPort = "19999";
            services.AddGrpcService<ITranslationService>(ServiceEnvironmentConsts.TRANSLATION_SERVER_IP, ServiceEnvironmentConsts.TRANSLATION_SERVER_PORT, defaultPort);
        }

        public static void AddGrpcRelationServiceClient(this IServiceCollection services)
        {
            const string defaultPort = "21111";
            services.AddGrpcService<IRelationService>(ServiceEnvironmentConsts.RELATION_SERVER_IP, ServiceEnvironmentConsts.RELATION_SERVER_PORT, defaultPort);
        }

        public static void AddGrpcBazaarServiceClient(this IServiceCollection services)
        {
            const string defaultPort = "25555";
            services.AddGrpcService<IBazaarService>(ServiceEnvironmentConsts.BAZAAR_SERVER_IP, ServiceEnvironmentConsts.BAZAAR_SERVER_PORT, defaultPort);
        }

        public static void AddGrpcFamilyServiceClient(this IServiceCollection services)
        {
            const string defaultPort = "26666";
            services.AddGrpcService<IFamilyService>(ServiceEnvironmentConsts.FAMILY_SERVER_IP, ServiceEnvironmentConsts.FAMILY_SERVER_PORT, defaultPort);
            services.AddGrpcService<IFamilyInvitationService>(ServiceEnvironmentConsts.FAMILY_SERVER_IP, ServiceEnvironmentConsts.FAMILY_SERVER_PORT, defaultPort);
            services.AddGrpcService<IFamilyWarehouseService>(ServiceEnvironmentConsts.FAMILY_SERVER_IP, ServiceEnvironmentConsts.FAMILY_SERVER_PORT, defaultPort);
        }

        public static void AddGrpcMailServiceClient(this IServiceCollection services)
        {
            const string defaultPort = "27777";
            services.AddGrpcService<IMailService>(ServiceEnvironmentConsts.MAIL_SERVER_IP, ServiceEnvironmentConsts.MAIL_SERVER_PORT, defaultPort);
            services.AddGrpcService<INoteService>(ServiceEnvironmentConsts.MAIL_SERVER_IP, ServiceEnvironmentConsts.MAIL_SERVER_PORT, defaultPort);
        }

        public static void AddGrpcDbServerServiceClient(this IServiceCollection services)
        {
            const string defaultPort = "29999";
            services.AddGrpcService<ICharacterService>(ServiceEnvironmentConsts.DB_SERVER_IP, ServiceEnvironmentConsts.DB_SERVER_PORT, defaultPort);
            services.AddGrpcService<IAccountWarehouseService>(ServiceEnvironmentConsts.DB_SERVER_IP, ServiceEnvironmentConsts.DB_SERVER_PORT, defaultPort);
            services.AddGrpcService<ITimeSpaceService>(ServiceEnvironmentConsts.DB_SERVER_IP, ServiceEnvironmentConsts.DB_SERVER_PORT, defaultPort);
            services.AddGrpcService<IAccountService>(ServiceEnvironmentConsts.DB_SERVER_IP, ServiceEnvironmentConsts.DB_SERVER_PORT, defaultPort);
        }
    }
}
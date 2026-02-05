using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Services;
using WingsAPI.Communication.Services.Messages;
using WingsAPI.Communication.Services.Requests;
using WingsAPI.Communication.Services.Responses;
using WingsEmu.Health;

namespace Master.Services.Maintenance
{
    public class GrpcClusterStatusService : IClusterStatusService
    {
        private readonly IMessagePublisher<ServiceMaintenanceActivateMessage> _activateMaintenancePublisher;
        private readonly IMessagePublisher<ServiceMaintenanceDeactivateMessage> _deactivateMaintenancePublisher;
        private readonly IStatusManager _maintenanceManager;
        private readonly IMessagePublisher<ServiceMaintenanceNotificationMessage> _maintenanceNotificationPublisher;

        private readonly TimeSpan[] _scheduledShutdownMessages =
        {
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(1), TimeSpan.FromHours(2), TimeSpan.FromHours(4), TimeSpan.FromHours(8), TimeSpan.FromHours(16),
            TimeSpan.FromDays(1)
        };

        private readonly IMessagePublisher<ServiceFlushAllMessage> _serviceFlushAllPublisher;
        private readonly IMessagePublisher<ServiceKickAllMessage> _serviceKickAllPublisher;

        private bool _maintenanceApplied;

        private CancellationTokenSource _scheduledShutdownTokenSource;

        public GrpcClusterStatusService(IStatusManager maintenanceManager, IMessagePublisher<ServiceMaintenanceActivateMessage> activateMaintenancePublisher,
            IMessagePublisher<ServiceMaintenanceDeactivateMessage> deactivateMaintenancePublisher, IMessagePublisher<ServiceMaintenanceNotificationMessage> maintenanceNotificationPublisher,
            IMessagePublisher<ServiceFlushAllMessage> serviceFlushAllPublisher, IMessagePublisher<ServiceKickAllMessage> serviceKickAllPublisher)
        {
            _maintenanceManager = maintenanceManager;
            _activateMaintenancePublisher = activateMaintenancePublisher;
            _deactivateMaintenancePublisher = deactivateMaintenancePublisher;
            _maintenanceNotificationPublisher = maintenanceNotificationPublisher;
            _serviceFlushAllPublisher = serviceFlushAllPublisher;
            _serviceKickAllPublisher = serviceKickAllPublisher;
        }

        public Task<ServiceGetAllResponse> GetAllServicesStatus(EmptyRpcRequest req)
        {
            IReadOnlyList<ServiceStatus> services = _maintenanceManager.GetAllServicesStatus();
            IEnumerable<Service> tmp = services.Select(s => new Service
            {
                Id = s.ServiceName,
                Status = (ServiceHealthStatus)s.Status,
                LastUpdate = s.LastUpdate
            });
            return Task.FromResult(new ServiceGetAllResponse
            {
                Services = tmp.ToList()
            });
        }

        public async Task<ServiceGetStatusByNameResponse> GetServiceStatusByNameAsync(ServiceBasicRequest req)
        {
            ServiceStatus service = _maintenanceManager.GetServiceByName(req.ServiceName);
            if (service == null)
            {
                return new ServiceGetStatusByNameResponse { ResponseType = RpcResponseType.GENERIC_SERVER_ERROR };
            }

            return new ServiceGetStatusByNameResponse
            {
                ResponseType = RpcResponseType.SUCCESS, Service = new Service
                {
                    Id = service.ServiceName,
                    Status = (ServiceHealthStatus)service.Status,
                    LastUpdate = service.LastUpdate
                }
            };
        }

        public async Task<BasicRpcResponse> EnableMaintenanceMode(ServiceBasicRequest req)
        {
            string serviceName = req.ServiceName;
            ServiceStatus serviceStatus = _maintenanceManager.GetServiceByName(serviceName);

            if (serviceStatus.Status != ServiceStatusType.ONLINE)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.GENERIC_SERVER_ERROR };
            }

            await _activateMaintenancePublisher.PublishAsync(new ServiceMaintenanceActivateMessage
            {
                TargetServiceName = serviceName
            });
            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        public async Task<BasicRpcResponse> DisableMaintenanceMode(ServiceBasicRequest req)
        {
            string serviceName = req.ServiceName;
            ServiceStatus serviceStatus = _maintenanceManager.GetServiceByName(serviceName);

            if (serviceStatus.Status != ServiceStatusType.UNDER_MAINTENANCE)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.GENERIC_SERVER_ERROR };
            }

            await _deactivateMaintenancePublisher.PublishAsync(new ServiceMaintenanceDeactivateMessage
            {
                TargetServiceName = serviceName
            });
            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        public async Task<BasicRpcResponse> ScheduleGeneralMaintenance(ServiceScheduleGeneralMaintenanceRequest maintenanceRequest)
        {
            if (_maintenanceApplied)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.MAINTENANCE_MODE };
            }

            bool rescheduled = _scheduledShutdownTokenSource != null;
            if (rescheduled)
            {
                _scheduledShutdownTokenSource.Cancel();
                _scheduledShutdownTokenSource.Dispose();
            }

            _scheduledShutdownTokenSource = new CancellationTokenSource();

            Task.Run(() => ScheduleGeneralMaintenance(maintenanceRequest.ShutdownTimeSpan, _scheduledShutdownTokenSource.Token));

            await _maintenanceNotificationPublisher.PublishAsync(new ServiceMaintenanceNotificationMessage
            {
                NotificationType = rescheduled ? ServiceMaintenanceNotificationType.Rescheduled : ServiceMaintenanceNotificationType.ScheduleWarning,
                Reason = maintenanceRequest.Reason,
                TimeLeft = maintenanceRequest.ShutdownTimeSpan
            }, _scheduledShutdownTokenSource.Token);

            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        public async Task<BasicRpcResponse> UnscheduleGeneralMaintenance(EmptyRpcRequest emptyRpcRequest)
        {
            if (_maintenanceApplied)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.MAINTENANCE_MODE };
            }

            if (_scheduledShutdownTokenSource == null)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.GENERIC_SERVER_ERROR };
            }

            _scheduledShutdownTokenSource.Cancel();
            _scheduledShutdownTokenSource.Dispose();
            _scheduledShutdownTokenSource = null;

            await _maintenanceNotificationPublisher.PublishAsync(new ServiceMaintenanceNotificationMessage
            {
                NotificationType = ServiceMaintenanceNotificationType.ScheduleStopped
            });

            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        public async Task<BasicRpcResponse> ExecuteGeneralEmergencyMaintenance(ServiceExecuteGeneralEmergencyMaintenanceRequest shutdownRequest)
        {
            if (_maintenanceApplied)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.MAINTENANCE_MODE };
            }

            if (_scheduledShutdownTokenSource != null)
            {
                _scheduledShutdownTokenSource.Cancel();
                _scheduledShutdownTokenSource.Dispose();
                _scheduledShutdownTokenSource = null;
            }

            await ExecuteGeneralMaintenance(true, shutdownRequest.Reason);

            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        public async Task<BasicRpcResponse> LiftGeneralMaintenance(EmptyRpcRequest shutdownRequest)
        {
            if (!_maintenanceApplied)
            {
                return new BasicRpcResponse { ResponseType = RpcResponseType.MAINTENANCE_MODE };
            }

            await LiftGeneralMaintenance();

            return new BasicRpcResponse { ResponseType = RpcResponseType.SUCCESS };
        }

        private async Task ScheduleGeneralMaintenance(TimeSpan shutdownTimeSpan, CancellationToken cancellationToken)
        {
            DateTime shutdownDateTime = DateTime.UtcNow + shutdownTimeSpan;
            int i;

            //Remove non needed messages
            for (i = _scheduledShutdownMessages.Length - 1; i >= 0; i--)
            {
                if (shutdownTimeSpan <= _scheduledShutdownMessages[i])
                {
                    continue;
                }

                break;
            }

            //Send notification messages until we run out of them
            while (i >= 0)
            {
                TimeSpan timeLeft = shutdownDateTime - DateTime.UtcNow;
                TimeSpan message = _scheduledShutdownMessages[i];

                if (timeLeft <= message)
                {
                    await _maintenanceNotificationPublisher.PublishAsync(new ServiceMaintenanceNotificationMessage
                    {
                        NotificationType = ServiceMaintenanceNotificationType.ScheduleWarning,
                        TimeLeft = message
                    }, cancellationToken);

                    i--;
                    continue;
                }

                await Task.Delay(500, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            //Start shutdown
            await ExecuteGeneralMaintenance(false);
        }

        private async Task ExecuteGeneralMaintenance(bool emergency, string reason = null)
        {
            _maintenanceApplied = true;

            await _activateMaintenancePublisher.PublishAsync(new ServiceMaintenanceActivateMessage
            {
                IsGlobal = true
            });

            await _serviceKickAllPublisher.PublishAsync(new ServiceKickAllMessage
            {
                IsGlobal = true
            });

            await Task.Delay(5000);

            await _serviceFlushAllPublisher.PublishAsync(new ServiceFlushAllMessage
            {
                IsGlobal = true
            });

            await _maintenanceNotificationPublisher.PublishAsync(new ServiceMaintenanceNotificationMessage
            {
                NotificationType = emergency ? ServiceMaintenanceNotificationType.EmergencyExecuted : ServiceMaintenanceNotificationType.Executed,
                Reason = reason
            });
        }

        private async Task LiftGeneralMaintenance()
        {
            await _deactivateMaintenancePublisher.PublishAsync(new ServiceMaintenanceDeactivateMessage
            {
                IsGlobal = true
            });

            await _maintenanceNotificationPublisher.PublishAsync(new ServiceMaintenanceNotificationMessage
            {
                NotificationType = ServiceMaintenanceNotificationType.Lifted
            });

            _maintenanceApplied = false;
        }
    }
}
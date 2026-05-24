using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.DTOs.Dashboard;
using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Interfaces;

public interface IDashboardService
{
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(CancellationToken cancellationToken = default);
    Task<SupportQueueDashboardDto> GetSupportQueueDashboardAsync(CancellationToken cancellationToken = default);
    Task<ManagerDashboardDto> GetManagerDashboardAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<TicketListItemDto>> SearchTicketsAsync(TicketQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<WorkloadReportDto> GetWorkloadReportAsync(CancellationToken cancellationToken = default);
    Task<TicketAgingReportDto> GetTicketAgingReportAsync(CancellationToken cancellationToken = default);
    Task<int> BulkAssignTicketsAsync(BulkAssignTicketsRequestDto request, CancellationToken cancellationToken = default);
}

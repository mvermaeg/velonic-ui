using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Fraud
{
    public interface ILeadFraudService
    {
        Task CheckAndApplyAsync(Lead lead);
    }
}
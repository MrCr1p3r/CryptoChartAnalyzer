using FluentResults;
using SVC_Coins.ApiContracts.Requests;
using SVC_Coins.ApiContracts.Requests.CoinCreation;

namespace SVC_Coins.Services.Validators.Interfaces;

/// <summary>
/// Defines the contract for validating coin-related data.
/// </summary>
public interface ICoinsValidator
{
    /// <summary>
    /// Validates a collection of coin creation requests.
    /// </summary>
    /// <param name="requests">Collection of coin creation requests to validate.</param>
    /// <returns>
    /// Success: Empty success result.
    /// Failure: List of validation errors.
    /// </returns>
    Task<Result> ValidateCoinCreationRequests(IEnumerable<CoinCreationRequest> requests);

    /// <summary>
    /// Validates a collection of quote coin creation requests.
    /// </summary>
    /// <param name="requests">Collection of quote coin creation requests to validate.</param>
    /// <returns>
    /// Success: Empty success result.
    /// Failure: List of validation errors.
    /// </returns>
    Task<Result> ValidateQuoteCoinCreationRequests(IEnumerable<QuoteCoinCreationRequest> requests);

    /// <summary>
    /// Validates market data update requests.
    /// </summary>
    /// <param name="requests">Collection of coin market data update requests to validate.</param>
    /// <returns>
    /// Success: Empty success result.
    /// Failure: List of validation errors.
    /// </returns>
    Task<Result> ValidateMarketDataUpdateRequests(
        IEnumerable<CoinMarketDataUpdateRequest> requests
    );
}

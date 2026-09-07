using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);
        Task<Transaction> AddTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId);
    }
}

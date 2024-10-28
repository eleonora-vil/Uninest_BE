using BE_EXE201.Dtos;
using BE_EXE201.Entities;
using BE_EXE201.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BE_EXE201.Services
{

    public class DashboardServices
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IRepository<PaymentTransaction, int> _paymentTransactionRepository;
        private readonly IRepository<Home, int> _homeRepository;


        public DashboardServices(IRepository<User, int> userRepository, IRepository<PaymentTransaction, int> paymentTransactionRepository,
            IRepository<Home, int> homeRepository)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
            _userRepository = userRepository;
            _homeRepository = homeRepository;
        }

        public async Task<int> GetTotalUsersCount()
        {
            return await _userRepository.CountAsync();
        }

        public async Task<int> GetTotalHomePosts()
        {
            return await _homeRepository.CountAsync();
        }
        public async Task<int> GetTotalTransactions()
        {
            return await _paymentTransactionRepository.CountAsync();
        }

        public async Task<decimal> GetTotalEarningsFromActiveTransactions()
        {
            Expression<Func<PaymentTransaction, bool>> predicate = pt => pt.Status == "PAID";
            Expression<Func<PaymentTransaction, decimal>> selector = pt => pt.Amount;

            return await _paymentTransactionRepository.SumAsync(predicate, selector);
        }

        public async Task<IEnumerable<object>> GetWeeklyTransactionAmounts()
        {
            var transactions = await _paymentTransactionRepository.GetLastSevenDaysTransactionsAsync();

            var groupedTransactions = transactions
                .GroupBy(t => t.CreateDate.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var result = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(day => new
                {
                    DayOfWeek = day.ToString(),
                    Amount = groupedTransactions.ContainsKey(day) ? groupedTransactions[day] : 0m
                })
                .OrderBy(x => x.DayOfWeek);

            return result;
        }

        public async Task<IEnumerable<object>> GetRecentUsers(int count = 10)
        {
            var users = await _userRepository.GetRecentUsersAsync(count);
            return users.Select(u => new
            {
                FullName = u.FullName,
                CreateDate = u.CreateDate,
                AvatarUrl = u.AvatarUrl
            });
        }
        public async Task<IEnumerable<RecentTransactionModel>> GetRecentTransactions(int count = 10)
        {
            var recentTransactions = await _paymentTransactionRepository.GetRecentTransactionsAsync(count);
            var userIds = recentTransactions.Select(t => t.UserId).Distinct().ToList();
            var users = await _userRepository.FindByCondition(u => userIds.Contains(u.UserId)).ToListAsync();

            return recentTransactions.OrderByDescending(t => t.CreateDate).Select(t => new RecentTransactionModel
            {
                TransactionId = t.TransactionId,
                Username = users.FirstOrDefault(u => u.UserId == t.UserId)?.UserName ?? "Unknown",
                Status = t.Status,
                Amount = t.Amount,
                CreateDate = t.CreateDate,
            });
        }

    }
}

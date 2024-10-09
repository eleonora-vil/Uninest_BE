using BE_EXE201.Entities;
using BE_EXE201.Repositories;
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

        public async Task<decimal> GetTotalEarningsFromActiveTransactions()
        {
            Expression<Func<PaymentTransaction, bool>> predicate = pt => pt.Status == "active";
            Expression<Func<PaymentTransaction, decimal>> selector = pt => pt.Amount;

            return await _paymentTransactionRepository.SumAsync(predicate, selector);
        }

        public async Task<IEnumerable<object>> GetTotalEarningsByDayForLastSevenDays()
        {
            Expression<Func<PaymentTransaction, bool>> predicate = pt => pt.Status == "active";
            var transactions = await _paymentTransactionRepository.GetLastSevenDaysTransactionsAsync(predicate);

            var groupedTransactions = transactions
                .GroupBy(t => t.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key.DayOfWeek.ToString(),
                    TotalEarnings = g.Sum(t => t.Amount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            var result = new List<object>();
            var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

            foreach (var day in daysOfWeek)
            {
                var earnings = groupedTransactions.FirstOrDefault(g => g.Date == day.ToString())?.TotalEarnings ?? 0;
                result.Add(new { Day = day.ToString(), TotalEarnings = earnings });
            }

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

    }
}
